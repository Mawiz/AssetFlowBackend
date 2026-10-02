using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.Maintenance;
using AssetFlow.Data.Provider;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Maintenance;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Net;
using X.PagedList;

namespace AssetFlow.Services.Core.Maintenance
{
    public class MaintenanceScheduleService : IMaintenanceScheduleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IPreventiveMaintenanceGenerationService _generationService;

        public MaintenanceScheduleService(
            ApplicationDbContext context,
            ITenantProvider tenantProvider,
            IPreventiveMaintenanceGenerationService generationService)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _generationService = generationService;
        }

        public async Task<ResponseDto<MaintenanceScheduleDto>> CreateAsync(SaveMaintenanceScheduleDto dto)
        {
            var response = new ResponseDto<MaintenanceScheduleDto>();
            var err = await ValidateAsync(dto);
            if (err != null) { response.AddError(err); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var relatedError = await ValidateRelatedTenantsAsync(tenantResult.TenantId, dto.AssetId, dto.MaintenanceTypeId, dto.MaintenanceChecklistId);
            if (relatedError != null) { response.AddError(relatedError); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var entity = MapToEntity(new MaintenanceSchedule(), dto, tenantResult.TenantId);
            entity.NextDueDate = RecurrenceCalculator.CalculateFirstDueDate(dto.StartDate, dto.RecurrenceType, dto.IntervalValue, dto.DayOfWeek);
            if (RecurrenceCalculator.IsMeterBased(dto.RecurrenceType))
            {
                entity.NextDueOperatingHours = dto.RecurrenceType == (int)AssetFlow.Common.Enum.Enums.MaintenanceRecurrenceType.OperatingHours
                    ? dto.NextDueOperatingHours ?? RecurrenceCalculator.CalculateNextMeterThreshold(null, dto.IntervalValue)
                    : null;
                entity.NextDueCycles = dto.RecurrenceType == (int)AssetFlow.Common.Enum.Enums.MaintenanceRecurrenceType.Cycles
                    ? dto.NextDueCycles ?? RecurrenceCalculator.CalculateNextMeterThreshold(null, dto.IntervalValue)
                    : null;
            }

            _context.MaintenanceSchedules.Add(entity);
            await _context.SaveChangesAsync();
            if (entity.IsActive)
                await _generationService.GenerateAsync(new GeneratePreventiveMaintenanceDto { TenantId = entity.TenantId, MaintenanceScheduleId = entity.Id });

            response.Result = await MapAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<MaintenanceScheduleDto>> UpdateAsync(UpdateMaintenanceScheduleDto dto)
        {
            var response = new ResponseDto<MaintenanceScheduleDto>();
            var entity = await _context.MaintenanceSchedules.FindAsync(dto.Id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entity.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }

            var err = await ValidateAsync(dto);
            if (err != null) { response.AddError(err); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var relatedError = await ValidateRelatedTenantsAsync(entity.TenantId, dto.AssetId, dto.MaintenanceTypeId, dto.MaintenanceChecklistId);
            if (relatedError != null) { response.AddError(relatedError); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var recurrenceChanged = entity.RecurrenceType != dto.RecurrenceType
                || entity.IntervalValue != dto.IntervalValue
                || entity.DayOfWeek != dto.DayOfWeek
                || entity.StartDate.Date != dto.StartDate.Date;

            MapToEntity(entity, dto, entity.TenantId);
            if (recurrenceChanged && !RecurrenceCalculator.IsMeterBased(entity.RecurrenceType))
            {
                entity.NextDueDate = RecurrenceCalculator.CalculateFirstDueDate(entity.StartDate, entity.RecurrenceType, entity.IntervalValue, entity.DayOfWeek);
                await _generationService.CancelFutureOpenOccurrencesAsync(entity.Id);
            }

            await _context.SaveChangesAsync();
            if (entity.IsActive)
                await _generationService.GenerateAsync(new GeneratePreventiveMaintenanceDto { TenantId = entity.TenantId, MaintenanceScheduleId = entity.Id });

            response.Result = await MapAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<MaintenanceScheduleDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<MaintenanceScheduleDto>();
            var dto = await MapAsync(id);
            if (dto == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, dto.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<object>> FilterAsync(MaintenanceScheduleFilterDto model)
        {
            var response = new ResponseDto<object>();
            var query = TenantScopeHelper.ApplyTenantScope(_context.MaintenanceSchedules.AsQueryable(), _tenantProvider, model.TenantId);
            query = query.Include(x => x.Asset).ThenInclude(a => a.Location)
                .Include(x => x.MaintenanceType)
                .Include(x => x.MaintenanceChecklist)
                .Include(x => x.ResponsibleUser);
            if (model.IsActive.HasValue) query = query.Where(x => x.IsActive == model.IsActive);
            if (model.ActiveOnly == true) query = query.Where(x => x.IsActive);
            if (model.AssetId.HasValue) query = query.Where(x => x.AssetId == model.AssetId);
            if (model.MaintenanceTypeId.HasValue) query = query.Where(x => x.MaintenanceTypeId == model.MaintenanceTypeId);
            if (model.LocationId.HasValue) query = query.Where(x => x.Asset.LocationId == model.LocationId);
            if (!string.IsNullOrWhiteSpace(model.SearchText))
            {
                var s = model.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(s) || x.Asset.AssetCode.Contains(s) || x.Asset.Name.Contains(s));
            }
            var order = string.IsNullOrWhiteSpace(model.OrderByProp) ? "Name" : model.OrderByProp;
            if (model.SortDirection == (int)AssetFlow.Common.Enum.Enums.OrderBy.Descending) order += " descending";
            var page = await query.OrderBy(order).Select(x => new MaintenanceScheduleDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                AssetId = x.AssetId,
                AssetCode = x.Asset.AssetCode,
                AssetName = x.Asset.Name,
                LocationId = x.Asset.LocationId,
                LocationName = x.Asset.Location.Name,
                MaintenanceTypeId = x.MaintenanceTypeId,
                MaintenanceTypeName = x.MaintenanceType.Name,
                MaintenanceChecklistId = x.MaintenanceChecklistId,
                ChecklistName = x.MaintenanceChecklist != null ? x.MaintenanceChecklist.Name : null,
                Name = x.Name,
                Description = x.Description,
                RecurrenceType = x.RecurrenceType,
                IntervalValue = x.IntervalValue,
                DayOfWeek = x.DayOfWeek,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                NextDueDate = x.NextDueDate,
                NextDueOperatingHours = x.NextDueOperatingHours,
                NextDueCycles = x.NextDueCycles,
                LastCompletedDate = x.LastCompletedDate,
                ResponsibleUserId = x.ResponsibleUserId,
                ResponsibleUserName = x.ResponsibleUser != null ? (x.ResponsibleUser.FullName ?? x.ResponsibleUser.UserName) : null,
                GenerationHorizonDays = x.GenerationHorizonDays,
                IsActive = x.IsActive,
                FrequencyDisplay = PreventiveMaintenanceStatusHelper.FormatFrequency(x.RecurrenceType, x.IntervalValue, x.DayOfWeek)
            }).ToPagedListAsync(model.PageNumber, model.PageSize);
            response.Result = page;
            return response;
        }

        public async Task<ResponseDto<bool>> SetActiveAsync(int id, bool isActive)
        {
            var response = new ResponseDto<bool>();
            var entity = await _context.MaintenanceSchedules.FindAsync(id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entity.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            entity.IsActive = isActive;
            if (!isActive) await _generationService.CancelFutureOpenOccurrencesAsync(id);
            else await _generationService.GenerateAsync(new GeneratePreventiveMaintenanceDto { TenantId = entity.TenantId, MaintenanceScheduleId = id });
            await _context.SaveChangesAsync();
            response.Result = true;
            return response;
        }

        private static MaintenanceSchedule MapToEntity(MaintenanceSchedule entity, SaveMaintenanceScheduleDto dto, int? tenantId)
        {
            entity.TenantId = tenantId;
            entity.AssetId = dto.AssetId;
            entity.MaintenanceTypeId = dto.MaintenanceTypeId;
            entity.MaintenanceChecklistId = dto.MaintenanceChecklistId;
            entity.Name = dto.Name?.Trim();
            entity.Description = dto.Description;
            entity.RecurrenceType = dto.RecurrenceType;
            entity.IntervalValue = Math.Max(1, dto.IntervalValue);
            entity.DayOfWeek = dto.DayOfWeek;
            entity.StartDate = dto.StartDate.Date;
            entity.EndDate = dto.EndDate?.Date;
            entity.ResponsibleUserId = dto.ResponsibleUserId;
            entity.GenerationHorizonDays = dto.GenerationHorizonDays <= 0 ? 90 : dto.GenerationHorizonDays;
            entity.IsActive = dto.IsActive;
            entity.NextDueOperatingHours = dto.NextDueOperatingHours;
            entity.NextDueCycles = dto.NextDueCycles;
            return entity;
        }

        private Task<string?> ValidateAsync(SaveMaintenanceScheduleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) return Task.FromResult<string?>("Schedule name is required.");
            if (dto.AssetId <= 0 || dto.MaintenanceTypeId <= 0) return Task.FromResult<string?>("Asset and maintenance type are required.");
            if (dto.RecurrenceType == (int)AssetFlow.Common.Enum.Enums.MaintenanceRecurrenceType.Weekly && !dto.DayOfWeek.HasValue)
                return Task.FromResult<string?>("Day of week is required for weekly schedules.");
            return Task.FromResult<string?>(null);
        }

        private async Task<string?> ValidateRelatedTenantsAsync(int? tenantId, int assetId, int maintenanceTypeId, int? maintenanceChecklistId)
        {
            if (!tenantId.HasValue) return "Tenant is required.";
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null || !asset.IsActive) return "Asset not found or inactive.";
            if (asset.TenantId != tenantId) return "Asset does not belong to the selected tenant.";
            var mt = await _context.MaintenanceTypes.FindAsync(maintenanceTypeId);
            if (mt == null || !mt.IsActive) return "Maintenance type not found or inactive.";
            if (mt.TenantId != tenantId) return "Maintenance type does not belong to the selected tenant.";
            if (maintenanceChecklistId.HasValue)
            {
                var cl = await _context.MaintenanceChecklists.FindAsync(maintenanceChecklistId.Value);
                if (cl == null || !cl.IsActive) return "Checklist not found or inactive.";
                if (cl.TenantId != tenantId) return "Checklist does not belong to the selected tenant.";
            }
            return null;
        }

        private async Task<MaintenanceScheduleDto?> MapAsync(int id) =>
            await _context.MaintenanceSchedules
                .Include(x => x.Asset).ThenInclude(a => a.Location)
                .Include(x => x.MaintenanceType)
                .Include(x => x.MaintenanceChecklist)
                .Include(x => x.ResponsibleUser)
                .Where(x => x.Id == id).Select(x => new MaintenanceScheduleDto
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    AssetId = x.AssetId,
                    AssetCode = x.Asset.AssetCode,
                    AssetName = x.Asset.Name,
                    LocationId = x.Asset.LocationId,
                    LocationName = x.Asset.Location.Name,
                    MaintenanceTypeId = x.MaintenanceTypeId,
                    MaintenanceTypeName = x.MaintenanceType.Name,
                    MaintenanceChecklistId = x.MaintenanceChecklistId,
                    ChecklistName = x.MaintenanceChecklist != null ? x.MaintenanceChecklist.Name : null,
                    Name = x.Name,
                    Description = x.Description,
                    RecurrenceType = x.RecurrenceType,
                    IntervalValue = x.IntervalValue,
                    DayOfWeek = x.DayOfWeek,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    NextDueDate = x.NextDueDate,
                    NextDueOperatingHours = x.NextDueOperatingHours,
                    NextDueCycles = x.NextDueCycles,
                    LastCompletedDate = x.LastCompletedDate,
                    ResponsibleUserId = x.ResponsibleUserId,
                    ResponsibleUserName = x.ResponsibleUser != null ? (x.ResponsibleUser.FullName ?? x.ResponsibleUser.UserName) : null,
                    GenerationHorizonDays = x.GenerationHorizonDays,
                    IsActive = x.IsActive,
                    FrequencyDisplay = PreventiveMaintenanceStatusHelper.FormatFrequency(x.RecurrenceType, x.IntervalValue, x.DayOfWeek)
                }).FirstOrDefaultAsync();
    }
}
