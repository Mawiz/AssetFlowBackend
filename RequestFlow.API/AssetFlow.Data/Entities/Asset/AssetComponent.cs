using AssetFlow.Data.Entities.Tenant;
using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.Asset
{
    public class AssetComponent : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        public int AssetId { get; set; }
        public virtual Asset Asset { get; set; }

        [StringLength(50)]
        public string ComponentCode { get; set; }

        [StringLength(200)]
        public string ComponentName { get; set; }

        [StringLength(100)]
        public string PartNumber { get; set; }

        [StringLength(100)]
        public string SerialNumber { get; set; }

        [StringLength(200)]
        public string Manufacturer { get; set; }

        public DateTime? InstallationDate { get; set; }

        public int? ExpectedLifeValue { get; set; }
        public int? ExpectedLifeUnit { get; set; }

        public int CurrentStatus { get; set; }

        [StringLength(200)]
        public string SupplierName { get; set; }

        public DateTime? WarrantyStartDate { get; set; }
        public DateTime? WarrantyEndDate { get; set; }

        [StringLength(500)]
        public string InstallationLocation { get; set; }

        public decimal? CurrentRunningHours { get; set; }

        [StringLength(2000)]
        public string Notes { get; set; }
    }
}
