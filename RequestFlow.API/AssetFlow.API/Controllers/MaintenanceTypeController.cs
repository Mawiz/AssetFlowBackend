using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Maintenance;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceTypeController : BaseAppController
    {
        private readonly IMaintenanceTypeService _service;

        public MaintenanceTypeController(IMaintenanceTypeService service) => _service = service;

        [HttpPost]
        [RequirePermission(Permissions.MaintenanceTypeCreate)]
        public async Task<IActionResult> Create(CreateMaintenanceTypeDto dto)
        {
            var response = await _service.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.MaintenanceTypeUpdate)]
        public async Task<IActionResult> Update(UpdateMaintenanceTypeDto dto)
        {
            var response = await _service.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.MaintenanceTypeView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [RequirePermission(Permissions.MaintenanceTypeView)]
        public async Task<IActionResult> GetAll([FromQuery] int? tenantId) => Ok(await _service.GetAllAsync(tenantId));

        [HttpPost("Filter")]
        [RequirePermission(Permissions.MaintenanceTypeView)]
        public async Task<IActionResult> Filter([FromBody] MaintenanceTypeFilterDto model) => Ok(await _service.FilterAsync(model));

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.MaintenanceTypeDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
