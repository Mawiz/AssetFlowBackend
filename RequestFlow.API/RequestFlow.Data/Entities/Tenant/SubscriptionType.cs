using System.ComponentModel.DataAnnotations;

namespace RequestFlow.Data.Entities.Tenant
{
    public class SubscriptionType : BaseEnumModel
    {
        [StringLength(100)]
        public string Name { get; set; }   // e.g. Free, Trial, Premium

        [StringLength(500)]
        public string Description { get; set; }

        public virtual ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
    }
}
