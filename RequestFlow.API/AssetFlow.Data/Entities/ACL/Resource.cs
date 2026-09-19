using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.ACL
{
    public class Resource
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string ResourceName { get; set; }

        public int? FeatureId { get; set; }
        public virtual Resource Feature { get; set; }
        public virtual ICollection<Resource> SubResources { get; set; } = new List<Resource>();

        public virtual ICollection<RoleResource> RoleResources { get; set; } = new List<RoleResource>();
    }
}
