using AssetFlow.Data.Entities.Tenant;
using LocationEntity = AssetFlow.Data.Entities.Location.Location;
using AssetFlow.Data.Identity;
using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.Asset
{
    public class Asset : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        [StringLength(50)]
        public string AssetCode { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        public int AssetCategoryId { get; set; }
        public virtual AssetCategory AssetCategory { get; set; }

        public int AssetTypeId { get; set; }
        public virtual AssetType AssetType { get; set; }

        [StringLength(200)]
        public string Manufacturer { get; set; }

        [StringLength(200)]
        public string Model { get; set; }

        [StringLength(100)]
        public string SerialNumber { get; set; }

        public DateTime? InstallationDate { get; set; }

        public int LocationId { get; set; }
        public virtual LocationEntity Location { get; set; }

        public int? ResponsibleUserId { get; set; }
        public virtual ApplicationUser ResponsibleUser { get; set; }

        [StringLength(1000)]
        public string OtherLocationInformation { get; set; }

        public int Status { get; set; }

        public int Criticality { get; set; }

        public DateTime? WarrantyStartDate { get; set; }
        public DateTime? WarrantyEndDate { get; set; }

        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchaseCost { get; set; }

        [StringLength(200)]
        public string SupplierName { get; set; }

        public int? ExpectedLifeValue { get; set; }
        public int? ExpectedLifeUnit { get; set; }

        [StringLength(2000)]
        public string Notes { get; set; }

        public virtual ICollection<AssetComponent> Components { get; set; } = new List<AssetComponent>();
    }
}
