using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Maintenance;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreventiveMaintenanceController : BaseAppController
    {
        private readonly IPreventiveMaintenanceService _pmService;
        private readonly IPreventiveMaintenanceGenerationService _generationService;

        public PreventiveMaintenanceController(
            IPreventiveMaintenanceService pmService,
            IPreventiveMaintenanceGenerationService generationService)
        {
            _pmService = pmService;
            _generationService = generationService;
        }

        [HttpPost("Filter")]
        [RequirePermission(Permissions.PreventiveMaintenanceView)]
        public async Task<IActionResult> Filter([FromBody] PreventiveMaintenanceOccurrenceFilterDto model) =>
            Ok(await _pmService.FilterAsync(model));

        [HttpGet("{id}")]
        [RequirePermission(Permissions.PreventiveMaintenanceView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _pmService.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("{id}/Start")]
        [RequirePermission(Permissions.PreventiveMaintenanceUpdate)]
        public async Task<IActionResult> Start(int id)
        {
            var response = await _pmService.StartAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Complete")]
        [RequirePermission(Permissions.PreventiveMaintenanceComplete)]
        public async Task<IActionResult> Complete([FromBody] CompletePreventiveMaintenanceDto dto)
        {
            var response = await _pmService.CompleteAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("{id}/Cancel")]
        [RequirePermission(Permissions.PreventiveMaintenanceUpdate)]
        public async Task<IActionResult> Cancel(int id)
        {
            var response = await _pmService.CancelAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Generate")]
        [RequirePermission(Permissions.PreventiveMaintenanceGenerate)]
        public async Task<IActionResult> Generate(GeneratePreventiveMaintenanceDto dto)
        {
            var response = await _generationService.GenerateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Calendar")]
        [RequirePermission(Permissions.PreventiveMaintenanceView)]
        public async Task<IActionResult> Calendar([FromBody] PreventiveMaintenanceOccurrenceFilterDto model) =>
            Ok(await _pmService.GetCalendarAsync(model));

        [HttpGet("AssetSummary/{assetId}")]
        [RequirePermission(Permissions.PreventiveMaintenanceView)]
        public async Task<IActionResult> AssetSummary(int assetId) => Ok(await _pmService.GetAssetSummaryAsync(assetId));
    }
}
