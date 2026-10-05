using AssetFlow.Data.Entities.Asset;
using AssetFlow.Data.Entities.Tenant;
using AssetFlow.Data.Entities.WorkOrder;
using AssetFlow.Data.Identity;
using System.ComponentModel.DataAnnotations;
using WorkOrderEntity = AssetFlow.Data.Entities.WorkOrder.WorkOrder;
using LocationEntity = AssetFlow.Data.Entities.Location.Location;
using AssetEntity = AssetFlow.Data.Entities.Asset.Asset;

namespace AssetFlow.Data.Entities.SparePart
{
    public class PartReplacement : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        public int WorkOrderId { get; set; }
        public virtual WorkOrderEntity WorkOrder { get; set; }

        public int AssetId { get; set; }
        public virtual AssetEntity Asset { get; set; }

        public int? OldAssetComponentId { get; set; }
        public virtual AssetComponent OldAssetComponent { get; set; }

        [StringLength(100)]
        public string OldPartNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string OldSerialNumber { get; set; } = string.Empty;

        public int? OldPartSerialNumberId { get; set; }
        public virtual PartSerialNumber OldPartSerialNumber { get; set; }

        public int NewPartId { get; set; }
        public virtual Part NewPart { get; set; }

        public int? NewPartSerialNumberId { get; set; }
        public virtual PartSerialNumber NewPartSerialNumber { get; set; }

        public int? NewPartInventoryBatchId { get; set; }
        public virtual PartInventoryBatch NewPartInventoryBatch { get; set; }

        public int? NewAssetComponentId { get; set; }
        public virtual AssetComponent NewAssetComponent { get; set; }

        public decimal Quantity { get; set; } = 1;

        public int InstalledByUserId { get; set; }
        public virtual ApplicationUser InstalledByUser { get; set; }

        public DateTime InstalledAt { get; set; }

        public int? RemovedByUserId { get; set; }
        public virtual ApplicationUser RemovedByUser { get; set; }

        public DateTime? RemovedAt { get; set; }

        [StringLength(2000)]
        public string RemovalReason { get; set; } = string.Empty;

        [StringLength(2000)]
        public string FailureReason { get; set; } = string.Empty;

        public int? FromLocationId { get; set; }
        public virtual LocationEntity FromLocation { get; set; }

        [StringLength(500)]
        public string InstallationLocation { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Remarks { get; set; } = string.Empty;

        public int? PartTransactionId { get; set; }
        public virtual PartTransaction PartTransaction { get; set; }
    }
}
