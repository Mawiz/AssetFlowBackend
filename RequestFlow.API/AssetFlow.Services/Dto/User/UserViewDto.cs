namespace AssetFlow.Services.Dto.User
{
    public class UserViewDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<int> RoleIds { get; set; } = new();
        public List<string> RoleNames { get; set; } = new();
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
    }
}
