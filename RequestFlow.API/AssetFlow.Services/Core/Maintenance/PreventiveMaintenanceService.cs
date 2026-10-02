using AssetFlow.Common.Enum;
using AssetFlow.Common.Helper;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.Maintenance;
using AssetFlow.Data.Provider;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Maintenance;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Net;
using X.PagedList;

namespace AssetFlow.Services.Core.Maintenance
{
    public class PreventiveMaintenanceService : IPreventiveMaintenanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPreventiveMaintenanceGenerationService _generationService;

        public PreventiveMaintenanceService(
            ApplicationDbContext context,
            ITenantProvider tenantProvider,
            IHttpContextAccessor httpContextAccessor,
            IPreventiveMaintenanceGenerationService generationService)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _httpContextAccessor = httpContextAccessor;
            _generationService = generationService;
        }

        public async Task<ResponseDto<object>> FilterAsync(PreventiveMaintenanceOccurrenceFilterDto model)
        {
            await RefreshOpenStatusesAsync();
            var response = new ResponseDto<object>();
            var query = BuildFilterQuery(model);
            var order = string.IsNullOrWhiteSpace(model.OrderByProp) ? "DueDate" : model.OrderByProp;
            if (model.SortDirection == (int)Enums.OrderBy.Descending) order += " descending";
            var page = await query.OrderBy(order).Select(ProjectList()).ToPagedListAsync(model.PageNumber, model.PageSize);
            response.Result = page;
            return response;
        }

        public async Task<ResponseDto<PreventiveMaintenanceOccurrenceDto>> GetByIdAsync(int id)
        {
            await RefreshOpenStatusesAsync();
            var response = new ResponseDto<PreventiveMaintenanceOccurrenceDto>();
            var dto = await MapDetailAsync(id);
            if (dto == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, dto.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<PreventiveMaintenanceOccurrenceDto>> StartAsync(int id)
        {
            var response = new ResponseDto<PreventiveMaintenanceOccurrenceDto>();
            var occurrence = await _context.PreventiveMaintenanceOccurrences.FindAsync(id);
            if (occurrence == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var startAccess = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, occurrence.TenantId);
            if (!startAccess.Ok) { response.AddError(startAccess.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            if (occurrence.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed
                || occurrence.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Cancelled)
            { response.AddError("Occurrence cannot be started."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            occurrence.Status = (int)Enums.PreventiveMaintenanceOccurrenceStatus.InProgress;
            occurrence.StartedAt = DateTime.UtcNow;
            occurrence.StartedByUserId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(id);
            return response;
        }

        public async Task<ResponseDto<PreventiveMaintenanceOccurrenceDto>> CompleteAsync(CompletePreventiveMaintenanceDto dto)
        {
            var response = new ResponseDto<PreventiveMaintenanceOccurrenceDto>();
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var occurrence = await _context.PreventiveMaintenanceOccurrences
                    .Include(x => x.ChecklistItems)
                    .FirstOrDefaultAsync(x => x.Id == dto.OccurrenceId);
                if (occurrence == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
                var completeAccess = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, occurrence.TenantId);
                if (!completeAccess.Ok) { response.AddError(completeAccess.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
                if (occurrence.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed)
                { response.AddError("Already completed."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
                if (occurrence.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Cancelled)
                { response.AddError("Occurrence is cancelled."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

                var responsesByItem = (dto.ChecklistResponses ?? new()).ToDictionary(x => x.OccurrenceChecklistItemId);
                foreach (var item in occurrence.ChecklistItems.Where(x => x.IsActive))
                {
                    responsesByItem.TryGetValue(item.Id, out var submitted);
                    var validationError = ChecklistResponseValidator.ValidateItem(item, submitted?.ResponseValue, submitted?.NumericValue);
                    if (validationError != null) { response.AddError(validationError); response.StatusCode = HttpStatusCode.BadRequest; return response; }

                    if (submitted != null && (!string.IsNullOrWhiteSpace(submitted.ResponseValue) || submitted.NumericValue.HasValue))
                    {
                        _context.PreventiveMaintenanceChecklistResponses.Add(new PreventiveMaintenanceChecklistResponse
                        {
                            PreventiveMaintenanceOccurrenceId = occurrence.Id,
                            OccurrenceChecklistItemId = item.Id,
                            ResponseValue = submitted.ResponseValue?.Trim() ?? string.Empty,
                            NumericValue = submitted.NumericValue,
                            Remarks = submitted.Remarks?.Trim() ?? string.Empty,
                            IsActive = true
                        });
                    }
                }

                occurrence.Status = (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed;
                occurrence.CompletedAt = DateTime.UtcNow;
                occurrence.CompletedByUserId = UserHelper.GetCurrentUserId(_httpContextAccessor);
                occurrence.Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? string.Empty : dto.Remarks.Trim();
                if (!occurrence.StartedAt.HasValue)
                {
                    occurrence.StartedAt = occurrence.CompletedAt;
                    occurrence.StartedByUserId = occurrence.CompletedByUserId;
                }

                var schedule = await _context.MaintenanceSchedules.FindAsync(occurrence.MaintenanceScheduleId);
                if (schedule != null)
                {
                    schedule.LastCompletedDate = occurrence.CompletedAt;
                    if (RecurrenceCalculator.IsMeterBased(schedule.RecurrenceType))
                    {
                        if (schedule.RecurrenceType == (int)Enums.MaintenanceRecurrenceType.OperatingHours && occurrence.DueOperatingHours.HasValue)
                            schedule.NextDueOperatingHours = RecurrenceCalculator.CalculateNextMeterThreshold(occurrence.DueOperatingHours, schedule.IntervalValue);
                        if (schedule.RecurrenceType == (int)Enums.MaintenanceRecurrenceType.Cycles && occurrence.DueCycles.HasValue)
                            schedule.NextDueCycles = RecurrenceCalculator.CalculateNextMeterThreshold(occurrence.DueCycles, schedule.IntervalValue);
                    }
                    else if (occurrence.ScheduledDate.HasValue)
                    {
                        schedule.NextDueDate = RecurrenceCalculator.CalculateNextDueDate(
                            occurrence.ScheduledDate, schedule.StartDate, schedule.RecurrenceType, schedule.IntervalValue, schedule.DayOfWeek);
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                if (schedule != null && schedule.IsActive)
                    await _generationService.GenerateAsync(new GeneratePreventiveMaintenanceDto { TenantId = schedule.TenantId, MaintenanceScheduleId = schedule.Id });

                response.Result = await MapDetailAsync(dto.OccurrenceId);
                return response;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<ResponseDto<PreventiveMaintenanceOccurrenceDto>> CancelAsync(int id)
        {
            var response = new ResponseDto<PreventiveMaintenanceOccurrenceDto>();
            var occurrence = await _context.PreventiveMaintenanceOccurrences.FindAsync(id);
            if (occurrence == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var cancelAccess = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, occurrence.TenantId);
            if (!cancelAccess.Ok) { response.AddError(cancelAccess.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            if (occurrence.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed)
            { response.AddError("Completed occurrence cannot be cancelled."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            occurrence.Status = (int)Enums.PreventiveMaintenanceOccurrenceStatus.Cancelled;
            await _context.SaveChangesAsync();
            response.Result = await MapDetailAsync(id);
            return response;
        }

        public async Task<ResponseDto<List<CalendarOccurrenceDto>>> GetCalendarAsync(PreventiveMaintenanceOccurrenceFilterDto model)
        {
            await RefreshOpenStatusesAsync();
            var response = new ResponseDto<List<CalendarOccurrenceDto>>();
            var query = BuildFilterQuery(model);
            var list = await query.Select(x => new CalendarOccurrenceDto
            {
                Id = x.Id,
                Title = x.Asset.AssetCode + " — " + x.MaintenanceType.Name,
                Start = x.DueDate,
                End = x.DueDate,
                Status = x.Status,
                AssetId = x.AssetId,
                AssetCode = x.Asset.AssetCode
            }).ToListAsync();
            response.Result = list;
            return response;
        }

        public async Task<ResponseDto<AssetPreventiveMaintenanceSummaryDto>> GetAssetSummaryAsync(int assetId)
        {
            var response = new ResponseDto<AssetPreventiveMaintenanceSummaryDto> { Result = new AssetPreventiveMaintenanceSummaryDto() };
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                response.AddError("Asset not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }
            var assetAccess = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, asset.TenantId);
            if (!assetAccess.Ok) { response.AddError(assetAccess.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }

            var schedules = await _context.MaintenanceSchedules
                .Include(x => x.Asset).ThenInclude(a => a.Location)
                .Include(x => x.MaintenanceType)
                .Include(x => x.MaintenanceChecklist)
                .Include(x => x.ResponsibleUser)
                .Where(x => x.AssetId == assetId && x.IsActive)
                .Select(x => new MaintenanceScheduleDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    MaintenanceTypeName = x.MaintenanceType.Name,
                    NextDueDate = x.NextDueDate,
                    FrequencyDisplay = PreventiveMaintenanceStatusHelper.FormatFrequency(x.RecurrenceType, x.IntervalValue, x.DayOfWeek),
                    IsActive = x.IsActive
                }).ToListAsync();
            response.Result.ActiveSchedules = schedules;

            var openStatuses = new[]
            {
                (int)Enums.PreventiveMaintenanceOccurrenceStatus.Upcoming,
                (int)Enums.PreventiveMaintenanceOccurrenceStatus.Due,
                (int)Enums.PreventiveMaintenanceOccurrenceStatus.Overdue,
                (int)Enums.PreventiveMaintenanceOccurrenceStatus.InProgress
            };
            response.Result.NextOccurrence = await _context.PreventiveMaintenanceOccurrences
                .Where(x => x.AssetId == assetId && openStatuses.Contains(x.Status))
                .OrderBy(x => x.DueDate)
                .Select(ProjectList())
                .FirstOrDefaultAsync();

            response.Result.RecentOccurrences = await _context.PreventiveMaintenanceOccurrences
                .Where(x => x.AssetId == assetId && x.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed)
                .OrderByDescending(x => x.CompletedAt)
                .Take(5)
                .Select(ProjectList())
                .ToListAsync();
            return response;
        }

        private IQueryable<PreventiveMaintenanceOccurrence> BuildFilterQuery(PreventiveMaintenanceOccurrenceFilterDto model)
        {
            var query = TenantScopeHelper.ApplyTenantScope(_context.PreventiveMaintenanceOccurrences.AsQueryable(), _tenantProvider, model.TenantId);
            query = query.Include(x => x.Asset).ThenInclude(a => a.Location).Include(x => x.MaintenanceType).Include(x => x.MaintenanceSchedule);
            if (model.AssetId.HasValue) query = query.Where(x => x.AssetId == model.AssetId);
            if (model.MaintenanceScheduleId.HasValue) query = query.Where(x => x.MaintenanceScheduleId == model.MaintenanceScheduleId);
            if (model.MaintenanceTypeId.HasValue) query = query.Where(x => x.MaintenanceTypeId == model.MaintenanceTypeId);
            if (model.LocationId.HasValue) query = query.Where(x => x.Asset.LocationId == model.LocationId);
            if (model.Status.HasValue) query = query.Where(x => x.Status == model.Status);
            if (model.OverdueOnly == true) query = query.Where(x => x.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Overdue);
            if (model.UpcomingOnly == true) query = query.Where(x => x.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Upcoming || x.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Due);
            if (model.CompletedOnly == true) query = query.Where(x => x.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed);
            if (model.DueFrom.HasValue) query = query.Where(x => x.DueDate >= model.DueFrom);
            if (model.DueTo.HasValue) query = query.Where(x => x.DueDate <= model.DueTo);
            if (model.StartDate.HasValue) query = query.Where(x => x.DueDate >= model.StartDate);
            if (model.EndDate.HasValue) query = query.Where(x => x.DueDate <= model.EndDate);
            if (!string.IsNullOrWhiteSpace(model.SearchText))
            {
                var s = model.SearchText.Trim();
                query = query.Where(x => x.Asset.AssetCode.Contains(s) || x.Asset.Name.Contains(s) || x.MaintenanceType.Name.Contains(s) || x.MaintenanceSchedule.Name.Contains(s));
            }
            return query;
        }

        private async Task RefreshOpenStatusesAsync()
        {
            var open = new[]
            {
                (int)Enums.PreventiveMaintenanceOccurrenceStatus.Upcoming,
                (int)Enums.PreventiveMaintenanceOccurrenceStatus.Due,
                (int)Enums.PreventiveMaintenanceOccurrenceStatus.Overdue
            };
            var rows = await _context.PreventiveMaintenanceOccurrences.Where(x => open.Contains(x.Status)).ToListAsync();
            var changed = false;
            foreach (var row in rows)
            {
                var before = row.Status;
                PreventiveMaintenanceStatusHelper.RefreshOccurrenceStatus(row);
                if (before != row.Status) changed = true;
            }
            if (changed) await _context.SaveChangesAsync();
        }

        private static System.Linq.Expressions.Expression<Func<PreventiveMaintenanceOccurrence, PreventiveMaintenanceOccurrenceDto>> ProjectList() =>
            x => new PreventiveMaintenanceOccurrenceDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,
                MaintenanceScheduleId = x.MaintenanceScheduleId,
                ScheduleName = x.MaintenanceSchedule.Name,
                AssetId = x.AssetId,
                AssetCode = x.Asset.AssetCode,
                AssetName = x.Asset.Name,
                LocationId = x.Asset.LocationId,
                LocationName = x.Asset.Location.Name,
                MaintenanceTypeId = x.MaintenanceTypeId,
                MaintenanceTypeName = x.MaintenanceType.Name,
                ScheduledDate = x.ScheduledDate,
                DueDate = x.DueDate,
                DueOperatingHours = x.DueOperatingHours,
                DueCycles = x.DueCycles,
                Status = x.Status,
                WorkOrderId = x.WorkOrderId,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                CompletedByUserId = x.CompletedByUserId,
                Remarks = x.Remarks
            };

        private async Task<PreventiveMaintenanceOccurrenceDto?> MapDetailAsync(int id)
        {
            var baseDto = await _context.PreventiveMaintenanceOccurrences
                .Include(x => x.Asset).ThenInclude(a => a.Location)
                .Include(x => x.MaintenanceType)
                .Include(x => x.MaintenanceSchedule)
                .Include(x => x.CompletedByUser)
                .Where(x => x.Id == id)
                .Select(ProjectList())
                .FirstOrDefaultAsync();
            if (baseDto == null) return null;

            var items = await _context.PreventiveMaintenanceOccurrenceChecklistItems
                .Include(x => x.Response)
                .Where(x => x.PreventiveMaintenanceOccurrenceId == id && x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
            baseDto.ChecklistItems = items.Select(x => new PreventiveMaintenanceOccurrenceChecklistItemDto
            {
                Id = x.Id,
                ItemText = x.ItemText,
                Description = x.Description,
                ResponseType = x.ResponseType,
                IsRequired = x.IsRequired,
                SortOrder = x.SortOrder,
                Options = ChecklistResponseValidator.ParseOptions(x.OptionsJson),
                Response = x.Response == null ? null : new PreventiveMaintenanceChecklistResponseDto
                {
                    Id = x.Response.Id,
                    OccurrenceChecklistItemId = x.Response.OccurrenceChecklistItemId,
                    ResponseValue = x.Response.ResponseValue,
                    NumericValue = x.Response.NumericValue,
                    Remarks = x.Response.Remarks
                }
            }).ToList();

            if (baseDto.CompletedByUserId.HasValue)
            {
                var user = await _context.ApplicationUsers.FindAsync(baseDto.CompletedByUserId.Value);
                baseDto.CompletedByUserName = user?.FullName ?? user?.UserName;
            }
            return baseDto;
        }
    }
}
