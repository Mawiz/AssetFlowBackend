using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Issue;
using Microsoft.AspNetCore.Http;

namespace AssetFlow.Services.Contracts
{
    public interface IAssetIssueService
    {
        Task<ResponseDto<AssetIssueDto>> CreateAsync(CreateAssetIssueDto dto);
        Task<ResponseDto<AssetIssueDto>> UpdateAsync(UpdateAssetIssueDto dto);
        Task<ResponseDto<AssetIssueDto>> GetByIdAsync(int id);
        Task<ResponseDto<object>> FilterAsync(AssetIssueFilterDto model);
        Task<ResponseDto<AssetIssueDto>> ChangeStatusAsync(ChangeAssetIssueStatusDto dto);
        Task<ResponseDto<bool>> DeleteAsync(int id);
        Task<ResponseDto<IssueAttachmentDto>> AddAttachmentAsync(int issueId, IFormFile file);
        Task<ResponseDto<bool>> DeleteAttachmentAsync(int attachmentId);
        Task<(Stream Stream, string ContentType, string FileName)?> GetAttachmentStreamAsync(int attachmentId);
    }
}
