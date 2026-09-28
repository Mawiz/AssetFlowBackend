using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Asset;

namespace AssetFlow.Services.Contracts
{
    public interface IAssetCategoryService
    {
        Task<ResponseDto<AssetCategoryDto>> CreateAsync(CreateAssetCategoryDto dto);
        Task<ResponseDto<AssetCategoryDto>> UpdateAsync(UpdateAssetCategoryDto dto);
        Task<ResponseDto<AssetCategoryDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<AssetCategoryDto>>> GetAllAsync(int? tenantId = null);
        Task<ResponseDto<List<AssetCategoryDto>>> FilterAsync(SearchViewDto model);
        Task<ResponseDto<bool>> DeleteAsync(int id);
    }
}
