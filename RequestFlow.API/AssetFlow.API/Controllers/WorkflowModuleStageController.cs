using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Workflow;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowModuleStageController : BaseAppController
    {
        private readonly IWorkflowConfigurationService _service;

        public WorkflowModuleStageController(IWorkflowConfigurationService service) => _service = service;

        [HttpPost]
        [RequirePermission(Permissions.WorkflowConfigurationCreate)]
        public async Task<IActionResult> Create([FromBody] CreateWorkflowModuleStageDto dto)
        {
            var response = await _service.CreateStageAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.WorkflowConfigurationUpdate)]
        public async Task<IActionResult> Update([FromBody] UpdateWorkflowModuleStageDto dto)
        {
            var response = await _service.UpdateStageAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{stageId}")]
        [RequirePermission(Permissions.WorkflowConfigurationView)]
        public async Task<IActionResult> GetById(int stageId)
        {
            var response = await _service.GetStageByIdAsync(stageId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [RequirePermission(Permissions.WorkflowConfigurationView)]
        public async Task<IActionResult> GetAll([FromQuery] int moduleId) => Ok(await _service.GetAllStagesAsync(moduleId));

        [HttpDelete("{stageId}")]
        [RequirePermission(Permissions.WorkflowConfigurationDelete)]
        public async Task<IActionResult> Delete(int stageId)
        {
            var response = await _service.DeleteStageAsync(stageId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
