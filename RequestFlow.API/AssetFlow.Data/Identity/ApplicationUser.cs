using Microsoft.AspNetCore.Identity;
using RequestFlow.Data.Entities.Tenant;
using System.ComponentModel.DataAnnotations;

namespace RequestFlow.Data.Identity
{
    public class ApplicationUser : IdentityUser<int>, ITenancyModel
    {
        public char FirstLetter { get; set; }

        [StringLength(100)]
        public string FullName { get; set; }

        public bool IsActive { get; set; }

        public bool IsReset { get; set; }

        [StringLength(20)]
        public string OtpCode { get; set; }

        public DateTime? OtpExpiry { get; set; }

        public bool IsPlatformUser { get; set; }

        public int? TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }

        public DateTime CreatedOn { get; set; }

        public int? CreatedById { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public int? ModifiedById { get; set; }
        public virtual ApplicationUser ModifiedBy { get; set; }

        public DateTime? LastActive { get; set; }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    }
}
