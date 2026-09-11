namespace AssetFlow.Services.Dto.User
{
    public class UserUpdateDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<int> RoleIds { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? TenantId { get; set; }
    }
}
