using AssetFlow.Common.Enum;

namespace AssetFlow.Services.Dto.User
{
    public class UserUpdateDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Enums.UserRole RoleId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
