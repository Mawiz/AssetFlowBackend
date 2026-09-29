using AssetFlow.Data.Entities.Tenant;
using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.SparePart
{
    public class Supplier : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Code { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(200)]
        public string ContactName { get; set; }

        [StringLength(100)]
        public string ContactPhone { get; set; }

        [StringLength(200)]
        public string ContactEmail { get; set; }

        [StringLength(500)]
        public string Address { get; set; }
    }
}
