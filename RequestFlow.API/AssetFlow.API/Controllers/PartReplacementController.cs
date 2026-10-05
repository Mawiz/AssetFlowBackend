using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.SparePart;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartReplacementController : BaseAppController
    {
        private readonly IPartReplacementService _service;

        public PartReplacementController(IPartReplacementService service) => _service = service;

        [HttpPost("Validate")]
        [RequirePermission(Permissions.PartReplacementValidate)]
        public async Task<IActionResult> Validate([FromBody] ValidatePartReplacementDto dto)
        {
            var response = await _service.ValidateAsync(dto);
            return Ok(response);
        }

        [HttpGet("LookupSerial")]
        [RequirePermission(Permissions.PartReplacementValidate)]
        public async Task<IActionResult> LookupSerial([FromQuery] int workOrderId, [FromQuery] string serial)
        {
            var response = await _service.LookupSerialAsync(workOrderId, serial);
            return Ok(response);
        }

        [HttpPost("Replace")]
        [RequirePermission(Permissions.PartReplacementReplace)]
        public async Task<IActionResult> Replace([FromBody] ConfirmPartReplacementDto dto)
        {
            var response = await _service.ReplaceAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.PartReplacementView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("ByWorkOrder/{workOrderId}")]
        [RequirePermission(Permissions.PartReplacementView)]
        public async Task<IActionResult> GetByWorkOrder(int workOrderId)
        {
            var response = await _service.GetByWorkOrderAsync(workOrderId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
