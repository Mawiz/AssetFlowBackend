using AssetFlow.Data.Entities.Tenant;
using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.SparePart
{
    public class Part : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        [StringLength(50)]
        public string PartNumber { get; set; }

        [StringLength(200)]
        public string PartName { get; set; }

        public int PartCategoryId { get; set; }
        public virtual PartCategory PartCategory { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(200)]
        public string Manufacturer { get; set; }

        [StringLength(200)]
        public string SupplierName { get; set; }

        [StringLength(50)]
        public string UnitOfMeasure { get; set; }

        public int? ExpectedLifeValue { get; set; }
        public int? ExpectedLifeUnit { get; set; }

        public decimal MinStockLevel { get; set; }
        public decimal? MaxStockLevel { get; set; }

        public bool IsSerialized { get; set; }

        public virtual ICollection<PartSerialNumber> SerialNumbers { get; set; } = new List<PartSerialNumber>();
        public virtual ICollection<PartInventory> InventoryItems { get; set; } = new List<PartInventory>();
    }
}
