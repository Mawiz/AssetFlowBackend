namespace RequestFlow.Services.Dto.User
{
    public class ChangePasswordDto
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string NewPassword { get; set; }
    }
}
