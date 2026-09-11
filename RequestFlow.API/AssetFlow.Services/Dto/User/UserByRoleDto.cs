namespace AssetFlow.Services.Dto.User
{
    public class UserByRoleDto : FilterViewDto
    {
        public string DisplayName { get; set; }
        public List<int> RoleIds { get; set; } = new();
    }
}
