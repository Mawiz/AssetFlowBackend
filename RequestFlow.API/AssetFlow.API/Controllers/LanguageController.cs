using Microsoft.AspNetCore.Mvc;
using AssetFlow.API.Filter;
using AssetFlow.Common.Helper;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Tenant;

namespace AssetFlow.API.Controllers
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
        [RequirePermission(Permissions.LanguageCreate)]
        public async Task<IActionResult> Create(CreateLanguageDto dto)
        {
            var response = await _languageService.CreateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("update")]
        [RequirePermission(Permissions.LanguageUpdate)]
        public async Task<IActionResult> Update(UpdateLanguageDto dto)
        {
            var response = await _languageService.UpdateAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.LanguageView)]
        public async Task<IActionResult> Get(int id)
        {
            var response = await _languageService.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("list")]
        [RequirePermission(Permissions.LanguageList)]
        public async Task<IActionResult> GetAll()
        {
            var response = await _languageService.GetAllAsync();
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPatch("toggle-status/{id}")]
        [RequirePermission(Permissions.LanguageToggle)]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var response = await _languageService.ToggleStatusAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
