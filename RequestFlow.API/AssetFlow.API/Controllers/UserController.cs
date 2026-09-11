using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.User;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseAppController
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpPost]
        [RequirePermission(Permissions.UserCreate)]
        public async Task<IActionResult> Create(UserCreateDto model)
        {
            return Ok(await userService.CreateAsync(model));
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.UserView)]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await userService.GetByIdAsync(id));
        }

        [HttpPut]
        [RequirePermission(Permissions.UserUpdate)]
        public async Task<IActionResult> Update(UserUpdateDto model)
        {
            return Ok(await userService.UpdateAsync(model));
        }

        [HttpPost("Filter")]
        [RequirePermission(Permissions.UserList)]
        public async Task<IActionResult> Filter([FromBody] FilterDto model)
        {
            return Ok(await userService.FilterAsync(model));
        }

        [HttpPost("RoleId")]
        [RequirePermission(Permissions.UserList)]
        public async Task<IActionResult> GetByRole([FromBody] UserByRoleRequestDto model)
        {
            return Ok(await userService.GetByRoleAsync(model));
        }

        [HttpPut("ToggleActive")]
        [RequirePermission(Permissions.UserToggle)]
        public async Task<IActionResult> ToggleActive([FromBody] UserToggleDto model)
        {
            return Ok(await userService.ToggleActiveAsync(model));
        }

        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] LoginDto model)
        {
            return Ok(await userService.LoginAsync(model));
        }

        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public IActionResult RefreshToken(TokenDto model)
        {
            return Ok(userService.RefreshToken(model));
        }

        [AllowAnonymous]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            return Ok(await userService.ChangePasswordAsync(model));
        }

        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordDto model)
        {
            return Ok(await userService.ForgotPasswordAsync(model));
        }

        [AllowAnonymous]
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            return Ok(await userService.ResetPasswordAsync(model));
        }
    }
}
