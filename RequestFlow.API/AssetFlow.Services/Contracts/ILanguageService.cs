using RequestFlow.Services.Dto;
using RequestFlow.Services.Dto.Tenant;

namespace RequestFlow.Services.Contracts
{
    public interface ILanguageService
    {
        Task<ResponseDto<LanguageDto>> CreateAsync(CreateLanguageDto dto);
        Task<ResponseDto<LanguageDto>> UpdateAsync(UpdateLanguageDto dto);
        Task<ResponseDto<LanguageDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<LanguageDto>>> GetAllAsync();
        Task<ResponseDto<bool>> ToggleStatusAsync(int id);
    }
}
