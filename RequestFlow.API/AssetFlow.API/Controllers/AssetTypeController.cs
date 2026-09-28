using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Asset;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetTypeController : BaseAppController
    {
        private readonly IAssetTypeService _service;

        public AssetTypeController(IAssetTypeService service)
        {
            _service = service;
        }

        [HttpPost]
        [RequirePermission(Permissions.AssetTypeCreate)]
        public async Task<IActionResult> Create(CreateAssetTypeDto dto)
        {
            var response = await _service.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.AssetTypeUpdate)]
        public async Task<IActionResult> Update(UpdateAssetTypeDto dto)
        {
            var response = await _service.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.AssetTypeView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [RequirePermission(Permissions.AssetTypeView)]
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.GetAllAsync();
            return Ok(response);
        }

        [HttpPost("Filter")]
        [RequirePermission(Permissions.AssetTypeView)]
        public async Task<IActionResult> Filter([FromBody] AssetTypeFilterDto model)
        {
            var response = await _service.FilterAsync(model);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.AssetTypeDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
