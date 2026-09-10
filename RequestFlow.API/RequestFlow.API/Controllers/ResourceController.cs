using Microsoft.AspNetCore.Mvc;
using RequestFlow.Services.Contracts;
using RequestFlow.Services.Dto.Role.RoleResource;

namespace RequestFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourceController : BaseAppController
    {
        private readonly IResourceService _service;
        public ResourceController(IResourceService service)
        {
            _service = service;
        }

        // 🔹 Create Resource
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateResourceDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return StatusCode((int)result.StatusCode, result);
        }

        // 🔹 Update Resource
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateResourceDto dto)
        {
            var result = await _service.UpdateAsync(dto);
            return StatusCode((int)result.StatusCode, result);
        }

        // 🔹 Get All Resources
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("Filter")]
        public async Task<IActionResult> Filter([FromBody] RequestFlow.Services.Dto.SearchViewDto model)
        {
            var result = await _service.FilterAsync(model);
            return Ok(result);
        }
    }
}
