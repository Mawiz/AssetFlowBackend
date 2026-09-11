using AssetFlow.API.Filter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    //[ResourceAuthorize]
    [TypeFilter(typeof(ValidateModelStateFilter))]
    public class BaseAppController : ControllerBase
    {
 
    }
}
