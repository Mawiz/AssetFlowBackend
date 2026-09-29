using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.SparePart;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartTransactionController : BaseAppController
    {
        private readonly IPartTransactionService _service;

        public PartTransactionController(IPartTransactionService service) => _service = service;

        [HttpPost]
        [RequirePermission(Permissions.PartTransactionCreate)]
        public async Task<IActionResult> Create(CreatePartTransactionDto dto)
        {
            var response = await _service.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Filter")]
        [RequirePermission(Permissions.PartTransactionView)]
        public async Task<IActionResult> Filter([FromBody] PartTransactionFilterDto model) => Ok(await _service.FilterAsync(model));

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.PartTransactionDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
