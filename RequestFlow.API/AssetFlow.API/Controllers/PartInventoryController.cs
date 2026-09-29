using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.SparePart;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartInventoryController : BaseAppController
    {
        private readonly IPartInventoryService _service;

        public PartInventoryController(IPartInventoryService service) => _service = service;

        [HttpPost("Filter")]
        [RequirePermission(Permissions.PartInventoryView)]
        public async Task<IActionResult> Filter([FromBody] PartInventoryFilterDto model) => Ok(await _service.FilterAsync(model));

        [HttpPost("Receipt")]
        [RequirePermission(Permissions.PartInventoryCreate)]
        public async Task<IActionResult> Receipt([FromBody] PartReceiptDto dto)
        {
            var response = await _service.ReceiptAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Transfer")]
        [RequirePermission(Permissions.PartInventoryUpdate)]
        public async Task<IActionResult> Transfer([FromBody] PartTransferDto dto)
        {
            var response = await _service.TransferAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Adjust")]
        [RequirePermission(Permissions.PartInventoryUpdate)]
        public async Task<IActionResult> Adjust([FromBody] PartAdjustmentDto dto)
        {
            var response = await _service.AdjustAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.PartInventoryDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
