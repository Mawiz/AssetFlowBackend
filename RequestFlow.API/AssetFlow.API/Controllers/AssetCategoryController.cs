using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Asset;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetCategoryController : BaseAppController
    {
        private readonly IAssetCategoryService _service;

        public AssetCategoryController(IAssetCategoryService service)
        {
            _service = service;
        }

        [HttpPost]
        [RequirePermission(Permissions.AssetCategoryCreate)]
        public async Task<IActionResult> Create(CreateAssetCategoryDto dto)
        {
            var response = await _service.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.AssetCategoryUpdate)]
        public async Task<IActionResult> Update(UpdateAssetCategoryDto dto)
        {
            var response = await _service.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.AssetCategoryView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [RequirePermission(Permissions.AssetCategoryView)]
        public async Task<IActionResult> GetAll([FromQuery] int? tenantId)
        {
            var response = await _service.GetAllAsync(tenantId);
            return Ok(response);
        }

        [HttpPost("Filter")]
        [RequirePermission(Permissions.AssetCategoryView)]
        public async Task<IActionResult> Filter([FromBody] SearchViewDto model)
        {
            var response = await _service.FilterAsync(model);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.AssetCategoryDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
