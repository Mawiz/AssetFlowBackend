using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.SparePart;

namespace AssetFlow.Services.Contracts
{
    public interface IPartInventoryService
    {
        Task<ResponseDto<List<PartInventoryDto>>> FilterAsync(PartInventoryFilterDto model);
        Task<ResponseDto<PartInventoryDto>> ReceiptAsync(PartReceiptDto dto);
        Task<ResponseDto<PartInventoryDto>> TransferAsync(PartTransferDto dto);
        Task<ResponseDto<PartInventoryDto>> AdjustAsync(PartAdjustmentDto dto);
        Task<ResponseDto<bool>> DeleteAsync(int id);
    }
}
