using Microsoft.AspNetCore.Identity;
using AssetFlow.Data.Entities.ACL;
using AssetFlow.Data.Entities.Tenant;
using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Identity
{
    public class ApplicationRole : IdentityRole<int>, ITenancyModel
    {
        [StringLength(100)]
        public string DisplayName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public double Order { get; set; }

        public DateTime CreatedOn { get; set; }

        public int? CreatedById { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public int? ModifiedById { get; set; }
        public virtual ApplicationUser ModifiedBy { get; set; }

        public int? TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();

        public virtual ICollection<RoleResource> RoleResources { get; set; } = new List<RoleResource>();

    }
}
