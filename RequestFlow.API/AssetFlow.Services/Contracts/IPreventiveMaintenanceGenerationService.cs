using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Maintenance;

namespace AssetFlow.Services.Contracts
{
    public interface IPreventiveMaintenanceGenerationService
    {
        Task<ResponseDto<GeneratePreventiveMaintenanceResultDto>> GenerateAsync(GeneratePreventiveMaintenanceDto dto);
        Task CancelFutureOpenOccurrencesAsync(int scheduleId);
    }
}
