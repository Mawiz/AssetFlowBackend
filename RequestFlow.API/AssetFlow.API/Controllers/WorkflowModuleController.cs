using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Workflow;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowModuleController : BaseAppController
    {
        private readonly IWorkflowConfigurationService _service;

        public WorkflowModuleController(IWorkflowConfigurationService service) => _service = service;

        [HttpPost]
        [RequirePermission(Permissions.WorkflowConfigurationCreate)]
        public async Task<IActionResult> Create([FromBody] CreateWorkflowModuleDto dto)
        {
            var response = await _service.CreateModuleAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.WorkflowConfigurationUpdate)]
        public async Task<IActionResult> Update([FromBody] UpdateWorkflowModuleDto dto)
        {
            var response = await _service.UpdateModuleAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{moduleId}")]
        [RequirePermission(Permissions.WorkflowConfigurationView)]
        public async Task<IActionResult> GetById(int moduleId)
        {
            var response = await _service.GetModuleByIdAsync(moduleId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [RequirePermission(Permissions.WorkflowConfigurationView)]
        public async Task<IActionResult> GetAll([FromQuery] int? tenantId) => Ok(await _service.GetAllModulesAsync(tenantId));

        [HttpPost("Filter")]
        [RequirePermission(Permissions.WorkflowConfigurationView)]
        public async Task<IActionResult> Filter([FromBody] WorkflowModuleFilterDto model) => Ok(await _service.FilterModulesAsync(model));

        [HttpDelete("{moduleId}")]
        [RequirePermission(Permissions.WorkflowConfigurationDelete)]
        public async Task<IActionResult> Delete(int moduleId)
        {
            var response = await _service.DeleteModuleAsync(moduleId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
