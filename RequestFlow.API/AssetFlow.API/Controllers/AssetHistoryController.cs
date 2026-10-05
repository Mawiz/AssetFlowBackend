using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.History;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetHistoryController : BaseAppController
    {
        private readonly IAssetHistoryService _service;

        public AssetHistoryController(IAssetHistoryService service) => _service = service;

        [HttpGet("{assetId}/summary")]
        [RequirePermission(Permissions.AssetHistoryView)]
        public async Task<IActionResult> Summary(int assetId) => Ok(await _service.GetSummaryAsync(assetId));

        [HttpPost("timeline")]
        [RequirePermission(Permissions.AssetHistoryView)]
        public async Task<IActionResult> Timeline([FromBody] AssetHistoryFilterDto filter) =>
            Ok(await _service.GetTimelineAsync(filter));

        [HttpPost("maintenance")]
        [RequirePermission(Permissions.AssetHistoryView)]
        public async Task<IActionResult> Maintenance([FromBody] AssetHistoryFilterDto filter) =>
            Ok(await _service.GetMaintenanceHistoryAsync(filter));

        [HttpPost("breakdown")]
        [RequirePermission(Permissions.AssetHistoryView)]
        public async Task<IActionResult> Breakdown([FromBody] AssetHistoryFilterDto filter) =>
            Ok(await _service.GetBreakdownHistoryAsync(filter));

        [HttpPost("workorders")]
        [RequirePermission(Permissions.AssetHistoryView)]
        public async Task<IActionResult> WorkOrders([FromBody] AssetHistoryFilterDto filter) =>
            Ok(await _service.GetWorkOrderHistoryAsync(filter));

        [HttpPost("parts")]
        [RequirePermission(Permissions.AssetHistoryView)]
        public async Task<IActionResult> Parts([FromBody] AssetHistoryFilterDto filter) =>
            Ok(await _service.GetPartsHistoryAsync(filter));

        [HttpPost("downtime")]
        [RequirePermission(Permissions.AssetHistoryView)]
        public async Task<IActionResult> Downtime([FromBody] AssetHistoryFilterDto filter) =>
            Ok(await _service.GetDowntimeHistoryAsync(filter));

        [HttpPost("costs")]
        [RequirePermission(Permissions.AssetHistoryView)]
        public async Task<IActionResult> Costs([FromBody] AssetHistoryFilterDto filter) =>
            Ok(await _service.GetCostHistoryAsync(filter));

        [HttpPost("{assetId}/costsummary")]
        [RequirePermission(Permissions.AssetHistoryView)]
        public async Task<IActionResult> CostSummary(int assetId, [FromBody] AssetHistoryFilterDto filter)
        {
            filter.AssetId = assetId;
            return Ok(await _service.GetCostSummaryAsync(assetId, filter));
        }
    }
}
