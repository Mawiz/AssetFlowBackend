using AssetFlow.Data.Entities.Tenant;
using AssetEntity = AssetFlow.Data.Entities.Asset.Asset;
using LocationEntity = AssetFlow.Data.Entities.Location.Location;
using AssetFlow.Data.Entities.Issue;
using AssetFlow.Data.Entities.Maintenance;
using AssetFlow.Data.Identity;
using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.WorkOrder
{
    public class WorkOrder : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        [StringLength(30)]
        public string WorkOrderNumber { get; set; } = string.Empty;

        public int SourceType { get; set; }

        public int? AssetIssueId { get; set; }
        public virtual AssetIssue AssetIssue { get; set; }

        public int? PreventiveMaintenanceOccurrenceId { get; set; }
        public virtual PreventiveMaintenanceOccurrence PreventiveMaintenanceOccurrence { get; set; }

        public int AssetId { get; set; }
        public virtual AssetEntity Asset { get; set; }

        public int LocationId { get; set; }
        public virtual LocationEntity Location { get; set; }

        [StringLength(1000)]
        public string LocationDisplayPath { get; set; } = string.Empty;

        public int Priority { get; set; }

        public int Status { get; set; }

        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        [StringLength(4000)]
        public string Description { get; set; } = string.Empty;

        public int? AssignedByUserId { get; set; }
        public virtual ApplicationUser AssignedByUser { get; set; }

        public int? AssignedToUserId { get; set; }
        public virtual ApplicationUser AssignedToUser { get; set; }

        public DateTime? AssignedAt { get; set; }

        public DateTime? AcceptedAt { get; set; }

        public DateTime? EngineerArrivedAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? PausedAt { get; set; }

        public DateTime? ResumedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public int? CompletedByUserId { get; set; }
        public virtual ApplicationUser CompletedByUser { get; set; }

        public DateTime? AssetRestoredAt { get; set; }

        public int? AssetStatusAfterWork { get; set; }

        public DateTime? ClosedAt { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(4000)]
        public string WorkPerformed { get; set; } = string.Empty;

        [StringLength(2000)]
        public string FinalResult { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Remarks { get; set; } = string.Empty;

        [StringLength(2000)]
        public string RejectionReason { get; set; } = string.Empty;

        [StringLength(2000)]
        public string CancellationReason { get; set; } = string.Empty;

        [StringLength(2000)]
        public string ApprovalRemarks { get; set; } = string.Empty;

        public int? ApprovedByUserId { get; set; }
        public virtual ApplicationUser ApprovedByUser { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public virtual WorkOrderDiagnosis Diagnosis { get; set; }

        public virtual ICollection<WorkOrderStatusHistory> StatusHistory { get; set; } = new List<WorkOrderStatusHistory>();

        public virtual ICollection<WorkOrderAssignmentHistory> AssignmentHistory { get; set; } = new List<WorkOrderAssignmentHistory>();

        public virtual ICollection<WorkOrderAttachment> Attachments { get; set; } = new List<WorkOrderAttachment>();
    }
}
