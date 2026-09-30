using AssetFlow.Data.Entities.Tenant;
using System.ComponentModel.DataAnnotations;
using LocationEntity = AssetFlow.Data.Entities.Location.Location;

namespace AssetFlow.Data.Entities.SparePart
{
    public class PartInventoryBatch : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        public int PartInventoryId { get; set; }
        public virtual PartInventory PartInventory { get; set; }

        public int PartId { get; set; }
        public virtual Part Part { get; set; }

        [StringLength(50)]
        public string BatchReference { get; set; }

        public int? SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        public DateTime? ReceivedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public int? ExpectedLifeValue { get; set; }
        public int? ExpectedLifeUnit { get; set; }

        public decimal TotalQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal FaultyQuantity { get; set; }
        public decimal QuarantineQuantity { get; set; }
        public decimal IssuedQuantity { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public virtual ICollection<PartSerialNumber> SerialNumbers { get; set; } = new List<PartSerialNumber>();
    }
}
