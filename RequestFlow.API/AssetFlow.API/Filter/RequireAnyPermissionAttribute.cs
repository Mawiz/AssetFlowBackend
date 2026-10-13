using AssetFlow.Data.Data;
using AssetFlow.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AssetFlow.API.Filter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireAnyPermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _permissions;

        public RequireAnyPermissionAttribute(params string[] permissions)
        {
            _permissions = permissions ?? Array.Empty<string>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (SkipAuthorization(context))
                return;

            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var tenantId = context.HttpContext.User.FindFirst("tenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId) || tenantId == "0")
                return;

            if (_permissions.Length == 0)
            {
                PermissionAuthorization.Forbid(context);
                return;
            }

            var userIdValue = context.HttpContext.User.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdValue, out var userId))
            {
                PermissionAuthorization.Forbid(context);
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

            var roleIds = db.ApplicationUserRoles
                .IgnoreQueryFilters()
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToList();

            var hasPermission = roleIds.Count > 0 && db.RoleResources
                .IgnoreQueryFilters()
                .Include(rr => rr.Resource)
                .Any(rr => roleIds.Contains(rr.ApplicationRoleId) && _permissions.Contains(rr.Resource.ResourceName));

            if (!hasPermission)
                PermissionAuthorization.Forbid(context);
        }

        private static bool SkipAuthorization(AuthorizationFilterContext context)
        {
            return (context.ActionDescriptor as ControllerActionDescriptor)?
                .MethodInfo
                .GetCustomAttributes(typeof(AllowAnonymousAttribute), true)
                .Any() == true;
        }
    }

    internal static class PermissionAuthorization
    {
        internal static void Forbid(AuthorizationFilterContext context)
        {
            context.Result = new ObjectResult(new ResponseDto<string>
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "Forbidden"
            })
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }
    }
}
