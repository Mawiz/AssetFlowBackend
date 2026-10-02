using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Issue;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IssueCategoryController : BaseAppController
    {
        private readonly IIssueCategoryService _service;

        public IssueCategoryController(IIssueCategoryService service) => _service = service;

        [HttpPost]
        [RequirePermission(Permissions.IssueCategoryCreate)]
        public async Task<IActionResult> Create([FromBody] CreateIssueCategoryDto dto)
        {
            var response = await _service.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.IssueCategoryUpdate)]
        public async Task<IActionResult> Update([FromBody] UpdateIssueCategoryDto dto)
        {
            var response = await _service.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.IssueCategoryView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [RequirePermission(Permissions.IssueCategoryView)]
        public async Task<IActionResult> GetAll([FromQuery] int? tenantId) => Ok(await _service.GetAllAsync(tenantId));

        [HttpPost("Filter")]
        [RequirePermission(Permissions.IssueCategoryView)]
        public async Task<IActionResult> Filter([FromBody] IssueCategoryFilterDto model) => Ok(await _service.FilterAsync(model));

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.IssueCategoryDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
