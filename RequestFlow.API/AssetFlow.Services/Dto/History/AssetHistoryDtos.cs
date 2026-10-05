using AssetFlow.Services.Dto;

namespace AssetFlow.Services.Dto.History
{
    public class AssetHistoryFilterDto : SearchViewDto
    {
        public int AssetId { get; set; }
        public int? EventType { get; set; }
        public int? WorkOrderId { get; set; }
        public int? UserId { get; set; }
    }

    public class AssetHistoryEventDto
    {
        public DateTime EventAt { get; set; }
        public int EventType { get; set; }
        public string EventTypeName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ReferenceNumber { get; set; }
        public string UserName { get; set; }
        public int? WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; }
        public int? IssueId { get; set; }
        public string IssueNumber { get; set; }
        public int? PmOccurrenceId { get; set; }
        public int? PartReplacementId { get; set; }
        public int? AssetComponentId { get; set; }
        public decimal? Cost { get; set; }
        public string Currency { get; set; }
        public int? Status { get; set; }
        public string StatusName { get; set; }
    }

    public class AssetHistoryPagedDto
    {
        public List<AssetHistoryEventDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class AssetHistorySummaryDto
    {
        public int AssetId { get; set; }
        public int TotalMaintenanceEvents { get; set; }
        public int TotalBreakdowns { get; set; }
        public int TotalWorkOrders { get; set; }
        public int TotalPartsReplaced { get; set; }
        public decimal TotalDowntimeMinutes { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public decimal PartsCost { get; set; }
        public decimal LaborCost { get; set; }
        public decimal ExternalServiceCost { get; set; }
        public decimal OtherCost { get; set; }
    }

    public class AssetMaintenanceHistoryDto
    {
        public int Id { get; set; }
        public DateTime? MaintenanceDate { get; set; }
        public string MaintenanceTypeName { get; set; }
        public string ScheduleName { get; set; }
        public int? PmOccurrenceId { get; set; }
        public string ChecklistName { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string CompletedByUserName { get; set; }
        public string Remarks { get; set; }
        public int? WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; }
    }

    public class AssetBreakdownHistoryDto
    {
        public int Id { get; set; }
        public string IssueNumber { get; set; }
        public DateTime ReportedAt { get; set; }
        public string IssueCategoryName { get; set; }
        public int Priority { get; set; }
        public string Description { get; set; }
        public string ReportedByUserName { get; set; }
        public int AssetStatusAtReport { get; set; }
        public string ImmediateAction { get; set; }
        public int? WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; }
        public string Diagnosis { get; set; }
        public string RootCause { get; set; }
        public string ResolutionRemarks { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string ResolvedByUserName { get; set; }
        public decimal? DowntimeMinutes { get; set; }
    }

    public class AssetWorkOrderHistoryDto
    {
        public int Id { get; set; }
        public string WorkOrderNumber { get; set; }
        public int SourceType { get; set; }
        public string SourceTypeName { get; set; }
        public int? IssueId { get; set; }
        public string IssueNumber { get; set; }
        public int? PmOccurrenceId { get; set; }
        public string Title { get; set; }
        public int Priority { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string AssignedToUserName { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string Diagnosis { get; set; }
        public string RootCause { get; set; }
        public string WorkPerformed { get; set; }
        public string FinalResult { get; set; }
        public int PartsReplacedCount { get; set; }
        public decimal LaborCost { get; set; }
        public decimal PartsCost { get; set; }
        public decimal ExternalServiceCost { get; set; }
        public decimal OtherCost { get; set; }
        public decimal TotalCost { get; set; }
        public string Remarks { get; set; }
    }

    public class AssetPartHistoryDto
    {
        public int? PartReplacementId { get; set; }
        public int? AssetComponentId { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string SerialNumber { get; set; }
        public string ComponentName { get; set; }
        public DateTime? InstalledAt { get; set; }
        public DateTime? RemovedAt { get; set; }
        public int? LifeDays { get; set; }
        public string InstallationLocation { get; set; }
        public int? WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; }
        public string EngineerName { get; set; }
        public string RemovalReason { get; set; }
        public string FailureReason { get; set; }
        public int? ComponentStatus { get; set; }
        public bool IsActive { get; set; }
        public decimal? Cost { get; set; }
    }

    public class AssetDowntimeHistoryDto
    {
        public int AssetId { get; set; }
        public int? IssueId { get; set; }
        public string IssueNumber { get; set; }
        public int? WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; }
        public DateTime DowntimeStart { get; set; }
        public DateTime? DowntimeEnd { get; set; }
        public decimal? DurationMinutes { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
    }

    public class AssetCostHistoryDto
    {
        public int Id { get; set; }
        public DateTime CostDate { get; set; }
        public int? WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; }
        public int CostType { get; set; }
        public string CostTypeName { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string CreatedByUserName { get; set; }
    }

    public class AssetCostSummaryDto
    {
        public decimal TotalMaintenanceCost { get; set; }
        public decimal PartsCost { get; set; }
        public decimal LaborCost { get; set; }
        public decimal ExternalServiceCost { get; set; }
        public decimal OtherCost { get; set; }
        public int RecordCount { get; set; }
    }

    public class MaintenanceCostUpsertDto
    {
        public int? Id { get; set; }
        public int? TenantId { get; set; }
        public int AssetId { get; set; }
        public int? WorkOrderId { get; set; }
        public int CostType { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime CostDate { get; set; }
        public int? PartReplacementId { get; set; }
        public int? PartId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitCost { get; set; }
        public string ExternalServiceDescription { get; set; }
        public string ReferenceNumber { get; set; }
        public string Notes { get; set; }
    }

    public class WorkOrderLaborUpsertDto
    {
        public int? Id { get; set; }
        public int? TenantId { get; set; }
        public int WorkOrderId { get; set; }
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal? HourlyRate { get; set; }
        public decimal? LaborCost { get; set; }
        public string Notes { get; set; }
    }

    public class WorkOrderCostSummaryDto
    {
        public int WorkOrderId { get; set; }
        public decimal LaborCost { get; set; }
        public decimal PartsCost { get; set; }
        public decimal ExternalServiceCost { get; set; }
        public decimal OtherCost { get; set; }
        public decimal TotalCost { get; set; }
    }
}
