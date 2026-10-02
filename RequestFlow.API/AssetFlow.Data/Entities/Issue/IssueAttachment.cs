using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.Issue
{
    public class IssueAttachment : BaseModel
    {
        public int AssetIssueId { get; set; }
        public virtual AssetIssue AssetIssue { get; set; }

        [StringLength(260)]
        public string FileName { get; set; } = string.Empty;

        [StringLength(120)]
        public string ContentType { get; set; } = string.Empty;

        [StringLength(500)]
        public string StoragePath { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }
    }
}
