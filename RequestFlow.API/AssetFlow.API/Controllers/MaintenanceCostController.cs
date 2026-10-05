using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.History;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceCostController : BaseAppController
    {
        private readonly IMaintenanceCostService _service;

        public MaintenanceCostController(IMaintenanceCostService service) => _service = service;

        [HttpGet("workorder/{workOrderId}/summary")]
        [RequirePermission(Permissions.CostManagementView)]
        public async Task<IActionResult> WorkOrderSummary(int workOrderId) =>
            Ok(await _service.GetWorkOrderCostSummaryAsync(workOrderId));

        [HttpGet("workorder/{workOrderId}")]
        [RequirePermission(Permissions.CostManagementView)]
        public async Task<IActionResult> ByWorkOrder(int workOrderId) =>
            Ok(await _service.GetByWorkOrderAsync(workOrderId));

        [HttpGet("workorder/{workOrderId}/labor")]
        [RequirePermission(Permissions.CostManagementView)]
        public async Task<IActionResult> LaborByWorkOrder(int workOrderId) =>
            Ok(await _service.GetLaborByWorkOrderAsync(workOrderId));

        [HttpPost]
        [RequirePermission(Permissions.CostManagementCreate)]
        public async Task<IActionResult> UpsertCost([FromBody] MaintenanceCostUpsertDto dto) =>
            StatusCode(200, await _service.UpsertCostAsync(dto));

        [HttpPut]
        [RequirePermission(Permissions.CostManagementUpdate)]
        public async Task<IActionResult> UpdateCost([FromBody] MaintenanceCostUpsertDto dto) =>
            StatusCode(200, await _service.UpsertCostAsync(dto));

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.CostManagementDelete)]
        public async Task<IActionResult> DeleteCost(int id) =>
            StatusCode(200, await _service.DeleteCostAsync(id));

        [HttpPost("labor")]
        [RequirePermission(Permissions.CostManagementCreate)]
        public async Task<IActionResult> UpsertLabor([FromBody] WorkOrderLaborUpsertDto dto) =>
            StatusCode(200, await _service.UpsertLaborAsync(dto));

        [HttpPut("labor")]
        [RequirePermission(Permissions.CostManagementUpdate)]
        public async Task<IActionResult> UpdateLabor([FromBody] WorkOrderLaborUpsertDto dto) =>
            StatusCode(200, await _service.UpsertLaborAsync(dto));

        [HttpDelete("labor/{id}")]
        [RequirePermission(Permissions.CostManagementDelete)]
        public async Task<IActionResult> DeleteLabor(int id) =>
            StatusCode(200, await _service.DeleteLaborAsync(id));
    }
}
