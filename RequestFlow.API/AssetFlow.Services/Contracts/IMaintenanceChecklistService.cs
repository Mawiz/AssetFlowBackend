using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Maintenance;

namespace AssetFlow.Services.Contracts
{
    public interface IMaintenanceChecklistService
    {
        Task<ResponseDto<MaintenanceChecklistDto>> CreateAsync(SaveMaintenanceChecklistDto dto);
        Task<ResponseDto<MaintenanceChecklistDto>> UpdateAsync(UpdateMaintenanceChecklistDto dto);
        Task<ResponseDto<MaintenanceChecklistDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<MaintenanceChecklistDto>>> GetAllAsync(int? tenantId, int? maintenanceTypeId);
        Task<ResponseDto<object>> FilterAsync(MaintenanceChecklistFilterDto model);
        Task<ResponseDto<bool>> DeleteAsync(int id);
    }
}
