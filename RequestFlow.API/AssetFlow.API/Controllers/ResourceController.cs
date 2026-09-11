using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Role.RoleResource;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourceController : BaseAppController
    {
        private readonly IResourceService _service;
        public ResourceController(IResourceService service)
        {
            _service = service;
        }

        [HttpPost]
        [RequirePermission(Permissions.ResourceCreate)]
        public async Task<IActionResult> Create([FromBody] CreateResourceDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPut]
        [RequirePermission(Permissions.ResourceUpdate)]
        public async Task<IActionResult> Update([FromBody] UpdateResourceDto dto)
        {
            var result = await _service.UpdateAsync(dto);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet]
        [RequirePermission(Permissions.ResourceList)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("Filter")]
        [RequirePermission(Permissions.ResourceList)]
        public async Task<IActionResult> Filter([FromBody] AssetFlow.Services.Dto.SearchViewDto model)
        {
            var result = await _service.FilterAsync(model);
            return Ok(result);
        }
    }
}
