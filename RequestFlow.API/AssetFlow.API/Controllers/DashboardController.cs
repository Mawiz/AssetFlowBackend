using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Reporting;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BaseAppController
    {
        private readonly IReportingService _reportingService;

        public DashboardController(IReportingService reportingService) => _reportingService = reportingService;

        [HttpPost("summary")]
        [RequirePermission(Permissions.DashboardView)]
        public async Task<IActionResult> Summary([FromBody] ReportingFilterDto filter) =>
            Ok(await _reportingService.GetDashboardSummaryAsync(filter ?? new ReportingFilterDto()));
    }
}
