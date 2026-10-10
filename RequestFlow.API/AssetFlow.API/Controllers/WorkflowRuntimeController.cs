using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Workflow;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowRuntimeController : BaseAppController
    {
        private readonly IWorkflowRuntimeService _service;

        public WorkflowRuntimeController(IWorkflowRuntimeService service) => _service = service;

        [HttpGet("master/{workflowMasterId:guid}")]
        [RequirePermission(Permissions.WorkflowRuntimeView)]
        public async Task<IActionResult> GetMaster(Guid workflowMasterId)
        {
            var response = await _service.GetMasterByIdAsync(workflowMasterId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("master/by-reference")]
        [RequirePermission(Permissions.WorkflowRuntimeView)]
        public async Task<IActionResult> GetMasterByReference([FromQuery] string referenceItemType, [FromQuery] int referenceItemId)
        {
            var response = await _service.GetMasterByReferenceAsync(referenceItemType, referenceItemId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("task/{workflowTaskId:guid}")]
        [RequirePermission(Permissions.WorkflowRuntimeView)]
        public async Task<IActionResult> GetTaskDetail(Guid workflowTaskId)
        {
            var response = await _service.GetTaskDetailAsync(workflowTaskId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("task/{workflowTaskId:guid}/actions")]
        [RequirePermission(Permissions.WorkflowRuntimeView)]
        public async Task<IActionResult> GetAvailableActions(Guid workflowTaskId)
        {
            var response = await _service.GetAvailableActionsAsync(workflowTaskId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("perform")]
        [RequirePermission(Permissions.WorkflowRuntimePerform)]
        public async Task<IActionResult> Perform([FromBody] PerformWorkflowActionDto dto)
        {
            var response = await _service.PerformActionAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("history/{workflowMasterId:guid}")]
        [RequirePermission(Permissions.WorkflowRuntimeView)]
        public async Task<IActionResult> GetHistory(Guid workflowMasterId)
        {
            var response = await _service.GetHistoryAsync(workflowMasterId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("inbox/Filter")]
        [RequirePermission(Permissions.WorkflowRuntimeView)]
        public async Task<IActionResult> FilterInbox([FromBody] WorkflowInboxFilterDto model) => Ok(await _service.FilterInboxAsync(model));

        [HttpGet("issue/{issueId:int}")]
        [RequirePermission(Permissions.WorkflowRuntimeView)]
        public async Task<IActionResult> GetIssueIntegration(int issueId, [FromQuery] Guid? workflowTaskId)
        {
            var response = await _service.GetIssueWorkflowIntegrationAsync(issueId, workflowTaskId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
