using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Reporting;

namespace AssetFlow.Services.Contracts
{
    public interface IReportingService
    {
        Task<ResponseDto<DashboardSummaryDto>> GetDashboardSummaryAsync(ReportingFilterDto filter);
        Task<ResponseDto<AssetReportBundleDto>> GetAssetReportAsync(ReportingFilterDto filter);
        Task<ResponseDto<MaintenanceReportBundleDto>> GetMaintenanceReportAsync(ReportingFilterDto filter);
        Task<ResponseDto<BreakdownReportBundleDto>> GetBreakdownReportAsync(ReportingFilterDto filter);
        Task<ResponseDto<SparePartsReportBundleDto>> GetSparePartsReportAsync(ReportingFilterDto filter);
        Task<ResponseDto<PerformanceReportBundleDto>> GetPerformanceReportAsync(ReportingFilterDto filter);
        Task<ResponseDto<CostReportBundleDto>> GetCostReportAsync(ReportingFilterDto filter);
        Task<ResponseDto<ManagementAnalyticsDto>> GetManagementAnalyticsAsync(ReportingFilterDto filter);
        Task<ResponseDto<AssetAnalyticsDashboardDto>> GetAssetAnalyticsDashboardAsync(ReportingFilterDto filter);
        Task<ResponseDto<MaintenanceAnalyticsDashboardDto>> GetMaintenanceAnalyticsDashboardAsync(ReportingFilterDto filter);
        Task<ResponseDto<ReliabilityAnalyticsDashboardDto>> GetReliabilityAnalyticsDashboardAsync(ReportingFilterDto filter);
        Task<ResponseDto<WorkOrderAnalyticsDashboardDto>> GetWorkOrderAnalyticsDashboardAsync(ReportingFilterDto filter);
        Task<ResponseDto<SparePartsAnalyticsDashboardDto>> GetSparePartsAnalyticsDashboardAsync(ReportingFilterDto filter);
        Task<ResponseDto<CostAnalyticsDashboardDto>> GetCostAnalyticsDashboardAsync(ReportingFilterDto filter);
        Task<ResponseDto<AssetDetailAnalyticsDto>> GetAssetDetailAnalyticsAsync(int assetId, ReportingFilterDto filter);
    }
}
