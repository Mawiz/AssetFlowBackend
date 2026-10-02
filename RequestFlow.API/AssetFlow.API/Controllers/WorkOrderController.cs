using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.WorkOrder;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkOrderController : BaseAppController
    {
        private readonly IWorkOrderService _service;

        public WorkOrderController(IWorkOrderService service) => _service = service;

        [HttpPost("CreateFromIssue")]
        [RequirePermission(Permissions.WorkOrderCreate)]
        public async Task<IActionResult> CreateFromIssue([FromBody] CreateWorkOrderFromIssueDto dto)
        {
            var response = await _service.CreateFromIssueAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("CreateFromOccurrence")]
        [RequirePermission(Permissions.WorkOrderCreate)]
        public async Task<IActionResult> CreateFromOccurrence([FromBody] CreateWorkOrderFromOccurrenceDto dto)
        {
            var response = await _service.CreateFromOccurrenceAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.WorkOrderUpdate)]
        public async Task<IActionResult> Update([FromBody] UpdateWorkOrderDto dto)
        {
            var response = await _service.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.WorkOrderView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Filter")]
        [RequirePermission(Permissions.WorkOrderView)]
        public async Task<IActionResult> Filter([FromBody] WorkOrderFilterDto model) => Ok(await _service.FilterAsync(model));

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.WorkOrderDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Assign")]
        [RequirePermission(Permissions.WorkOrderAssign)]
        public async Task<IActionResult> Assign([FromBody] WorkOrderAssignDto dto)
        {
            var response = await _service.AssignAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Reassign")]
        [RequirePermission(Permissions.WorkOrderReassign)]
        public async Task<IActionResult> Reassign([FromBody] WorkOrderAssignDto dto)
        {
            var response = await _service.ReassignAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Accept")]
        [RequirePermission(Permissions.WorkOrderAccept)]
        public async Task<IActionResult> Accept([FromBody] WorkOrderActionRemarksDto dto)
        {
            var response = await _service.AcceptAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("EngineerArrived")]
        [RequirePermission(Permissions.WorkOrderStart)]
        public async Task<IActionResult> EngineerArrived([FromBody] WorkOrderEngineerArrivedDto dto)
        {
            var response = await _service.EngineerArrivedAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Start")]
        [RequirePermission(Permissions.WorkOrderStart)]
        public async Task<IActionResult> Start([FromBody] WorkOrderActionRemarksDto dto)
        {
            var response = await _service.StartAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Pause")]
        [RequirePermission(Permissions.WorkOrderPause)]
        public async Task<IActionResult> Pause([FromBody] WorkOrderActionRemarksDto dto)
        {
            var response = await _service.PauseAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("WaitingForParts")]
        [RequirePermission(Permissions.WorkOrderPause)]
        public async Task<IActionResult> WaitingForParts([FromBody] WorkOrderActionRemarksDto dto)
        {
            var response = await _service.WaitingForPartsAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Resume")]
        [RequirePermission(Permissions.WorkOrderResume)]
        public async Task<IActionResult> Resume([FromBody] WorkOrderActionRemarksDto dto)
        {
            var response = await _service.ResumeAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Diagnosis")]
        [RequirePermission(Permissions.WorkOrderUpdate)]
        public async Task<IActionResult> Diagnosis([FromBody] WorkOrderDiagnosisUpsertDto dto)
        {
            var response = await _service.UpsertDiagnosisAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("WorkPerformed")]
        [RequirePermission(Permissions.WorkOrderUpdate)]
        public async Task<IActionResult> WorkPerformed([FromBody] WorkOrderWorkPerformedDto dto)
        {
            var response = await _service.UpdateWorkPerformedAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Complete")]
        [RequirePermission(Permissions.WorkOrderComplete)]
        public async Task<IActionResult> Complete([FromBody] WorkOrderCompleteDto dto)
        {
            var response = await _service.CompleteAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Approve")]
        [RequirePermission(Permissions.WorkOrderApprove)]
        public async Task<IActionResult> Approve([FromBody] WorkOrderApproveDto dto)
        {
            var response = await _service.ApproveAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Reject")]
        [RequirePermission(Permissions.WorkOrderReject)]
        public async Task<IActionResult> Reject([FromBody] WorkOrderRejectDto dto)
        {
            var response = await _service.RejectAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Reopen")]
        [RequirePermission(Permissions.WorkOrderReopen)]
        public async Task<IActionResult> Reopen([FromBody] WorkOrderReopenDto dto)
        {
            var response = await _service.ReopenAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Cancel")]
        [RequirePermission(Permissions.WorkOrderCancel)]
        public async Task<IActionResult> Cancel([FromBody] WorkOrderCancelDto dto)
        {
            var response = await _service.CancelAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("{id}/Attachments")]
        [RequirePermission(Permissions.WorkOrderUpdate)]
        public async Task<IActionResult> UploadAttachment(int id, IFormFile file)
        {
            var response = await _service.AddAttachmentAsync(id, file);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("Attachments/{attachmentId}")]
        [RequirePermission(Permissions.WorkOrderUpdate)]
        public async Task<IActionResult> DeleteAttachment(int attachmentId)
        {
            var response = await _service.DeleteAttachmentAsync(attachmentId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("Attachments/{attachmentId}/download")]
        [RequirePermission(Permissions.WorkOrderView)]
        public async Task<IActionResult> DownloadAttachment(int attachmentId)
        {
            var file = await _service.GetAttachmentStreamAsync(attachmentId);
            if (file == null) return NotFound();
            return File(file.Value.Stream, file.Value.ContentType, file.Value.FileName);
        }
    }
}
