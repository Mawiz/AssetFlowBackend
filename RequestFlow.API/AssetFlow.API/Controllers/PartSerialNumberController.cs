using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.SparePart;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartSerialNumberController : BaseAppController
    {
        private readonly IPartSerialNumberService _service;

        public PartSerialNumberController(IPartSerialNumberService service) => _service = service;

        [HttpPut]
        [RequirePermission(Permissions.PartInventoryUpdate)]
        public async Task<IActionResult> Update(UpdatePartSerialNumberDto dto)
        {
            var response = await _service.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.PartInventoryView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Filter")]
        [RequirePermission(Permissions.PartInventoryView)]
        public async Task<IActionResult> Filter([FromBody] PartSerialNumberFilterDto model) => Ok(await _service.FilterAsync(model));

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.PartInventoryDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
