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

        public static List<int> GetCurrentRoleIds(IHttpContextAccessor httpContextAccessor)
        {
            return httpContextAccessor.HttpContext.User.Claims
                .Where(c => c.Type == "roleId")
                .Select(c => Convert.ToInt32(c.Value))
                .ToList();
        }
    }
}
