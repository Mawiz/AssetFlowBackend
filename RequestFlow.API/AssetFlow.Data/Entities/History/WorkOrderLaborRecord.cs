using AssetFlow.Data.Entities.Tenant;
using AssetFlow.Data.Identity;
using System.ComponentModel.DataAnnotations;
using WorkOrderEntity = AssetFlow.Data.Entities.WorkOrder.WorkOrder;

namespace AssetFlow.Data.Entities.History
{
    public class WorkOrderLaborRecord : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        public int WorkOrderId { get; set; }
        public virtual WorkOrderEntity WorkOrder { get; set; }

        public int UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int DurationMinutes { get; set; }

        public decimal? HourlyRate { get; set; }

        public decimal LaborCost { get; set; }

        [StringLength(2000)]
        public string Notes { get; set; } = string.Empty;
    }
}
