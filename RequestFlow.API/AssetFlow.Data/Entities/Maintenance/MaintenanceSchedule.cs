using AssetFlow.Data.Entities.Tenant;
using AssetEntity = AssetFlow.Data.Entities.Asset.Asset;
using AssetFlow.Data.Identity;
using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.Maintenance
{
    public class MaintenanceSchedule : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        public int AssetId { get; set; }
        public virtual AssetEntity Asset { get; set; }

        public int MaintenanceTypeId { get; set; }
        public virtual MaintenanceType MaintenanceType { get; set; }

        public int? MaintenanceChecklistId { get; set; }
        public virtual MaintenanceChecklist MaintenanceChecklist { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public int RecurrenceType { get; set; }

        public int IntervalValue { get; set; } = 1;

        /// <summary>DayOfWeek (0=Sunday..6=Saturday) for weekly recurrence.</summary>
        public int? DayOfWeek { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? NextDueDate { get; set; }

        public decimal? NextDueOperatingHours { get; set; }

        public decimal? NextDueCycles { get; set; }

        public DateTime? LastGeneratedDate { get; set; }

        public DateTime? LastOccurrenceDate { get; set; }

        public DateTime? LastCompletedDate { get; set; }

        public int? ResponsibleUserId { get; set; }
        public virtual ApplicationUser ResponsibleUser { get; set; }

        public int GenerationHorizonDays { get; set; } = 90;
    }
}
