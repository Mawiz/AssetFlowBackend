using AssetFlow.Data.Entities.Tenant;
using System.ComponentModel.DataAnnotations;
using LocationEntity = AssetFlow.Data.Entities.Location.Location;

namespace AssetFlow.Data.Entities.SparePart
{
    public class PartSerialNumber : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        public int PartId { get; set; }
        public virtual Part Part { get; set; }

        public int? PartInventoryBatchId { get; set; }
        public virtual PartInventoryBatch PartInventoryBatch { get; set; }

        [StringLength(100)]
        public string SerialNumber { get; set; }

        public int Status { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public int? LocationId { get; set; }
        public virtual LocationEntity Location { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int? ExpectedLifeValue { get; set; }
        public int? ExpectedLifeUnit { get; set; }

        public DateTime? WarrantyStartDate { get; set; }
        public DateTime? WarrantyEndDate { get; set; }

        public int? SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        [StringLength(200)]
        public string SupplierSerialReference { get; set; }
    }
}
