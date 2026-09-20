using AssetFlow.Data.Entities.ACL;

namespace AssetFlow.Data.Entities.Tenant
{
    public class TenantResource
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }
        public int ResourceId { get; set; }
        public virtual Resource Resource { get; set; }
    }
}
