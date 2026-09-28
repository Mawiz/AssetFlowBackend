using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Asset;

namespace AssetFlow.Services.Contracts
{
    public interface IAssetTypeService
    {
        Task<ResponseDto<AssetTypeDto>> CreateAsync(CreateAssetTypeDto dto);
        Task<ResponseDto<AssetTypeDto>> UpdateAsync(UpdateAssetTypeDto dto);
        Task<ResponseDto<AssetTypeDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<AssetTypeDto>>> GetAllAsync();
        Task<ResponseDto<List<AssetTypeDto>>> FilterAsync(AssetTypeFilterDto model);
        Task<ResponseDto<bool>> DeleteAsync(int id);
    }
}
