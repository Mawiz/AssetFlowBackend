using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Asset;

namespace AssetFlow.Services.Contracts
{
    public interface IAssetService
    {
        Task<ResponseDto<AssetDto>> CreateAsync(CreateAssetDto dto);
        Task<ResponseDto<AssetDto>> UpdateAsync(UpdateAssetDto dto);
        Task<ResponseDto<AssetDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<AssetDto>>> GetAllAsync();
        Task<ResponseDto<List<AssetDto>>> FilterAsync(AssetFilterDto model);
        Task<ResponseDto<bool>> DeleteAsync(int id);
    }
}
