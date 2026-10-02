using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Issue;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetIssueController : BaseAppController
    {
        private readonly IAssetIssueService _service;

        public AssetIssueController(IAssetIssueService service) => _service = service;

        [HttpPost]
        [RequirePermission(Permissions.AssetIssueCreate)]
        public async Task<IActionResult> Create([FromBody] CreateAssetIssueDto dto)
        {
            var response = await _service.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.AssetIssueUpdate)]
        public async Task<IActionResult> Update([FromBody] UpdateAssetIssueDto dto)
        {
            var response = await _service.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("ChangeStatus")]
        [RequirePermission(Permissions.AssetIssueUpdate)]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeAssetIssueStatusDto dto)
        {
            var response = await _service.ChangeStatusAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.AssetIssueView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Filter")]
        [RequirePermission(Permissions.AssetIssueView)]
        public async Task<IActionResult> Filter([FromBody] AssetIssueFilterDto model) => Ok(await _service.FilterAsync(model));

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.AssetIssueDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("{id}/Attachments")]
        [RequirePermission(Permissions.AssetIssueUpdate)]
        public async Task<IActionResult> UploadAttachment(int id, IFormFile file)
        {
            var response = await _service.AddAttachmentAsync(id, file);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("Attachments/{attachmentId}")]
        [RequirePermission(Permissions.AssetIssueUpdate)]
        public async Task<IActionResult> DeleteAttachment(int attachmentId)
        {
            var response = await _service.DeleteAttachmentAsync(attachmentId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("Attachments/{attachmentId}/download")]
        [RequirePermission(Permissions.AssetIssueView)]
        public async Task<IActionResult> DownloadAttachment(int attachmentId)
        {
            var file = await _service.GetAttachmentStreamAsync(attachmentId);
            if (file == null) return NotFound();
            return File(file.Value.Stream, file.Value.ContentType, file.Value.FileName);
        }
    }
}
