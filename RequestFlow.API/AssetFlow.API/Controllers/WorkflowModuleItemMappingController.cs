using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Workflow;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowModuleItemMappingController : BaseAppController
    {
        private readonly IWorkflowModuleItemMappingService _service;

        public WorkflowModuleItemMappingController(IWorkflowModuleItemMappingService service) => _service = service;

        [HttpPost]
        [RequirePermission(Permissions.WorkflowModuleItemMappingCreate)]
        public async Task<IActionResult> Create([FromBody] CreateWorkflowModuleItemMappingDto dto)
        {
            var response = await _service.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.WorkflowModuleItemMappingUpdate)]
        public async Task<IActionResult> Update([FromBody] UpdateWorkflowModuleItemMappingDto dto)
        {
            var response = await _service.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.WorkflowModuleItemMappingView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [RequirePermission(Permissions.WorkflowModuleItemMappingView)]
        public async Task<IActionResult> GetAll([FromQuery] int? tenantId) => Ok(await _service.GetAllAsync(tenantId));

        [HttpPost("Filter")]
        [RequirePermission(Permissions.WorkflowModuleItemMappingView)]
        public async Task<IActionResult> Filter([FromBody] WorkflowModuleItemMappingFilterDto model) => Ok(await _service.FilterAsync(model));

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.WorkflowModuleItemMappingDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
