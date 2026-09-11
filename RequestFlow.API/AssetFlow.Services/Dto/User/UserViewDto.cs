namespace AssetFlow.Services.Dto.User
{
    public class UserViewDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
    }
}
