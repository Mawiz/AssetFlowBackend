using AssetFlow.Common.Enum;

namespace AssetFlow.Services.Dto.User
{
    public class UserCreateDto
    {
        public string FullName { get; set; }
        public char FirstLetter { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Enums.UserRole RoleId { get; set; }
    }
}
