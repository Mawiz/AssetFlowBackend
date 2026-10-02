using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Maintenance;

namespace AssetFlow.Services.Contracts
{
    public interface IPreventiveMaintenanceService
    {
        Task<ResponseDto<object>> FilterAsync(PreventiveMaintenanceOccurrenceFilterDto model);
        Task<ResponseDto<PreventiveMaintenanceOccurrenceDto>> GetByIdAsync(int id);
        Task<ResponseDto<PreventiveMaintenanceOccurrenceDto>> StartAsync(int id);
        Task<ResponseDto<PreventiveMaintenanceOccurrenceDto>> CompleteAsync(CompletePreventiveMaintenanceDto dto);
        Task<ResponseDto<PreventiveMaintenanceOccurrenceDto>> CancelAsync(int id);
        Task<ResponseDto<List<CalendarOccurrenceDto>>> GetCalendarAsync(PreventiveMaintenanceOccurrenceFilterDto model);
        Task<ResponseDto<AssetPreventiveMaintenanceSummaryDto>> GetAssetSummaryAsync(int assetId);
    }
}
