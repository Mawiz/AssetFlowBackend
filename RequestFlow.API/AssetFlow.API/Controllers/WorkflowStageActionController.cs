using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Workflow;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowStageActionController : BaseAppController
    {
        private readonly IWorkflowConfigurationService _service;

        public WorkflowStageActionController(IWorkflowConfigurationService service) => _service = service;

        [HttpPost]
        [RequirePermission(Permissions.WorkflowConfigurationCreate)]
        public async Task<IActionResult> Create([FromBody] CreateWorkflowStageActionDto dto)
        {
            var response = await _service.CreateStageActionAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.WorkflowConfigurationUpdate)]
        public async Task<IActionResult> Update([FromBody] UpdateWorkflowStageActionDto dto)
        {
            var response = await _service.UpdateStageActionAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{stageActionId}")]
        [RequirePermission(Permissions.WorkflowConfigurationView)]
        public async Task<IActionResult> GetById(int stageActionId)
        {
            var response = await _service.GetStageActionByIdAsync(stageActionId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [RequirePermission(Permissions.WorkflowConfigurationView)]
        public async Task<IActionResult> GetAll([FromQuery] int stageId) => Ok(await _service.GetAllStageActionsAsync(stageId));

        [HttpDelete("{stageActionId}")]
        [RequirePermission(Permissions.WorkflowConfigurationDelete)]
        public async Task<IActionResult> Delete(int stageActionId)
        {
            var response = await _service.DeleteStageActionAsync(stageActionId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
