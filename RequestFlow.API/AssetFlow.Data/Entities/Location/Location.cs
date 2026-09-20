using AssetFlow.Data.Entities.Tenant;
using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.Location
{
    public class Location : BaseModel, ITenancyModel
    {
        public int? TenantId { get; set; }
        public virtual Tenant.Tenant Tenant { get; set; }

        public int LocationTypeId { get; set; }
        public virtual LocationType LocationType { get; set; }

        public int? ParentLocationId { get; set; }
        public virtual Location ParentLocation { get; set; }
        public virtual ICollection<Location> ChildLocations { get; set; } = new List<Location>();

        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }
}
