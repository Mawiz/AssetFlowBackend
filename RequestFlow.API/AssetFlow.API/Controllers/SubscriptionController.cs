using Microsoft.AspNetCore.Mvc;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto.Tenant;

namespace AssetFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : BaseAppController
    {
        private readonly ISubscriptionService _service;

        public SubscriptionController(ISubscriptionService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSubscriptionTypeDto dto)
        {
            var response = await _service.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateSubscriptionTypeDto dto)
        {
            var response = await _service.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.GetAllAsync();
            return Ok(response);
        }

        [HttpPost("Filter")]
        public async Task<IActionResult> Filter([FromBody] AssetFlow.Services.Dto.SearchViewDto model)
       {
            var response = await _service.FilterAsync(model);
            return Ok(response);
        }

        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var response = await _service.ToggleStatusAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

    }
}
