using System.Collections.Generic;

namespace RequestFlow.Services.Dto.User
{
    public class LoginResponseDto
    {
        public bool IsReset { get; set; }
        public string Token { get; set; }
        public LoginUserDto User { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public List<string> MenuItems { get; set; } = new();

        public List<string> NavigationItems { get; set; } = new();

        public List<string> NavigationCreateItems { get; set; } = new();

    }
}
