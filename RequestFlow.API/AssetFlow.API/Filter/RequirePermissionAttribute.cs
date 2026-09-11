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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
    {
        public string Permission { get; }

        public RequirePermissionAttribute(string permission)
        {
            Permission = permission;
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

            var userIdValue = context.HttpContext.User.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdValue, out var userId))
            {
                Forbid(context);
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
                .Any(rr => roleIds.Contains(rr.ApplicationRoleId) && rr.Resource.ResourceName == Permission);

            if (!hasPermission)
                Forbid(context);
        }

        private static void Forbid(AuthorizationFilterContext context)
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

        private static bool SkipAuthorization(AuthorizationFilterContext context)
        {
            return (context.ActionDescriptor as ControllerActionDescriptor)?
                .MethodInfo
                .GetCustomAttributes(typeof(AllowAnonymousAttribute), true)
                .Any() == true;
        }
    }
}
