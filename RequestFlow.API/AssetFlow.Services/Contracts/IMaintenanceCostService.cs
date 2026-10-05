using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.History;

namespace AssetFlow.Services.Contracts
{
    public interface IMaintenanceCostService
    {
        Task<ResponseDto<WorkOrderCostSummaryDto>> GetWorkOrderCostSummaryAsync(int workOrderId);
        Task<ResponseDto<List<AssetCostHistoryDto>>> GetByWorkOrderAsync(int workOrderId);
        Task<ResponseDto<AssetCostHistoryDto>> UpsertCostAsync(MaintenanceCostUpsertDto dto);
        Task<ResponseDto<bool>> DeleteCostAsync(int id);
        Task<ResponseDto<WorkOrderLaborUpsertDto>> UpsertLaborAsync(WorkOrderLaborUpsertDto dto);
        Task<ResponseDto<bool>> DeleteLaborAsync(int id);
        Task<ResponseDto<List<WorkOrderLaborUpsertDto>>> GetLaborByWorkOrderAsync(int workOrderId);
        Task<WorkOrderCostSummaryDto> BuildWorkOrderCostSummaryAsync(int workOrderId);
        Task<List<AssetCostHistoryDto>> QueryCostHistoryAsync(int assetId, int? workOrderId, int? costId, DateTime? start, DateTime? end);
    }
}
