using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics.Contracts;

namespace AssetFlow.API.Filter
{
    /// <summary>
    /// Legacy attribute; use <see cref="RequirePermissionAttribute"/> instead.
    /// </summary>
    public class ResourceAuthorize : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (SkipAuthorization(context))
                return;
        }

        private static bool SkipAuthorization(AuthorizationFilterContext context)
        {
            Contract.Assert(context != null);

            return (context.ActionDescriptor as ControllerActionDescriptor)
                .MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true)
                .Any();
        }
    }
}

