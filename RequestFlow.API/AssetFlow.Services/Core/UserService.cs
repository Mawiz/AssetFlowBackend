using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AssetFlow.Common;
using AssetFlow.Common.Helper;
using AssetFlow.Common.Settings;
using AssetFlow.Data.Data;
using AssetFlow.Data.Identity;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.User;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Security.Claims;
using System.Text;
using X.PagedList;
using Z.EntityFramework.Plus;
using static AssetFlow.Common.Enum.Enums;
using static AssetFlow.Common.Helper.Constants;

namespace AssetFlow.Services.Core
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly EmailSettings emailSettings;
        private readonly JWTSettings jwtSettings;
        private readonly ApplicationDbContext appDbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly SystemSettings systemSettings;
        public UserService(ApplicationDbContext appDbContext, UserManager<ApplicationUser> userManager,
            IOptions<JWTSettings> jwtSettings, SignInManager<ApplicationUser> signInManager, 
            IOptions<EmailSettings> emailSettings, IHttpContextAccessor httpContextAccessor,
            IOptions<SystemSettings> systemSettings)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.emailSettings = emailSettings.Value;
            this.jwtSettings = jwtSettings.Value;
            this.appDbContext = appDbContext;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.systemSettings = systemSettings.Value;
        }

        #region PUBLIC

        public async Task<ResponseDto<string>> CreateAsync(UserCreateDto model)
        {
            var response = new ResponseDto<string>();

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.UserName,
                Email = model.Email,
                FirstLetter = model.FirstLetter,
                IsActive = true,
                IsReset = true,
                OtpCode = string.Empty,
                TenantId = model.TenantId
            };

            foreach (var roleId in (model.RoleIds ?? new List<int>()).Distinct())
            {
                user.UserRoles.Add(new ApplicationUserRole { RoleId = roleId });
            }

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return response;
            }

            response.AddError(GetErrorResult(result));

            return response;
        }        
        public async Task<ResponseDto<UserViewDto>> GetByIdAsync(int userId)
        {
            var response = new ResponseDto<UserViewDto>();

            var user = await appDbContext.ApplicationUsers
                             .Where(u => u.Id == userId)
                             .Select(u => new UserViewDto
                             {
                                 Id = u.Id,
                                 FullName = u.FullName,
                                 UserName = u.UserName,
                                 Email = u.Email,
                                 RoleIds = u.UserRoles.Select(r => r.RoleId).ToList(),
                                 RoleNames = u.UserRoles.Select(r => r.Role.Name).ToList(),
                                 RoleName = string.Join(", ", u.UserRoles.Select(r => r.Role.Name)),
                                 IsActive = u.IsActive
                             })
                             .FirstOrDefaultAsync();

            if (user == null)
            {
                response.AddError(string.Format(AppResource.NotExist, "User"));
                return response;
            }

            response.Result = user;
            return response;
        }
        public async Task<ResponseDto<string>> UpdateAsync(UserUpdateDto model)
        {
            var response = new ResponseDto<string>();

            var user = await appDbContext.ApplicationUsers
                             .Where(x => x.Id == model.Id)
                             .IncludeOptimized(x => x.UserRoles)
                             .SingleOrDefaultAsync();

            if (user == null)
            {
                response.AddError(string.Format(AppResource.NotExist, "User"));
                return response;
            }

            var isEmailExist = appDbContext.ApplicationUsers.Any(u => u.Id != model.Id && u.Email == model.Email);
            
            if (isEmailExist)
            {
                response.AddError(string.Format(AppResource.AlreadyExist, "Email"));
                return response;
            }

            user.FullName = model.FullName;
            user.FirstLetter = model.FullName.Trim().ToUpper()[0];
            user.Email = model.Email;
            user.NormalizedEmail = model.Email.ToUpper();
            user.ModifiedOn = model.ModifiedOn;

            if (!string.IsNullOrEmpty(model.Password))
            {
               user.PasswordHash = userManager.PasswordHasher.HashPassword(user, model.Password);
               user.IsReset = true;
            }

            user.IsActive = model.IsActive;
            if (model.TenantId.HasValue)
                user.TenantId = model.TenantId;

            user.UserRoles.Clear();
            foreach (var roleId in (model.RoleIds ?? new List<int>()).Distinct())
            {
                user.UserRoles.Add(new ApplicationUserRole { RoleId = roleId, UserId = user.Id });
            }
            
            await appDbContext.SaveChangesAsync();

            return response;
        }      
        public async Task<ResponseDto<List<GetUserDto>>> FilterAsync(FilterDto model)
        {
            var response = new ResponseDto<List<GetUserDto>>();

            var queryable = appDbContext.ApplicationUsers
                            .Where(u =>
                                    (string.IsNullOrEmpty(model.SearchText) || u.UserName.Contains(model.SearchText) ||
                                     u.Email.Contains(model.SearchText) || u.FullName.Contains(model.SearchText)) &&
                                    (!model.Roles.Any() || u.UserRoles.Any(x => model.Roles.Contains(x.Role.Id))) &&
                                    (!model.IsActive.HasValue || u.IsActive == model.IsActive) &&
                                    (!model.StartDate.HasValue || u.ModifiedOn > model.StartDate.Value))
                                    .Select(user => new GetUserDto()
                                    {
                                        Id = user.Id,
                                        FirstLetter = user.FirstLetter,
                                        FullName = user.FullName,
                                        UserName = user.UserName,
                                        Email = user.Email,
                                        RoleIds = user.UserRoles.Select(r => r.RoleId).ToList(),
                                        RoleNames = user.UserRoles.Select(r => r.Role.Name).ToList(),
                                        RoleName = string.Join(", ", user.UserRoles.Select(r => r.Role.Name)),
                                        IsActive = user.IsActive,
                                        ModifiedOn = user.ModifiedOn,
                                        TenantId = user.TenantId,
                                        TenantName = user.TenantId.HasValue ? user.Tenant.CompanyName : "SystemAdmin"
                                    });

            model.OrderByProp ??= nameof(ApplicationUser.Id);

            var query = model.SortDirection == (int)OrderBy.Ascending
                ?
                queryable.OrderBy(model.OrderByProp)
                :
                queryable.OrderBy($"{model.OrderByProp} descending");

            var paginated = await query.ToPagedList(pageNumber: model.PageNumber, pageSize: model.PageSize).ToListAsync();

            response.Result = paginated;
            return response;
        }
        public async Task<ResponseDto<UserToggleDto>> ToggleActiveAsync(UserToggleDto model)
        {
            var response = new ResponseDto<UserToggleDto>();

            var user = await appDbContext.ApplicationUsers
                             .SingleOrDefaultAsync(u => u.Id == model.Id);

            if (user == null)
            {
                response.AddError(string.Format(AppResource.NotExist, "User"));
                return response;
            }

            user.IsActive = model.IsActive;

            await appDbContext.SaveChangesAsync();

            return response;
        }
        /// <summary>
        /// Force Password Reset
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ResponseDto<LoginResponseDto>> ChangePasswordAsync(ChangePasswordDto model)
        {
            var response = new ResponseDto<LoginResponseDto>();

            var user = await appDbContext.ApplicationUsers
                             .SingleOrDefaultAsync(u => u.UserName == model.UserName);

            if (user == null)
            {
                response.AddError(string.Format(AppResource.NotExist, "User"));
                return response;
            }

            var decodedToken = WebUtility.UrlDecode(model.Token);

            var result = await userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (result.Succeeded)
            {
                user.IsReset = false;

                await appDbContext.SaveChangesAsync();

                var login = new LoginDto { UserName = model.UserName, Password = model.NewPassword };

                var loginResponse = LoginAsync(login).Result;

                response.Result = loginResponse.Result;

                return response;
            }

            response.AddError(string.Format(AppResource.InValid, "Token"));
            return response;
        }
        public async Task<ResponseDto<string>> ForgotPasswordAsync(ForgetPasswordDto model)
        {
            var response = new ResponseDto<string>();

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null || !user.IsActive)
            {
                response.Message = AppResource.ResetPasswordSuccessMsg;
                return response;
            }   

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var encodedBase64 = Base64UrlEncoder.Encode(token);

            var url = emailSettings.ResetPasswordURL.Replace("[userid]", user.Id.ToString()).Replace("[token]", encodedBase64);

            var deepLinkUrl = string.Format(emailSettings.ResetPasswordURLDeepLink, WebUtility.UrlEncode(url));

            var emailHelper = new EmailHelper(emailSettings);

            var content = new Dictionary<string, string>
            {
                ["{Name}"] = user.UserName,
                ["{Link}"] = deepLinkUrl,
                ["{CompanyLogo}"] = emailSettings.CompanyLogo,
                ["{Shadow}"] = emailSettings.Shadow,
                ["{SecondShadow}"] = emailSettings.SecondShadow,
                ["{BottomShadow}"] = emailSettings.BottomShadow
            };

            string body = HtmlGenerator.GetEmailBody(emailSettings.HtmlTemplatePath, content);
            
            emailHelper.SendEmail(emailSettings.EmailSubject, user.Email, body);

            response.Message = AppResource.ResetPasswordSuccessMsg;
            return response;
        }
        /// <summary>
        /// Forgot Password Reset
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ResponseDto<string>> ResetPasswordAsync(ResetPasswordDto model)
        {
            var response = new ResponseDto<string>();

            var user = await userManager.FindByIdAsync(model.Id);

            if (user == null || !user.IsActive)
            {
                response.AddError(string.Format(AppResource.NotExist, "User"));
                return response;
            }

            // ✅ Check OTP
            if (string.IsNullOrEmpty(user.OtpCode)
                || user.OtpExpiry < DateTime.UtcNow
                || user.OtpCode != model.Otp)
            {
                response.AddError("Invalid or expired OTP."); // Or use AppResource.InValidOtp if defined
                return response;
            }

            // ✅ Decode token
            var decodedBytes = Base64UrlEncoder.DecodeBytes(model.Token);
            var decodedString = Encoding.UTF8.GetString(decodedBytes);

            // ✅ Reset password
            var result = await userManager.ResetPasswordAsync(user, decodedString, model.NewPassword);

            if (!result.Succeeded)
            {
                response.AddError(AppResource.ResetPasswordErrorMessage);
                return response;
            }

            // ✅ Clear OTP and mark reset done
            user.OtpCode = string.Empty;
            user.OtpExpiry = null;
            user.IsReset = false;
            await userManager.UpdateAsync(user);

            response.Result = "Password reset successful";
            return response;
        }


        public async Task<ResponseDto<LoginResponseDto>> LoginAsync(LoginDto model)
        {
            var response = new ResponseDto<LoginResponseDto>();

            // 🔹 Find by username OR email
            var user = await appDbContext.ApplicationUsers.IgnoreQueryFilters()
                             .Include(ur => ur.UserRoles)
                                .ThenInclude(r => r.Role)
                             .Where(x => (x.UserName == model.UserName || x.Email == model.UserName)
                                         && x.IsActive)
                             .SingleOrDefaultAsync();

            if (user == null)
            {
                response.AddError(string.Format(AppResource.InValid, "username or password"));
                return response;
            }

            // 🔹 Always login using UserName (Identity requirement)
            var signInResult = await signInManager.PasswordSignInAsync(
                                    user.UserName,
                                    model.Password,
                                    true,
                                    false);

            if (!signInResult.Succeeded)
            {
                response.AddError(string.Format(AppResource.InValid, "username or password"));
                return response;
            }

            if (user.IsReset)
            {
                response.Result = new LoginResponseDto();

                // Generate password reset token
                var token = await userManager.GeneratePasswordResetTokenAsync(user);

                // Generate OTP
                var otp = OtpHelper.GenerateOtp();

                // Save OTP temporarily in DB (add a column in ApplicationUser if not exists)
                user.OtpCode = otp;
                user.OtpExpiry = DateTime.UtcNow.AddMinutes(10); // expire in 10 min
                await appDbContext.SaveChangesAsync();

                // Send OTP via email
                //await notificationHelper.SendEmailAsync(user.Email,
                //    "Password Reset OTP",
                //    $"Your OTP is: {otp}. It is valid for 10 minutes.");

                response.Result.Token = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(token));
                response.Result.IsReset = true;
                response.Result.User = MapLoginUser(user, null);

                return response;
            }


            var userDetail = MapLoginUser(user, systemSettings.PageSize);

            user.LastActive = DateTime.UtcNow;

            await appDbContext.SaveChangesAsync();

            var roleIds = GetRoleIds(user);

            var existingPermissions = await appDbContext.RoleResources
                                            .Where(u => roleIds.Contains(u.ApplicationRoleId))
                                            .Select(r => r.Resource.ResourceName)
                                            .Distinct()
                                            .ToListAsync();

            var menuItems = await appDbContext.MenuItemEnums
                                  .Where(m => roleIds.Contains(m.ApplicationRoleId))
                                  .Select(mi => mi.DisplayName)
                                  .Distinct()
                                  .ToListAsync();

            var navigationItems = await appDbContext.NavigationItemEnums
                                        .Where(n => roleIds.Contains(n.ApplicationRoleId))
                                        .Select(ni => ni.DisplayName)
                                        .Distinct()
                                        .ToListAsync();

            var navigationCreateItems = await appDbContext.NavigationCreateItemEnums
                                              .Where(n => roleIds.Contains(n.ApplicationRoleId))
                                              .Select(ni => ni.DisplayName)
                                              .Distinct()
                                              .ToListAsync();

            response.Result = new LoginResponseDto()
            {
                User = userDetail,
                Token = GenerateJSONWebToken(user),
                Permissions = existingPermissions,
                MenuItems = menuItems,
                NavigationItems = navigationItems,
                NavigationCreateItems = navigationCreateItems
            };

            return response;
        }

        public ResponseDto<string> RefreshToken(TokenDto model)
        {
            var response = new ResponseDto<string>();

            response = GetPrincipalFromExpiredToken(model.Token);

            return response;
        }
        public async Task<ResponseDto<List<UserByRoleDto>>> GetByRoleAsync(UserByRoleRequestDto model)
        {
            var response = new ResponseDto<List<UserByRoleDto>>();

            var queryable = await appDbContext.ApplicationUsers
                                  .Where(u =>
                                          (!model.Roles.Any() || u.UserRoles.Any(x => model.Roles.Contains(x.Role.Id))) &&
                                          (!model.IsActive.HasValue || u.IsActive == model.IsActive) &&
                                          (!model.LatestByDate.HasValue || u.ModifiedOn > model.LatestByDate.Value))
                                          .OrderBy(u => u.FullName)
                                          .Select(user => new UserByRoleDto()
                                          {
                                              Id = user.Id,
                                              DisplayName = user.FullName,
                                              RoleIds = user.UserRoles.Select(r => r.RoleId).ToList(),
                                              IsActive = user.IsActive,
                                              ModifiedOn = user.ModifiedOn
                                          })
                                          .ToListAsync();
                                    
            response.Result = queryable;

            return response;
        }
        #endregion PUBLIC

        #region PRIVATE

        /// <summary>
        /// Function to genereate JSON Web token for authentication.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Token</returns>
        private string GenerateJSONWebToken(ApplicationUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = BuildUserClaims(user);

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims,
                expires: DateTime.UtcNow.AddHours(jwtSettings.ExpireInHours).AddMinutes(jwtSettings.ExpireInMinutes),
                signingCredentials: credentials
                );

            var encodetoken = new JwtSecurityTokenHandler().WriteToken(token);
            return encodetoken;
        }

        /// <summary>
        /// Function to get error type result
        /// </summary>
        private List<string> GetErrorResult(IdentityResult result)
        {
            var res = new List<string>();

            if (result == null)
            {
                res.Add(AppResource.InternalServerError);
                return res;
            }

            foreach (var error in result.Errors)
            {
                res.Add(error.Description);
            }

            return res;
        }

        private ResponseDto<string> GetPrincipalFromExpiredToken(string token)
        {
            var response = new ResponseDto<string>();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidAudience = jwtSettings.Audience,
                ValidIssuer = jwtSettings.Issuer,
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                response.Message = AppResource.InValidToken;
                return response;
            }

            if (jwtSecurityToken.ValidTo.AddMinutes(Numbers.Five) > DateTime.UtcNow)
            {
                response.Message = AppResource.TokenNotExpired;
                return response;
            }

            response.Result = GetTokenObject(claims: principal.Claims);

            return response;
        }

        private string GetTokenObject(IEnumerable<Claim> claims = null, ApplicationUser user = null)
        {
            if (user != null)
            {
                claims = BuildUserClaims(user);
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

            int expireInHours = Convert.ToInt32(jwtSettings.ExpireInHours);

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                expires: DateTime.Now.AddHours(expireInHours),
                claims: claims,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );

            var encodetoken = new JwtSecurityTokenHandler().WriteToken(token);

            return encodetoken;

        }

        private static List<int> GetRoleIds(ApplicationUser user)
        {
            return user.UserRoles?.Select(r => r.RoleId).Distinct().ToList() ?? new List<int>();
        }

        private static LoginUserDto MapLoginUser(ApplicationUser user, int? pageSize)
        {
            return new LoginUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                RoleIds = GetRoleIds(user),
                PageSize = pageSize
            };
        }

        private static List<Claim> BuildUserClaims(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("userId", user.Id.ToString()),
                new Claim("tenantId", user.TenantId?.ToString() ?? "0"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var roleId in GetRoleIds(user))
            {
                claims.Add(new Claim("roleId", roleId.ToString()));
            }

            return claims;
        }

        #endregion PRIVATE
    }
}
