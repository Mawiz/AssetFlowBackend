using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using RequestFlow.Data.Data;
using RequestFlow.Services.Dto;
using System.Diagnostics.Contracts;
using System.Net;

namespace RequestFlow.API.Filter
{
    public class ResourceAuthorize : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (SkipAuthorization(context))
                return;

            var roleId = context.HttpContext.User.Identities.FirstOrDefault()?.FindFirst("roleId")?.Value;
            var uri = context.ActionDescriptor.AttributeRouteInfo.Template.ToLower();

            if (!context.HttpContext.User.Identity.IsAuthenticated ||
                !(context.HttpContext.RequestServices.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext)
                .RoleResources.Any(r => r.ApplicationRoleId == Convert.ToInt32(roleId) && r.Resource.IsBackEnd
                    && r.Resource.Verb.ToLower().EndsWith(uri)))
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                //TODO user some other class
                context.Result = new OkObjectResult(
                    new ResponseDto<string>
                    {
                        StatusCode = HttpStatusCode.Unauthorized,
                        Message = "Unauthorized"
                    });
              
                return;
            }
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

