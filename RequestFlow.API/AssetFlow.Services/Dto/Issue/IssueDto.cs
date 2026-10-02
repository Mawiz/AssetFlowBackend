using AssetFlow.Services.Dto;

namespace AssetFlow.Services.Dto.Issue
{
    public class IssueCategoryDto
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateIssueCategoryDto
    {
        public int? TenantId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateIssueCategoryDto : CreateIssueCategoryDto
    {
        public int Id { get; set; }
    }

    public class IssueCategoryFilterDto : SearchViewDto { }

    public class IssueAttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSizeBytes { get; set; }
        public string DownloadUrl { get; set; }
    }

    public class AssetIssueDto
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
        public string IssueNumber { get; set; }
        public int AssetId { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public string AssetSerialNumber { get; set; }
        public int AssetCriticality { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public string LocationDisplayPath { get; set; }
        public int ReportedByUserId { get; set; }
        public string ReportedByUserName { get; set; }
        public DateTime ReportedAt { get; set; }
        public int IssueCategoryId { get; set; }
        public string IssueCategoryName { get; set; }
        public int Priority { get; set; }
        public string Description { get; set; }
        public int AssetStatusAtReport { get; set; }
        public string ImmediateAction { get; set; }
        public int Status { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public int? ResolvedByUserId { get; set; }
        public string ResolvedByUserName { get; set; }
        public string ResolutionRemarks { get; set; }
        public int? WorkOrderId { get; set; }
        public List<IssueAttachmentDto> Attachments { get; set; } = new();
    }

    public class CreateAssetIssueDto
    {
        public int? TenantId { get; set; }
        public int AssetId { get; set; }
        public int IssueCategoryId { get; set; }
        public int Priority { get; set; }
        public string Description { get; set; }
        public int AssetStatusAtReport { get; set; }
        public string ImmediateAction { get; set; }
        public DateTime? ReportedAt { get; set; }
    }

    public class UpdateAssetIssueDto
    {
        public int Id { get; set; }
        public int IssueCategoryId { get; set; }
        public int Priority { get; set; }
        public string Description { get; set; }
        public int AssetStatusAtReport { get; set; }
        public string ImmediateAction { get; set; }
    }

    public class ChangeAssetIssueStatusDto
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public string ResolutionRemarks { get; set; }
    }

    public class AssetIssueFilterDto : SearchViewDto
    {
        public int? AssetId { get; set; }
        public int? IssueCategoryId { get; set; }
        public int? Priority { get; set; }
        public int? Status { get; set; }
        public int? LocationId { get; set; }
        public int? ReportedByUserId { get; set; }
        public DateTime? ReportedFrom { get; set; }
        public DateTime? ReportedTo { get; set; }
    }
}
