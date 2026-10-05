using AssetFlow.Data.Entities.Asset;
using AssetFlow.Data.Entities.SparePart;
using AssetFlow.Data.Entities.Tenant;
using AssetFlow.Data.Entities.WorkOrder;
using AssetFlow.Data.Identity;
using System.ComponentModel.DataAnnotations;
using WorkOrderEntity = AssetFlow.Data.Entities.WorkOrder.WorkOrder;
using AssetEntity = AssetFlow.Data.Entities.Asset.Asset;

namespace AssetFlow.Data.Entities.History
{
    public class MaintenanceCostRecord : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        public int AssetId { get; set; }
        public virtual AssetEntity Asset { get; set; }

        public int? WorkOrderId { get; set; }
        public virtual WorkOrderEntity WorkOrder { get; set; }

        public int CostType { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        public DateTime CostDate { get; set; }

        public int? PartReplacementId { get; set; }
        public virtual PartReplacement PartReplacement { get; set; }

        public int? PartId { get; set; }
        public virtual Part Part { get; set; }

        public decimal? Quantity { get; set; }

        public decimal? UnitCost { get; set; }

        public int? LaborRecordId { get; set; }
        public virtual WorkOrderLaborRecord LaborRecord { get; set; }

        [StringLength(500)]
        public string ExternalServiceDescription { get; set; } = string.Empty;

        [StringLength(100)]
        public string ReferenceNumber { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Notes { get; set; } = string.Empty;

        public int CreatedByUserId { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
    }
}
