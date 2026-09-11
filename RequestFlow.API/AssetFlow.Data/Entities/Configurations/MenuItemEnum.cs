using RequestFlow.Data.Identity;

namespace RequestFlow.Data.Entities.Configurations
{
    public class MenuItemEnum : BaseEnumModel
    {
        public int ApplicationRoleId { get; set; }
        public ApplicationRole ApplicationRole { get; set; }
    }
}
