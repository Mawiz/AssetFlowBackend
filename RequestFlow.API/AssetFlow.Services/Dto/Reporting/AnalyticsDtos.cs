namespace AssetFlow.Services.Dto.Reporting
{
    public class KpiMetricDto
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public decimal? Value { get; set; }
        public decimal? PreviousValue { get; set; }
        public decimal? ChangePercent { get; set; }
        public string Unit { get; set; } = "number";
        public string? DrillRoute { get; set; }
        public string? DrillQuery { get; set; }
        public bool HigherIsBetter { get; set; } = true;
    }

    public class AnalyticsInsightDto
    {
        public string Text { get; set; } = string.Empty;
        public string Severity { get; set; } = "info";
        public string? DrillRoute { get; set; }
        public string? DrillQuery { get; set; }
    }

    public class AssetAttentionDto
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public List<string> Reasons { get; set; } = new();
        public int BreakdownCount { get; set; }
        public int OverduePmCount { get; set; }
        public decimal DowntimeMinutes { get; set; }
        public decimal MaintenanceCost { get; set; }
        public decimal? MttrHours { get; set; }
    }

    public class TimeSeriesMultiDto
    {
        public List<string> Labels { get; set; } = new();
        public List<NamedSeriesDto> Series { get; set; } = new();
    }

    public class NamedSeriesDto
    {
        public string Name { get; set; } = string.Empty;
        public List<decimal> Values { get; set; } = new();
    }

    public class StackedCostMonthDto
    {
        public string Label { get; set; } = string.Empty;
        public decimal Labor { get; set; }
        public decimal Parts { get; set; }
        public decimal External { get; set; }
        public decimal Other { get; set; }
        public decimal Total => Labor + Parts + External + Other;
    }

    public class ScatterPointDto
    {
        public int AssetId { get; set; }
        public string Label { get; set; } = string.Empty;
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public decimal? Size { get; set; }
        public string Tooltip { get; set; } = string.Empty;
    }

    public class PmOverdueRowDto
    {
        public int OccurrenceId { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string MaintenanceTypeName { get; set; } = string.Empty;
        public DateTime? ScheduledDate { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public int Status { get; set; }
    }

    public class WorkOrderAgingBucketDto
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class LocationAssetMetricDto
    {
        public string LocationName { get; set; } = string.Empty;
        public int AssetCount { get; set; }
        public int BreakdownCount { get; set; }
        public decimal DowntimeMinutes { get; set; }
    }

    public class AssetAgeBucketDto
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ManagementAnalyticsDto
    {
        public DashboardSummaryDto Summary { get; set; } = new();
        public List<KpiMetricDto> HeroKpis { get; set; } = new();
        public List<AnalyticsInsightDto> Insights { get; set; } = new();
        public List<AssetAttentionDto> AttentionAssets { get; set; } = new();
        public TimeSeriesMultiDto MaintenanceActivity { get; set; } = new();
        public List<ChartPointDto> WorkOrderPipeline { get; set; } = new();
        public List<StackedCostMonthDto> CostTrendStacked { get; set; } = new();
        public List<ChartPointDto> DowntimeTrend { get; set; } = new();
        public List<TopFailingAssetDto> TopDowntimeAssets { get; set; } = new();
        public List<TopReplacedPartDto> TopReplacedParts { get; set; } = new();
        public List<ChartPointDto> StockByLocation { get; set; } = new();
        public List<EngineerPerformanceRowDto> TeamPerformance { get; set; } = new();
        public DateTime LastUpdatedUtc { get; set; }
    }

    public class AssetAnalyticsDashboardDto
    {
        public AssetSummaryDto Summary { get; set; } = new();
        public List<KpiMetricDto> Kpis { get; set; } = new();
        public List<ChartPointDto> ByType { get; set; } = new();
        public List<LocationAssetMetricDto> ByLocation { get; set; } = new();
        public List<AssetAgeBucketDto> AgeDistribution { get; set; } = new();
        public List<AssetReliabilityRowDto> Reliability { get; set; } = new();
        public List<ScatterPointDto> MtbfMttrScatter { get; set; } = new();
    }

    public class MaintenanceAnalyticsDashboardDto
    {
        public MaintenanceSummaryDto Summary { get; set; } = new();
        public PmReportSummaryDto PreventiveMaintenance { get; set; } = new();
        public List<PmOverdueRowDto> OverduePm { get; set; } = new();
        public List<ChartPointDto> PmComplianceBreakdown { get; set; } = new();
        public TimeSeriesMultiDto PmTrend { get; set; } = new();
        public List<ChartPointDto> WorkOrdersByPriority { get; set; } = new();
        public List<ChartPointDto> PmByCategory { get; set; } = new();
    }

    public class ReliabilityAnalyticsDashboardDto
    {
        public BreakdownReportBundleDto Breakdown { get; set; } = new();
        public PerformanceSummaryDto Performance { get; set; } = new();
        public List<ScatterPointDto> MtbfMttrScatter { get; set; } = new();
        public List<ScatterPointDto> CostVsBreakdownScatter { get; set; } = new();
        public List<TopFailingAssetDto> RepeatedFailures { get; set; } = new();
    }

    public class WorkOrderAnalyticsDashboardDto
    {
        public List<ChartPointDto> StatusPipeline { get; set; } = new();
        public List<WorkOrderAgingBucketDto> Aging { get; set; } = new();
        public List<ChartPointDto> ByPriority { get; set; } = new();
        public TimeSeriesMultiDto CompletionTrend { get; set; } = new();
        public List<KpiMetricDto> Kpis { get; set; } = new();
    }

    public class SparePartsAnalyticsDashboardDto
    {
        public SparePartsReportBundleDto Report { get; set; } = new();
        public decimal TotalStockQuantity { get; set; }
        public decimal FaultyStockQuantity { get; set; }
        public List<ChartPointDto> ConsumptionTrend { get; set; } = new();
        public List<ChartPointDto> InventoryHealth { get; set; } = new();
    }

    public class CostAnalyticsDashboardDto
    {
        public CostReportBundleDto Report { get; set; } = new();
        public List<KpiMetricDto> Kpis { get; set; } = new();
        public List<ScatterPointDto> CostVsBreakdownScatter { get; set; } = new();
    }

    public class AssetDetailAnalyticsDto
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public int Status { get; set; }
        public int Criticality { get; set; }
        public List<KpiMetricDto> Kpis { get; set; } = new();
        public List<ChartPointDto> BreakdownTrend { get; set; } = new();
        public List<StackedCostMonthDto> CostTrend { get; set; } = new();
        public decimal? MtbfHours { get; set; }
        public decimal? MttrHours { get; set; }
        public DateTime? LastMaintenance { get; set; }
        public DateTime? NextMaintenanceDue { get; set; }
    }
}
