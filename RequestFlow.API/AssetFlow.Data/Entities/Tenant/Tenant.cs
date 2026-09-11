using AssetFlow.Data.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetFlow.Data.Entities.Tenant
{
    public class Tenant : BaseModel
    {
        [StringLength(255)]
        public string CompanyName { get; set; }

        public int SubscriptionTypeId { get; set; }
        public virtual SubscriptionType SubscriptionType { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        public virtual ICollection<ApplicationRole> Roles { get; set; } = new List<ApplicationRole>();

        public virtual ICollection<TenantLanguage> TenantLanguages { get; set; } = new List<TenantLanguage>();
    }
}
