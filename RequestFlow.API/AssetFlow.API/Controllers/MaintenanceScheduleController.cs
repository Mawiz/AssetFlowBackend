using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Maintenance;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceScheduleController : BaseAppController
    {
        private readonly IMaintenanceScheduleService _service;

        public MaintenanceScheduleController(IMaintenanceScheduleService service) => _service = service;

        [HttpPost]
        [RequirePermission(Permissions.MaintenanceScheduleCreate)]
        public async Task<IActionResult> Create(SaveMaintenanceScheduleDto dto)
        {
            var response = await _service.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.MaintenanceScheduleUpdate)]
        public async Task<IActionResult> Update(UpdateMaintenanceScheduleDto dto)
        {
            var response = await _service.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.MaintenanceScheduleView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Filter")]
        [RequirePermission(Permissions.MaintenanceScheduleView)]
        public async Task<IActionResult> Filter([FromBody] MaintenanceScheduleFilterDto model) => Ok(await _service.FilterAsync(model));

        [HttpPut("{id}/active")]
        [RequirePermission(Permissions.MaintenanceScheduleUpdate)]
        public async Task<IActionResult> SetActive(int id, [FromQuery] bool isActive)
        {
            var response = await _service.SetActiveAsync(id, isActive);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
