using AssetFlow.Data.Identity;

namespace AssetFlow.Data.Entities.ACL
{
    public class RoleResource
    {
        public int Id { get; set; }
        public int ApplicationRoleId { get; set; }
        public virtual ApplicationRole ApplicationRole { get; set; }
        public int ResourceId { get; set; }
        public virtual Resource Resource { get; set; }
    }
}
