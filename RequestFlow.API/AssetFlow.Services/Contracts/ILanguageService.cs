using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Tenant;

namespace AssetFlow.Services.Contracts
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
