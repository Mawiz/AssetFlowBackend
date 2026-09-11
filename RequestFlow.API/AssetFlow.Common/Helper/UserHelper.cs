using Microsoft.AspNetCore.Http;

namespace AssetFlow.Common.Helper
{
    public static class UserHelper
    {
        public static int? GetCurrentUserId(IHttpContextAccessor httpContextAccessor)
        {
            return Convert.ToInt32(httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == "userId")?.Value);
        }

        public static int GetCurrentRoleId(IHttpContextAccessor httpContextAccessor)
        {
            return Convert.ToInt32(httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == "roleId")?.Value);
        }
    }
}
