using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AssetFlow.Services.Dto;

namespace AssetFlow.API.Filter
{
    public class ValidateModelStateFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (!actionContext.ModelState.IsValid)
            {
                var response = new ResponseDto<List<string>>();
                var list = new List<string>();

                foreach (var k in actionContext.ModelState.Keys)
                {
                    foreach (var e in actionContext.ModelState[k].Errors)
                    {
                        list.Add(string.IsNullOrEmpty(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage);
                    }
                }

                response.AddError(list);
                response.Exception = "One or more validation failed.";

                actionContext.Result = new JsonResult(response);
            }
        }
    }
}
