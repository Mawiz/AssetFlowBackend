using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Asset;

namespace AssetFlow.Services.Contracts
{
    public interface IAssetComponentService
    {
        Task<ResponseDto<AssetComponentDto>> CreateAsync(CreateAssetComponentDto dto);
        Task<ResponseDto<AssetComponentDto>> UpdateAsync(UpdateAssetComponentDto dto);
        Task<ResponseDto<AssetComponentDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<AssetComponentDto>>> FilterAsync(AssetComponentFilterDto model);
        Task<ResponseDto<bool>> DeleteAsync(int id);
    }
}
