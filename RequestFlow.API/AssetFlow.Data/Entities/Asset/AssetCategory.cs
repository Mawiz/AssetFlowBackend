using AssetFlow.Data.Entities.Tenant;
using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.Asset
{
    public class AssetCategory : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Code { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public virtual ICollection<AssetType> AssetTypes { get; set; } = new List<AssetType>();
    }
}
