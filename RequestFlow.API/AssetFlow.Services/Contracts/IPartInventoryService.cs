using AssetFlow.Services.Dto;

using AssetFlow.Services.Dto.SparePart;



namespace AssetFlow.Services.Contracts

{

    public interface IPartInventoryService

    {

        Task<ResponseDto<PartInventoryDetailDto>> GetByIdAsync(int id);

        Task<ResponseDto<PartInventoryBatchDetailDto>> GetBatchByIdAsync(int batchId);

        Task<ResponseDto<List<PartInventoryDto>>> FilterAsync(PartInventoryFilterDto model);

        Task<ResponseDto<PartInventoryDto>> ReceiptAsync(PartReceiptDto dto);

        Task<ResponseDto<PartBatchReceiptResultDto>> BatchReceiptAsync(PartBatchReceiptDto dto);

        Task<ResponseDto<PartBatchReceiptResultDto>> ReceiveStockAsync(PartReceiveStockDto dto);

        Task<ResponseDto<PartInventoryDto>> TransferAsync(PartTransferDto dto);

        Task<ResponseDto<PartInventoryDto>> AdjustAsync(PartAdjustmentDto dto);

        Task<ResponseDto<PartInventoryDto>> IssueAsync(PartIssueDto dto);

        Task<ResponseDto<PartInventoryDto>> ReturnAsync(PartReturnDto dto);

        Task<ResponseDto<PartInventoryDto>> MarkFaultyAsync(PartInventoryStateChangeDto dto);

        Task<ResponseDto<PartInventoryDto>> QuarantineAsync(PartInventoryStateChangeDto dto);

        Task<ResponseDto<PartInventoryDto>> ReleaseFromQuarantineAsync(PartInventoryStateChangeDto dto);

        Task<ResponseDto<PartInventoryDto>> ReturnToSupplierAsync(PartReturnToSupplierDto dto);

        Task<ResponseDto<PartInventoryDto>> ScrapAsync(PartInventoryStateChangeDto dto);

        Task<ResponseDto<bool>> DeleteAsync(int id);

    }

}


