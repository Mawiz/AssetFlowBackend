using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Maintenance;

namespace AssetFlow.Services.Contracts
{
    public interface IMaintenanceScheduleService
    {
        Task<ResponseDto<MaintenanceScheduleDto>> CreateAsync(SaveMaintenanceScheduleDto dto);
        Task<ResponseDto<MaintenanceScheduleDto>> UpdateAsync(UpdateMaintenanceScheduleDto dto);
        Task<ResponseDto<MaintenanceScheduleDto>> GetByIdAsync(int id);
        Task<ResponseDto<object>> FilterAsync(MaintenanceScheduleFilterDto model);
        Task<ResponseDto<bool>> SetActiveAsync(int id, bool isActive);
    }
}
