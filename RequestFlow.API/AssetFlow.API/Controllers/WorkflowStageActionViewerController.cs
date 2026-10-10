using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Workflow;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowStageActionViewerController : BaseAppController
    {
        private readonly IWorkflowConfigurationService _service;

        public WorkflowStageActionViewerController(IWorkflowConfigurationService service) => _service = service;

        [HttpPost]
        [RequirePermission(Permissions.WorkflowConfigurationCreate)]
        public async Task<IActionResult> Create([FromBody] CreateWorkflowStageActionViewerDto dto)
        {
            var response = await _service.CreateStageActionViewerAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.WorkflowConfigurationUpdate)]
        public async Task<IActionResult> Update([FromBody] UpdateWorkflowStageActionViewerDto dto)
        {
            var response = await _service.UpdateStageActionViewerAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{stageActionViewerId}")]
        [RequirePermission(Permissions.WorkflowConfigurationView)]
        public async Task<IActionResult> GetById(int stageActionViewerId)
        {
            var response = await _service.GetStageActionViewerByIdAsync(stageActionViewerId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [RequirePermission(Permissions.WorkflowConfigurationView)]
        public async Task<IActionResult> GetAll([FromQuery] int stageId) => Ok(await _service.GetAllStageActionViewersAsync(stageId));

        [HttpDelete("{stageActionViewerId}")]
        [RequirePermission(Permissions.WorkflowConfigurationDelete)]
        public async Task<IActionResult> Delete(int stageActionViewerId)
        {
            var response = await _service.DeleteStageActionViewerAsync(stageActionViewerId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
