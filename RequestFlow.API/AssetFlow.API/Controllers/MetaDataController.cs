using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.MetaData;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaDataController : BaseAppController     
    {
        private readonly IMetaDataService metaDataService;

        public MetaDataController(IMetaDataService metaDataService)
        {
            this.metaDataService = metaDataService;
        }

        [HttpPost("GetMetaDataValues")]
        [RequirePermission(Permissions.MetaDataView)]
        public IActionResult GetMetaDataValues([FromBody] MetaDataRequestDto model)
        {
            return Ok(metaDataService.GetMetaDataValues(model));
        }

        [HttpGet("GetMetaDataEnums")]
        [RequirePermission(Permissions.MetaDataView)]
        public IActionResult Get()
        {
            return Ok(metaDataService.GetAllEnums());
        }

        [HttpGet("MetaDataKeys")]
        [RequirePermission(Permissions.MetaDataView)]
        public IActionResult MetaDataKeys()
        {
            return Ok(metaDataService.MetaDataKeys());
        }
    }
}
