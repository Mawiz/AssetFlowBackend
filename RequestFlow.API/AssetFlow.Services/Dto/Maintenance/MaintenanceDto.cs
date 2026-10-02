using AssetFlow.Services.Dto;

namespace AssetFlow.Services.Dto.Maintenance
{
    public class MaintenanceTypeDto
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateMaintenanceTypeDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateMaintenanceTypeDto : CreateMaintenanceTypeDto
    {
        public int Id { get; set; }
    }

    public class MaintenanceTypeFilterDto : SearchViewDto { }

    public class MaintenanceChecklistItemOptionDto
    {
        public int Id { get; set; }
        public string OptionText { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class MaintenanceChecklistItemDto
    {
        public int Id { get; set; }
        public string ItemText { get; set; }
        public string Description { get; set; }
        public int ResponseType { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public List<MaintenanceChecklistItemOptionDto> Options { get; set; } = new();
    }

    public class MaintenanceChecklistDto
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int? MaintenanceTypeId { get; set; }
        public string MaintenanceTypeName { get; set; }
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public List<MaintenanceChecklistItemDto> Items { get; set; } = new();
    }

    public class SaveMaintenanceChecklistDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int? MaintenanceTypeId { get; set; }
        public bool IsActive { get; set; } = true;
        public List<MaintenanceChecklistItemDto> Items { get; set; } = new();
    }

    public class UpdateMaintenanceChecklistDto : SaveMaintenanceChecklistDto
    {
        public int Id { get; set; }
    }

    public class MaintenanceChecklistFilterDto : SearchViewDto
    {
        public int? MaintenanceTypeId { get; set; }
    }

    public class MaintenanceScheduleDto
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
        public int AssetId { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public int? LocationId { get; set; }
        public string LocationName { get; set; }
        public int MaintenanceTypeId { get; set; }
        public string MaintenanceTypeName { get; set; }
        public int? MaintenanceChecklistId { get; set; }
        public string ChecklistName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RecurrenceType { get; set; }
        public int IntervalValue { get; set; }
        public int? DayOfWeek { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public decimal? NextDueOperatingHours { get; set; }
        public decimal? NextDueCycles { get; set; }
        public DateTime? LastCompletedDate { get; set; }
        public int? ResponsibleUserId { get; set; }
        public string ResponsibleUserName { get; set; }
        public int GenerationHorizonDays { get; set; }
        public bool IsActive { get; set; }
        public string FrequencyDisplay { get; set; }
    }

    public class SaveMaintenanceScheduleDto
    {
        public int? TenantId { get; set; }
        public int AssetId { get; set; }
        public int MaintenanceTypeId { get; set; }
        public int? MaintenanceChecklistId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RecurrenceType { get; set; }
        public int IntervalValue { get; set; } = 1;
        public int? DayOfWeek { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? NextDueOperatingHours { get; set; }
        public decimal? NextDueCycles { get; set; }
        public int? ResponsibleUserId { get; set; }
        public int GenerationHorizonDays { get; set; } = 90;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateMaintenanceScheduleDto : SaveMaintenanceScheduleDto
    {
        public int Id { get; set; }
    }

    public class MaintenanceScheduleFilterDto : SearchViewDto
    {
        public int? AssetId { get; set; }
        public int? MaintenanceTypeId { get; set; }
        public int? LocationId { get; set; }
        public bool? ActiveOnly { get; set; }
    }

    public class PreventiveMaintenanceOccurrenceDto
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
        public int MaintenanceScheduleId { get; set; }
        public string ScheduleName { get; set; }
        public int AssetId { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public int? LocationId { get; set; }
        public string LocationName { get; set; }
        public int MaintenanceTypeId { get; set; }
        public string MaintenanceTypeName { get; set; }
        public int? MaintenanceChecklistId { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal? DueOperatingHours { get; set; }
        public decimal? DueCycles { get; set; }
        public int Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? CompletedByUserId { get; set; }
        public string CompletedByUserName { get; set; }
        public string Remarks { get; set; }
        public List<PreventiveMaintenanceOccurrenceChecklistItemDto> ChecklistItems { get; set; } = new();
    }

    public class PreventiveMaintenanceOccurrenceChecklistItemDto
    {
        public int Id { get; set; }
        public string ItemText { get; set; }
        public string Description { get; set; }
        public int ResponseType { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public List<string> Options { get; set; } = new();
        public PreventiveMaintenanceChecklistResponseDto Response { get; set; }
    }

    public class PreventiveMaintenanceChecklistResponseDto
    {
        public int Id { get; set; }
        public int OccurrenceChecklistItemId { get; set; }
        public string ResponseValue { get; set; }
        public decimal? NumericValue { get; set; }
        public string Remarks { get; set; }
    }

    public class PreventiveMaintenanceOccurrenceFilterDto : SearchViewDto
    {
        public int? AssetId { get; set; }
        public int? MaintenanceScheduleId { get; set; }
        public int? MaintenanceTypeId { get; set; }
        public int? LocationId { get; set; }
        public int? Status { get; set; }
        public bool? OverdueOnly { get; set; }
        public bool? UpcomingOnly { get; set; }
        public bool? CompletedOnly { get; set; }
        public DateTime? DueFrom { get; set; }
        public DateTime? DueTo { get; set; }
    }

    public class CompletePreventiveMaintenanceDto
    {
        public int OccurrenceId { get; set; }
        public string? Remarks { get; set; }
        public List<SubmitChecklistResponseDto> ChecklistResponses { get; set; } = new();
    }

    public class SubmitChecklistResponseDto
    {
        public int OccurrenceChecklistItemId { get; set; }
        public string? ResponseValue { get; set; }
        public decimal? NumericValue { get; set; }
        public string? Remarks { get; set; }
    }

    public class GeneratePreventiveMaintenanceDto
    {
        public int? TenantId { get; set; }
        public int? MaintenanceScheduleId { get; set; }
        public int? HorizonDays { get; set; }
    }

    public class GeneratePreventiveMaintenanceResultDto
    {
        public int CreatedCount { get; set; }
        public int CancelledCount { get; set; }
    }

    public class AssetPreventiveMaintenanceSummaryDto
    {
        public List<MaintenanceScheduleDto> ActiveSchedules { get; set; } = new();
        public PreventiveMaintenanceOccurrenceDto NextOccurrence { get; set; }
        public List<PreventiveMaintenanceOccurrenceDto> RecentOccurrences { get; set; } = new();
    }

    public class CalendarOccurrenceDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int Status { get; set; }
        public int AssetId { get; set; }
        public string AssetCode { get; set; }
    }
}
