using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.ACL
{
    public class Resource
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string ResourceName { get; set; }

        [StringLength(200)]
        public string Verb { get; set; } = string.Empty;
        public bool IsBackEnd { get; set; }

        // 🔹 Self-referencing relationship
        public int? FeatureId { get; set; }       // Null → this is a Feature/Module
        public virtual Resource Feature { get; set; }  // Parent Feature
        public virtual ICollection<Resource> SubResources { get; set; } = new List<Resource>(); // Child permissions

        public virtual ICollection<RoleResource> RoleResources { get; set; } = new List<RoleResource>();
    }
}
