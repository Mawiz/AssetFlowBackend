using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Maintenance;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceChecklistController : BaseAppController
    {
        private readonly IMaintenanceChecklistService _service;

        public MaintenanceChecklistController(IMaintenanceChecklistService service) => _service = service;

        [HttpPost]
        [RequirePermission(Permissions.MaintenanceChecklistCreate)]
        public async Task<IActionResult> Create(SaveMaintenanceChecklistDto dto)
        {
            var response = await _service.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.MaintenanceChecklistUpdate)]
        public async Task<IActionResult> Update(UpdateMaintenanceChecklistDto dto)
        {
            var response = await _service.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.MaintenanceChecklistView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [RequirePermission(Permissions.MaintenanceChecklistView)]
        public async Task<IActionResult> GetAll([FromQuery] int? tenantId, [FromQuery] int? maintenanceTypeId) =>
            Ok(await _service.GetAllAsync(tenantId, maintenanceTypeId));

        [HttpPost("Filter")]
        [RequirePermission(Permissions.MaintenanceChecklistView)]
        public async Task<IActionResult> Filter([FromBody] MaintenanceChecklistFilterDto model) => Ok(await _service.FilterAsync(model));

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.MaintenanceChecklistDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
