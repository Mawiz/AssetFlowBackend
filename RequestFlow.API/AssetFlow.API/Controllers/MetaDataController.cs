using Microsoft.AspNetCore.Mvc;
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
        public IActionResult GetMetaDataValues([FromBody] MetaDataRequestDto model)
        {
            return Ok(metaDataService.GetMetaDataValues(model));
        }

        [HttpGet("GetMetaDataEnums")]
        public IActionResult Get()
        {
            return Ok(metaDataService.GetAllEnums());
        }

        [HttpGet("MetaDataKeys")]
        public IActionResult MetaDataKeys()
        {
            return Ok(metaDataService.MetaDataKeys());
        }
    }
}
