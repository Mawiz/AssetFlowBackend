using Microsoft.AspNetCore.Mvc;
using RequestFlow.Services.Contracts;
using RequestFlow.Services.Dto.Role;

namespace RequestFlow.API.Controllers
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

        // 🔹 Create Role
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return StatusCode((int)result.StatusCode, result);
        }

        // 🔹 Update Role
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateRoleDto dto)
        {
            var result = await _service.UpdateAsync(dto);
            return StatusCode((int)result.StatusCode, result);
        }

        // 🔹 Get All Roles
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("Filter")]
        public async Task<IActionResult> Filter([FromBody] RequestFlow.Services.Dto.SearchViewDto model)
        {
            var result = await _service.FilterAsync(model);
            return Ok(result);
        }
    }
}
