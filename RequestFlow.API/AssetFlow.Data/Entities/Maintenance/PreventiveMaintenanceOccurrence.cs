using AssetFlow.Data.Entities.Tenant;
using AssetEntity = AssetFlow.Data.Entities.Asset.Asset;
using AssetFlow.Data.Identity;
using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.Maintenance
{
    public class PreventiveMaintenanceOccurrence : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        public int MaintenanceScheduleId { get; set; }
        public virtual MaintenanceSchedule MaintenanceSchedule { get; set; }

        public int AssetId { get; set; }
        public virtual AssetEntity Asset { get; set; }

        public int MaintenanceTypeId { get; set; }
        public virtual MaintenanceType MaintenanceType { get; set; }

        public int? MaintenanceChecklistId { get; set; }
        public virtual MaintenanceChecklist MaintenanceChecklist { get; set; }

        public int? ChecklistVersion { get; set; }

        public DateTime? ScheduledDate { get; set; }

        public DateTime DueDate { get; set; }

        public decimal? DueOperatingHours { get; set; }

        public decimal? DueCycles { get; set; }

        public int Status { get; set; }

        public DateTime? StartedAt { get; set; }

        public int? StartedByUserId { get; set; }
        public virtual ApplicationUser StartedByUser { get; set; }

        public DateTime? CompletedAt { get; set; }

        public int? CompletedByUserId { get; set; }
        public virtual ApplicationUser CompletedByUser { get; set; }

        [StringLength(2000)]
        public string Remarks { get; set; }

        /// <summary>Reserved for future Work Order integration.</summary>
        public int? WorkOrderId { get; set; }

        public virtual ICollection<PreventiveMaintenanceOccurrenceChecklistItem> ChecklistItems { get; set; } =
            new List<PreventiveMaintenanceOccurrenceChecklistItem>();
    }
}
