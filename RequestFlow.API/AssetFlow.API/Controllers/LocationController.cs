using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Location;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : BaseAppController
    {
        private readonly ILocationService _service;

        public LocationController(ILocationService service)
        {
            _service = service;
        }

        [HttpPost]
        [RequirePermission(Permissions.LocationCreate)]
        public async Task<IActionResult> Create(CreateLocationDto dto)
        {
            var response = await _service.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.LocationUpdate)]
        public async Task<IActionResult> Update(UpdateLocationDto dto)
        {
            var response = await _service.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.LocationView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [RequirePermission(Permissions.LocationView)]
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.GetAllAsync();
            return Ok(response);
        }

        [HttpPost("Filter")]
        [RequirePermission(Permissions.LocationView)]
        public async Task<IActionResult> Filter([FromBody] LocationFilterDto model)
        {
            var response = await _service.FilterAsync(model);
            return Ok(response);
        }

        [HttpGet("by-location-type/{locationTypeId}")]
        [RequirePermission(Permissions.LocationView)]
        public async Task<IActionResult> GetByLocationTypeId(int locationTypeId, [FromQuery] int? tenantId)
        {
            var response = await _service.GetByLocationTypeIdAsync(locationTypeId, tenantId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.LocationDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
