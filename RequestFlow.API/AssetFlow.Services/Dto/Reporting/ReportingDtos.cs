using AssetFlow.Services.Dto;

namespace AssetFlow.Services.Dto.Reporting
{
    public class ReportingFilterDto : SearchViewDto
    {
        /// <summary>Today, ThisWeek, ThisMonth, LastMonth, ThisQuarter, ThisYear, Custom</summary>
        public string? PeriodPreset { get; set; }

        public int? AssetId { get; set; }
        public int? AssetCategoryId { get; set; }
        public int? AssetTypeId { get; set; }
        public int? LocationId { get; set; }
        public int? EngineerUserId { get; set; }
        public int? PartId { get; set; }
        public int? IssueCategoryId { get; set; }
        public int? Priority { get; set; }
    }

    public class ChartPointDto
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class DashboardSummaryDto
    {
        public AssetSummaryDto Asset { get; set; } = new();
        public MaintenanceSummaryDto Maintenance { get; set; } = new();
        public PerformanceSummaryDto Performance { get; set; } = new();
        public SparePartSummaryDto SpareParts { get; set; } = new();
        public List<ChartPointDto> BreakdownTrend { get; set; } = new();
        public List<TopFailingAssetDto> TopFailingAssets { get; set; } = new();
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
    }

    public class AssetSummaryDto
    {
        public int TotalAssets { get; set; }
        public int Operational { get; set; }
        public int UnderMaintenance { get; set; }
        public int Breakdown { get; set; }
        public int Retired { get; set; }
        public int Critical { get; set; }
        public List<ChartPointDto> StatusDistribution { get; set; } = new();
        public List<ChartPointDto> CriticalityDistribution { get; set; } = new();
        public List<ChartPointDto> ByCategory { get; set; } = new();
        public List<ChartPointDto> ByLocation { get; set; } = new();
    }

    public class MaintenanceSummaryDto
    {
        public int OpenIssues { get; set; }
        public int OpenWorkOrders { get; set; }
        public int TodaysMaintenance { get; set; }
        public int OverdueMaintenance { get; set; }
        public int CompletedToday { get; set; }
        public int WaitingForParts { get; set; }
        public decimal? PmCompliancePercent { get; set; }
        public List<ChartPointDto> WorkOrderStatusDistribution { get; set; } = new();
        public List<ChartPointDto> PmStatusDistribution { get; set; } = new();
        public List<ChartPointDto> MaintenanceCostTrend { get; set; } = new();
    }

    /// <summary>
    /// MTBF: mean hours between consecutive Issue.ReportedAt per asset (assets with 2+ failures), averaged.
    /// MTTR: mean (WorkOrder.CompletedAt - WorkOrder.StartedAt) in hours for completed repairs with both timestamps.
    /// Downtime: Issue.ReportedAt to WO restore/complete/close or Issue.ResolvedAt (merged intervals), filtered by overlap with period.
    /// First-time fix: resolved issues with no WorkOrderStatusHistory.ToStatus = Reopened on linked work orders.
    /// Response time: mean (WorkOrder.StartedAt - Issue.ReportedAt) when both exist.
    /// </summary>
    public class PerformanceSummaryDto
    {
        public decimal? MtbfHours { get; set; }
        public decimal? MttrHours { get; set; }
        public decimal TotalDowntimeMinutes { get; set; }
        public decimal? PmCompliancePercent { get; set; }
        public decimal? FirstTimeFixRatePercent { get; set; }
        public decimal? AvgResponseTimeMinutes { get; set; }
    }

    public class SparePartSummaryDto
    {
        public int TotalParts { get; set; }
        public int LowStockParts { get; set; }
        public decimal PartsConsumedInPeriod { get; set; }
        public int PartsReplacedInPeriod { get; set; }
    }

    public class TopFailingAssetDto
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public int BreakdownCount { get; set; }
        public int OpenIssues { get; set; }
        public int ClosedIssues { get; set; }
        public decimal TotalDowntimeMinutes { get; set; }
        public decimal MaintenanceCost { get; set; }
        public DateTime? LastFailureDate { get; set; }
    }

    public class RepeatedFailureDto
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public int BreakdownCount { get; set; }
    }

    public class AssetReliabilityRowDto
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int Breakdowns { get; set; }
        public decimal? MtbfHours { get; set; }
        public decimal? MttrHours { get; set; }
        public decimal DowntimeMinutes { get; set; }
        public decimal MaintenanceCost { get; set; }
        public int PartsReplaced { get; set; }
        public DateTime? LastBreakdown { get; set; }
        public DateTime? LastMaintenance { get; set; }
    }

    public class EngineerPerformanceRowDto
    {
        public int UserId { get; set; }
        public string EngineerName { get; set; } = string.Empty;
        public int AssignedJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int PendingJobs { get; set; }
        public int OverdueJobs { get; set; }
        public decimal CompletionRatePercent { get; set; }
        public decimal? AvgResponseTimeMinutes { get; set; }
        public decimal? AvgRepairTimeMinutes { get; set; }
        public int ReopenedJobs { get; set; }
        public decimal? FirstTimeFixRatePercent { get; set; }
        public int PmCompleted { get; set; }
        public int PmOverdue { get; set; }
        public int PartsUsed { get; set; }
    }

    public class AssetCostReportRowDto
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int WorkOrderCount { get; set; }
        public decimal LaborCost { get; set; }
        public decimal PartsCost { get; set; }
        public decimal ExternalServiceCost { get; set; }
        public decimal OtherCost { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
    }

    public class TopReplacedPartDto
    {
        public int PartId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public int ReplacementCount { get; set; }
        public DateTime? LastReplacementDate { get; set; }
    }

    public class PartLifeRowDto
    {
        public int PartId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string AssetCode { get; set; } = string.Empty;
        public DateTime InstalledAt { get; set; }
        public DateTime? RemovedAt { get; set; }
        public decimal? LifeDays { get; set; }
        public string FailureReason { get; set; } = string.Empty;
        public string RemovalReason { get; set; } = string.Empty;
    }

    public class PartStockReportRowDto
    {
        public int PartId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalQuantity { get; set; }
        public string Locations { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public bool IsLowStock { get; set; }
    }

    public class PartTransactionSummaryDto
    {
        public decimal QuantityReceived { get; set; }
        public decimal QuantityIssued { get; set; }
        public decimal QuantityReturned { get; set; }
        public decimal QuantityAdjusted { get; set; }
        public decimal QuantityTransferred { get; set; }
        public decimal QuantityFaulty { get; set; }
        public decimal QuantityReturnedToSupplier { get; set; }
        public decimal QuantityScrapped { get; set; }
    }

    public class PmReportSummaryDto
    {
        public int TotalScheduled { get; set; }
        public int Completed { get; set; }
        public int Upcoming { get; set; }
        public int Due { get; set; }
        public int Overdue { get; set; }
        public int Cancelled { get; set; }
        public decimal? CompliancePercent { get; set; }
        public List<ChartPointDto> CompletionTrend { get; set; } = new();
        public List<ChartPointDto> ComplianceByMonth { get; set; } = new();
    }

    public class BreakdownReportBundleDto
    {
        public int TotalBreakdowns { get; set; }
        public List<ChartPointDto> Trend { get; set; } = new();
        public List<ChartPointDto> ByCategory { get; set; } = new();
        public List<ChartPointDto> ByLocation { get; set; } = new();
        public List<ChartPointDto> ByIssueCategory { get; set; } = new();
        public List<ChartPointDto> ByPriority { get; set; } = new();
        public List<TopFailingAssetDto> TopFailingAssets { get; set; } = new();
        public List<RepeatedFailureDto> RepeatedFailures { get; set; } = new();
        public decimal TotalDowntimeMinutes { get; set; }
    }

    public class AssetReportBundleDto
    {
        public AssetSummaryDto Summary { get; set; } = new();
        public List<ChartPointDto> ByType { get; set; } = new();
        public List<AssetReliabilityRowDto> Reliability { get; set; } = new();
        public List<TopFailingAssetDto> TopFailing { get; set; } = new();
        public List<AssetCostReportRowDto> HighestCost { get; set; } = new();
    }

    public class MaintenanceReportBundleDto
    {
        public MaintenanceSummaryDto Summary { get; set; } = new();
        public PmReportSummaryDto PreventiveMaintenance { get; set; } = new();
        public decimal TotalMaintenanceCost { get; set; }
        public List<ChartPointDto> ActivityTrend { get; set; } = new();
    }

    public class SparePartsReportBundleDto
    {
        public SparePartSummaryDto Summary { get; set; } = new();
        public List<PartStockReportRowDto> Stock { get; set; } = new();
        public PartTransactionSummaryDto Transactions { get; set; } = new();
        public List<TopReplacedPartDto> MostReplaced { get; set; } = new();
        public List<PartLifeRowDto> PartLife { get; set; } = new();
        public List<ChartPointDto> BySupplier { get; set; } = new();
        public List<ChartPointDto> ByManufacturer { get; set; } = new();
    }

    public class PerformanceReportBundleDto
    {
        public PerformanceSummaryDto Summary { get; set; } = new();
        public List<EngineerPerformanceRowDto> Engineers { get; set; } = new();
    }

    public class CostReportBundleDto
    {
        public decimal TotalLabor { get; set; }
        public decimal TotalParts { get; set; }
        public decimal TotalExternal { get; set; }
        public decimal TotalOther { get; set; }
        public decimal GrandTotal { get; set; }
        public List<ChartPointDto> ByMonth { get; set; } = new();
        public List<AssetCostReportRowDto> ByAsset { get; set; } = new();
    }
}
