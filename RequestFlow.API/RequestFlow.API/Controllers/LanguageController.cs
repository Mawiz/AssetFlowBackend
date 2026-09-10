using Microsoft.AspNetCore.Mvc;
using RequestFlow.Services.Contracts;
using RequestFlow.Services.Dto;
using RequestFlow.Services.Dto.Tenant;

namespace RequestFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguageController : BaseAppController
    {
        private readonly ILanguageService _languageService;

        public LanguageController(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateLanguageDto dto)
        {
            var response = await _languageService.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateLanguageDto dto)
        {
            var response = await _languageService.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var response = await _languageService.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAll()
        {
            var response = await _languageService.GetAllAsync();
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPatch("toggle-status/{id}")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var response = await _languageService.ToggleStatusAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
