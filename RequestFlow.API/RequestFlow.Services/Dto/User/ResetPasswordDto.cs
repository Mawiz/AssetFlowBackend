namespace RequestFlow.Services.Dto.User
{
    public class ResetPasswordDto
    {
        public string Id { get; set; }
        public string NewPassword { get; set; }
        public string Token { get; set; }
        public string Otp { get; set; }   // new property for OTP
    }
}
