using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Workflow;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowActionController : BaseAppController
    {
        private readonly IWorkflowConfigurationService _service;

        public WorkflowActionController(IWorkflowConfigurationService service) => _service = service;

        [HttpPost]
        [RequirePermission(Permissions.WorkflowConfigurationCreate)]
        public async Task<IActionResult> Create([FromBody] CreateWorkflowActionDto dto)
        {
            var response = await _service.CreateActionAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.WorkflowConfigurationUpdate)]
        public async Task<IActionResult> Update([FromBody] UpdateWorkflowActionDto dto)
        {
            var response = await _service.UpdateActionAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{actionId}")]
        [RequirePermission(Permissions.WorkflowConfigurationView)]
        public async Task<IActionResult> GetById(int actionId)
        {
            var response = await _service.GetActionByIdAsync(actionId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [RequirePermission(Permissions.WorkflowConfigurationView)]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllActionsAsync());

        [HttpPost("Filter")]
        [RequirePermission(Permissions.WorkflowConfigurationView)]
        public async Task<IActionResult> Filter([FromBody] WorkflowActionFilterDto model) => Ok(await _service.FilterActionsAsync(model));

        [HttpDelete("{actionId}")]
        [RequirePermission(Permissions.WorkflowConfigurationDelete)]
        public async Task<IActionResult> Delete(int actionId)
        {
            var response = await _service.DeleteActionAsync(actionId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
