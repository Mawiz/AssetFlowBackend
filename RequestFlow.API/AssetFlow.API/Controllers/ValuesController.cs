using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> Get()
        {
            return "API is running functionally....";
        }
    }
}
