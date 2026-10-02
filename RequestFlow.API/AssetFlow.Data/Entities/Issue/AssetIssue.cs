using AssetFlow.Data.Entities.Tenant;
using AssetEntity = AssetFlow.Data.Entities.Asset.Asset;
using LocationEntity = AssetFlow.Data.Entities.Location.Location;
using AssetFlow.Data.Identity;
using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.Issue
{
    public class AssetIssue : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        [StringLength(30)]
        public string IssueNumber { get; set; } = string.Empty;

        public int AssetId { get; set; }
        public virtual AssetEntity Asset { get; set; }

        public int LocationId { get; set; }
        public virtual LocationEntity Location { get; set; }

        [StringLength(1000)]
        public string LocationDisplayPath { get; set; } = string.Empty;

        public int ReportedByUserId { get; set; }
        public virtual ApplicationUser ReportedByUser { get; set; }

        public DateTime ReportedAt { get; set; }

        public int IssueCategoryId { get; set; }
        public virtual IssueCategory IssueCategory { get; set; }

        public int Priority { get; set; }

        [StringLength(4000)]
        public string Description { get; set; } = string.Empty;

        public int AssetStatusAtReport { get; set; }

        [StringLength(2000)]
        public string ImmediateAction { get; set; } = string.Empty;

        public int Status { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public int? ResolvedByUserId { get; set; }
        public virtual ApplicationUser ResolvedByUser { get; set; }

        [StringLength(2000)]
        public string ResolutionRemarks { get; set; } = string.Empty;

        /// <summary>Reserved for Phase 7 Work Order integration.</summary>
        public int? WorkOrderId { get; set; }

        public virtual ICollection<IssueAttachment> Attachments { get; set; } = new List<IssueAttachment>();
    }
}
