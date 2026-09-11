using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Tenant;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : BaseAppController
    {
        private readonly ITenantService _service;

        public TenantController(ITenantService service)
        {
            _service = service;
        }

        [HttpPost]
        [RequirePermission(Permissions.TenantCreate)]
        public async Task<IActionResult> Create(CreateTenantDto dto)
        {
            var response = await _service.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.TenantUpdate)]
        public async Task<IActionResult> Update(UpdateTenantDto dto)
        {
            var response = await _service.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.TenantView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [RequirePermission(Permissions.TenantList)]
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.GetAllAsync();
            return Ok(response);
        }

        [HttpPost("Filter")]
        [RequirePermission(Permissions.TenantList)]
        public async Task<IActionResult> Filter([FromBody] AssetFlow.Services.Dto.SearchViewDto model)
        {
            var response = await _service.FilterAsync(model);
            return Ok(response);
        }

        [HttpPatch("{id}/toggle-status")]
        [RequirePermission(Permissions.TenantToggle)]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var response = await _service.ToggleStatusAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
