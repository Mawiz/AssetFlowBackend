using RequestFlow.Services.Dto;
using RequestFlow.Services.Dto.User;

namespace RequestFlow.Services.Contracts
{
    public interface IUserService
    {
        Task<ResponseDto<string>> CreateAsync(UserCreateDto model);
        Task<ResponseDto<string>> UpdateAsync(UserUpdateDto model);
        Task<ResponseDto<UserViewDto>> GetByIdAsync(int userId);
        Task<ResponseDto<List<GetUserDto>>> FilterAsync(FilterDto model);
        Task<ResponseDto<UserToggleDto>> ToggleActiveAsync(UserToggleDto model);
        Task<ResponseDto<LoginResponseDto>> LoginAsync(LoginDto model);
        ResponseDto<string> RefreshToken(TokenDto model);
        Task<ResponseDto<LoginResponseDto>> ChangePasswordAsync(ChangePasswordDto model);
        Task<ResponseDto<string>> ForgotPasswordAsync(ForgetPasswordDto model);
        Task<ResponseDto<string>> ResetPasswordAsync(ResetPasswordDto model);
        Task<ResponseDto<List<UserByRoleDto>>> GetByRoleAsync(UserByRoleRequestDto model);
    }
}
