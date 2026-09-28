using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Asset;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetComponentController : BaseAppController
    {
        private readonly IAssetComponentService _service;

        public AssetComponentController(IAssetComponentService service)
        {
            _service = service;
        }

        [HttpPost]
        [RequirePermission(Permissions.AssetComponentCreate)]
        public async Task<IActionResult> Create(CreateAssetComponentDto dto)
        {
            var response = await _service.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.AssetComponentUpdate)]
        public async Task<IActionResult> Update(UpdateAssetComponentDto dto)
        {
            var response = await _service.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.AssetComponentView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Filter")]
        [RequirePermission(Permissions.AssetComponentView)]
        public async Task<IActionResult> Filter([FromBody] AssetComponentFilterDto model)
        {
            var response = await _service.FilterAsync(model);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.AssetComponentDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
