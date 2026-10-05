using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Reporting;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : BaseAppController
    {
        private readonly IReportingService _reportingService;

        public AnalyticsController(IReportingService reportingService) => _reportingService = reportingService;

        [HttpPost("management")]
        [RequirePermission(Permissions.DashboardView)]
        public async Task<IActionResult> Management([FromBody] ReportingFilterDto filter) =>
            Ok(await _reportingService.GetManagementAnalyticsAsync(filter ?? new ReportingFilterDto()));

        [HttpPost("assets")]
        [RequirePermission(Permissions.DashboardView)]
        public async Task<IActionResult> Assets([FromBody] ReportingFilterDto filter) =>
            Ok(await _reportingService.GetAssetAnalyticsDashboardAsync(filter ?? new ReportingFilterDto()));

        [HttpPost("maintenance")]
        [RequirePermission(Permissions.DashboardView)]
        public async Task<IActionResult> Maintenance([FromBody] ReportingFilterDto filter) =>
            Ok(await _reportingService.GetMaintenanceAnalyticsDashboardAsync(filter ?? new ReportingFilterDto()));

        [HttpPost("reliability")]
        [RequirePermission(Permissions.DashboardView)]
        public async Task<IActionResult> Reliability([FromBody] ReportingFilterDto filter) =>
            Ok(await _reportingService.GetReliabilityAnalyticsDashboardAsync(filter ?? new ReportingFilterDto()));

        [HttpPost("work-orders")]
        [RequirePermission(Permissions.DashboardView)]
        public async Task<IActionResult> WorkOrders([FromBody] ReportingFilterDto filter) =>
            Ok(await _reportingService.GetWorkOrderAnalyticsDashboardAsync(filter ?? new ReportingFilterDto()));

        [HttpPost("spare-parts")]
        [RequirePermission(Permissions.DashboardView)]
        public async Task<IActionResult> SpareParts([FromBody] ReportingFilterDto filter) =>
            Ok(await _reportingService.GetSparePartsAnalyticsDashboardAsync(filter ?? new ReportingFilterDto()));

        [HttpPost("cost")]
        [RequirePermission(Permissions.DashboardView)]
        public async Task<IActionResult> Cost([FromBody] ReportingFilterDto filter) =>
            Ok(await _reportingService.GetCostAnalyticsDashboardAsync(filter ?? new ReportingFilterDto()));

        [HttpPost("performance")]
        [RequirePermission(Permissions.DashboardView)]
        public async Task<IActionResult> Performance([FromBody] ReportingFilterDto filter) =>
            Ok(await _reportingService.GetPerformanceReportAsync(filter ?? new ReportingFilterDto()));

        [HttpPost("assets/{assetId}/detail")]
        [RequirePermission(Permissions.DashboardView)]
        public async Task<IActionResult> AssetDetail(int assetId, [FromBody] ReportingFilterDto filter) =>
            Ok(await _reportingService.GetAssetDetailAnalyticsAsync(assetId, filter ?? new ReportingFilterDto()));
    }
}
