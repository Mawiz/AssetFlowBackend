using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Reporting;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : BaseAppController
    {
        private readonly IReportingService _reportingService;

        public ReportsController(IReportingService reportingService) => _reportingService = reportingService;

        [HttpPost("assets")]
        [RequirePermission(Permissions.ReportsView)]
        public async Task<IActionResult> Assets([FromBody] ReportingFilterDto filter) =>
            Ok(await _reportingService.GetAssetReportAsync(filter ?? new ReportingFilterDto()));

        [HttpPost("maintenance")]
        [RequirePermission(Permissions.ReportsView)]
        public async Task<IActionResult> Maintenance([FromBody] ReportingFilterDto filter) =>
            Ok(await _reportingService.GetMaintenanceReportAsync(filter ?? new ReportingFilterDto()));

        [HttpPost("breakdowns")]
        [RequirePermission(Permissions.ReportsView)]
        public async Task<IActionResult> Breakdowns([FromBody] ReportingFilterDto filter) =>
            Ok(await _reportingService.GetBreakdownReportAsync(filter ?? new ReportingFilterDto()));

        [HttpPost("spare-parts")]
        [RequirePermission(Permissions.ReportsView)]
        public async Task<IActionResult> SpareParts([FromBody] ReportingFilterDto filter) =>
            Ok(await _reportingService.GetSparePartsReportAsync(filter ?? new ReportingFilterDto()));

        [HttpPost("performance")]
        [RequirePermission(Permissions.ReportsView)]
        public async Task<IActionResult> Performance([FromBody] ReportingFilterDto filter) =>
            Ok(await _reportingService.GetPerformanceReportAsync(filter ?? new ReportingFilterDto()));

        [HttpPost("costs")]
        [RequirePermission(Permissions.ReportsView)]
        public async Task<IActionResult> Costs([FromBody] ReportingFilterDto filter) =>
            Ok(await _reportingService.GetCostReportAsync(filter ?? new ReportingFilterDto()));
    }
}
