using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.SparePart;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartCategoryController : BaseAppController
    {
        private readonly IPartCategoryService _service;

        public PartCategoryController(IPartCategoryService service) => _service = service;

        [HttpPost]
        [RequirePermission(Permissions.PartCategoryCreate)]
        public async Task<IActionResult> Create(CreatePartCategoryDto dto)
        {
            var response = await _service.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.PartCategoryUpdate)]
        public async Task<IActionResult> Update(UpdatePartCategoryDto dto)
        {
            var response = await _service.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.PartCategoryView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [RequirePermission(Permissions.PartCategoryView)]
        public async Task<IActionResult> GetAll([FromQuery] int? tenantId) => Ok(await _service.GetAllAsync(tenantId));

        [HttpPost("Filter")]
        [RequirePermission(Permissions.PartCategoryView)]
        public async Task<IActionResult> Filter([FromBody] SearchViewDto model) => Ok(await _service.FilterAsync(model));

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.PartCategoryDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
