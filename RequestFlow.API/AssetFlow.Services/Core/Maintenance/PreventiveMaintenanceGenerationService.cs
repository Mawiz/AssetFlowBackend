using AssetFlow.Common.Enum;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.Maintenance;
using AssetFlow.Data.Provider;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Maintenance;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace AssetFlow.Services.Core.Maintenance
{
    public class PreventiveMaintenanceGenerationService : IPreventiveMaintenanceGenerationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public PreventiveMaintenanceGenerationService(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<ResponseDto<GeneratePreventiveMaintenanceResultDto>> GenerateAsync(GeneratePreventiveMaintenanceDto dto)
        {
            var response = new ResponseDto<GeneratePreventiveMaintenanceResultDto> { Result = new GeneratePreventiveMaintenanceResultDto() };
            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var query = _context.MaintenanceSchedules
                .Include(x => x.Asset)
                .Where(x => x.IsActive && x.Asset.IsActive);
            query = query.Where(x => x.TenantId == tenantResult.TenantId);
            if (dto.MaintenanceScheduleId.HasValue) query = query.Where(x => x.Id == dto.MaintenanceScheduleId);

            var schedules = await query.ToListAsync();
            foreach (var schedule in schedules)
            {
                if (RecurrenceCalculator.IsMeterBased(schedule.RecurrenceType))
                    response.Result.CreatedCount += await GenerateMeterOccurrenceAsync(schedule);
                else
                    response.Result.CreatedCount += await GenerateDateOccurrencesAsync(schedule, dto.HorizonDays);
            }
            await _context.SaveChangesAsync();
            return response;
        }

        public async Task CancelFutureOpenOccurrencesAsync(int scheduleId)
        {
            var open = new[]
            {
                (int)Enums.PreventiveMaintenanceOccurrenceStatus.Upcoming,
                (int)Enums.PreventiveMaintenanceOccurrenceStatus.Due,
                (int)Enums.PreventiveMaintenanceOccurrenceStatus.Overdue
            };
            var rows = await _context.PreventiveMaintenanceOccurrences
                .Where(x => x.MaintenanceScheduleId == scheduleId && open.Contains(x.Status))
                .ToListAsync();
            foreach (var o in rows)
            {
                o.Status = (int)Enums.PreventiveMaintenanceOccurrenceStatus.Cancelled;
            }
            await _context.SaveChangesAsync();
        }

        private async Task<int> GenerateDateOccurrencesAsync(MaintenanceSchedule schedule, int? horizonOverride)
        {
            var horizonDays = horizonOverride ?? schedule.GenerationHorizonDays;
            if (horizonDays <= 0) horizonDays = 90;
            var horizon = DateTime.UtcNow.Date.AddDays(horizonDays);
            var created = 0;

            var cursor = schedule.NextDueDate?.Date ?? schedule.StartDate.Date;
            if (cursor < schedule.StartDate.Date) cursor = schedule.StartDate.Date;

            var guard = 0;
            while (cursor <= horizon && guard++ < 500)
            {
                if (schedule.EndDate.HasValue && cursor > schedule.EndDate.Value.Date) break;

                if (!await OccurrenceExistsForDateAsync(schedule.Id, cursor))
                {
                    await CreateOccurrenceAsync(schedule, cursor, cursor, null, null);
                    created++;
                }

                var next = RecurrenceCalculator.CalculateNextDueDate(cursor, schedule.StartDate, schedule.RecurrenceType, schedule.IntervalValue, schedule.DayOfWeek);
                if (next == null) break;
                cursor = next.Value.Date;
            }

            schedule.LastGeneratedDate = DateTime.UtcNow;
            schedule.NextDueDate = cursor;
            return created;
        }

        private async Task<int> GenerateMeterOccurrenceAsync(MaintenanceSchedule schedule)
        {
            if (schedule.RecurrenceType == (int)Enums.MaintenanceRecurrenceType.OperatingHours)
            {
                var threshold = schedule.NextDueOperatingHours;
                if (!threshold.HasValue)
                {
                    threshold = RecurrenceCalculator.CalculateNextMeterThreshold(await GetAssetOperatingHoursAsync(schedule.AssetId), schedule.IntervalValue);
                    schedule.NextDueOperatingHours = threshold;
                }
                if (!threshold.HasValue) return 0;
                if (await OccurrenceExistsForHoursAsync(schedule.Id, threshold.Value)) return 0;
                var dueDate = DateTime.UtcNow.Date;
                await CreateOccurrenceAsync(schedule, null, dueDate, threshold, null);
                return 1;
            }

            if (schedule.RecurrenceType == (int)Enums.MaintenanceRecurrenceType.Cycles)
            {
                var threshold = schedule.NextDueCycles;
                if (!threshold.HasValue)
                {
                    threshold = RecurrenceCalculator.CalculateNextMeterThreshold(null, schedule.IntervalValue);
                    schedule.NextDueCycles = threshold;
                }
                if (!threshold.HasValue) return 0;
                if (await OccurrenceExistsForCyclesAsync(schedule.Id, threshold.Value)) return 0;
                await CreateOccurrenceAsync(schedule, null, DateTime.UtcNow.Date, null, threshold);
                return 1;
            }
            return 0;
        }

        private async Task<decimal?> GetAssetOperatingHoursAsync(int assetId)
        {
            var sum = await _context.AssetComponents
                .Where(x => x.AssetId == assetId && x.IsActive && x.CurrentRunningHours.HasValue)
                .Select(x => x.CurrentRunningHours!.Value)
                .ToListAsync();
            return sum.Count == 0 ? null : sum.Max();
        }

        private async Task<bool> OccurrenceExistsForDateAsync(int scheduleId, DateTime scheduledDate) =>
            await _context.PreventiveMaintenanceOccurrences.AnyAsync(x =>
                x.MaintenanceScheduleId == scheduleId && x.ScheduledDate == scheduledDate);

        private async Task<bool> OccurrenceExistsForHoursAsync(int scheduleId, decimal hours) =>
            await _context.PreventiveMaintenanceOccurrences.AnyAsync(x =>
                x.MaintenanceScheduleId == scheduleId && x.DueOperatingHours == hours);

        private async Task<bool> OccurrenceExistsForCyclesAsync(int scheduleId, decimal cycles) =>
            await _context.PreventiveMaintenanceOccurrences.AnyAsync(x =>
                x.MaintenanceScheduleId == scheduleId && x.DueCycles == cycles);

        private async Task CreateOccurrenceAsync(MaintenanceSchedule schedule, DateTime? scheduledDate, DateTime dueDate, decimal? dueHours, decimal? dueCycles)
        {
            var occurrence = new PreventiveMaintenanceOccurrence
            {
                TenantId = schedule.TenantId,
                MaintenanceScheduleId = schedule.Id,
                AssetId = schedule.AssetId,
                MaintenanceTypeId = schedule.MaintenanceTypeId,
                MaintenanceChecklistId = schedule.MaintenanceChecklistId,
                ScheduledDate = scheduledDate,
                DueDate = dueDate,
                DueOperatingHours = dueHours,
                DueCycles = dueCycles,
                Status = PreventiveMaintenanceStatusHelper.ResolveOpenStatus(dueDate),
                Remarks = string.Empty
            };

            if (schedule.MaintenanceChecklistId.HasValue)
            {
                var checklist = await _context.MaintenanceChecklists
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Options)
                    .FirstOrDefaultAsync(c => c.Id == schedule.MaintenanceChecklistId);
                if (checklist != null)
                {
                    occurrence.ChecklistVersion = checklist.Version;
                    foreach (var item in checklist.Items.Where(i => !i.IsDeleted && i.IsActive).OrderBy(i => i.SortOrder))
                    {
                        var options = item.Options.Where(o => !o.IsDeleted && o.IsActive).OrderBy(o => o.SortOrder).Select(o => o.OptionText).ToList();
                        occurrence.ChecklistItems.Add(new PreventiveMaintenanceOccurrenceChecklistItem
                        {
                            SourceChecklistItemId = item.Id,
                            ItemText = item.ItemText ?? string.Empty,
                            Description = item.Description ?? string.Empty,
                            ResponseType = item.ResponseType,
                            IsRequired = item.IsRequired,
                            SortOrder = item.SortOrder,
                            OptionsJson = options.Count > 0 ? JsonSerializer.Serialize(options) : "[]",
                            IsActive = true
                        });
                    }
                }
            }

            _context.PreventiveMaintenanceOccurrences.Add(occurrence);
            schedule.LastOccurrenceDate = scheduledDate ?? dueDate;
        }
    }
}
