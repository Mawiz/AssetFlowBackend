using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Location;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationTypeController : BaseAppController
    {
        private readonly ILocationTypeService _service;

        public LocationTypeController(ILocationTypeService service)
        {
            _service = service;
        }

        [HttpPost]
        [RequirePermission(Permissions.LocationTypeCreate)]
        public async Task<IActionResult> Create(CreateLocationTypeDto dto)
        {
            var response = await _service.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [RequirePermission(Permissions.LocationTypeUpdate)]
        public async Task<IActionResult> Update(UpdateLocationTypeDto dto)
        {
            var response = await _service.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.LocationTypeView)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [RequirePermission(Permissions.LocationTypeView)]
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.GetAllAsync();
            return Ok(response);
        }

        [HttpPost("Filter")]
        [RequirePermission(Permissions.LocationTypeView)]
        public async Task<IActionResult> Filter([FromBody] SearchViewDto model)
        {
            var response = await _service.FilterAsync(model);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.LocationTypeDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
