using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.History;

namespace AssetFlow.Services.Contracts
{
    public interface IAssetHistoryService
    {
        Task<ResponseDto<AssetHistorySummaryDto>> GetSummaryAsync(int assetId);
        Task<ResponseDto<AssetHistoryPagedDto>> GetTimelineAsync(AssetHistoryFilterDto filter);
        Task<ResponseDto<List<AssetMaintenanceHistoryDto>>> GetMaintenanceHistoryAsync(AssetHistoryFilterDto filter);
        Task<ResponseDto<List<AssetBreakdownHistoryDto>>> GetBreakdownHistoryAsync(AssetHistoryFilterDto filter);
        Task<ResponseDto<List<AssetWorkOrderHistoryDto>>> GetWorkOrderHistoryAsync(AssetHistoryFilterDto filter);
        Task<ResponseDto<List<AssetPartHistoryDto>>> GetPartsHistoryAsync(AssetHistoryFilterDto filter);
        Task<ResponseDto<List<AssetDowntimeHistoryDto>>> GetDowntimeHistoryAsync(AssetHistoryFilterDto filter);
        Task<ResponseDto<AssetCostSummaryDto>> GetCostSummaryAsync(int assetId, AssetHistoryFilterDto filter);
        Task<ResponseDto<List<AssetCostHistoryDto>>> GetCostHistoryAsync(AssetHistoryFilterDto filter);
    }
}
