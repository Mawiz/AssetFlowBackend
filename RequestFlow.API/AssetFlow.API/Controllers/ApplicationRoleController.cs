using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Role;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationRoleController : BaseAppController
    {
        private readonly IApplicationRoleService _service;
        public ApplicationRoleController(IApplicationRoleService service)
        {
            _service = service;
        }

        [HttpPost]
        [RequirePermission(Permissions.RoleCreate)]
        public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPut]
        [RequirePermission(Permissions.RoleUpdate)]
        public async Task<IActionResult> Update([FromBody] UpdateRoleDto dto)
        {
            var result = await _service.UpdateAsync(dto);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet]
        [RequirePermission(Permissions.RoleList)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet("{id:int}")]
        [RequirePermission(Permissions.RoleView)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("Filter")]
        [RequirePermission(Permissions.RoleList)]
        public async Task<IActionResult> Filter([FromBody] AssetFlow.Services.Dto.SearchViewDto model)
        {
            var result = await _service.FilterAsync(model);
            return Ok(result);
        }

        [HttpGet("tenant")]
        [HttpGet("tenant/{tenantId:int}")]
        //[RequirePermission(Permissions.RoleList)]
        public async Task<IActionResult> GetByTenant(int? tenantId = null)
        {
            var result = await _service.GetRolesByTenantAsync(tenantId);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}
