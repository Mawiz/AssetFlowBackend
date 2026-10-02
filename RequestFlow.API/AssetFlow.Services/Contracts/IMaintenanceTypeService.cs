using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Maintenance;

namespace AssetFlow.Services.Contracts
{
    public interface IMaintenanceTypeService
    {
        Task<ResponseDto<MaintenanceTypeDto>> CreateAsync(CreateMaintenanceTypeDto dto);
        Task<ResponseDto<MaintenanceTypeDto>> UpdateAsync(UpdateMaintenanceTypeDto dto);
        Task<ResponseDto<MaintenanceTypeDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<MaintenanceTypeDto>>> GetAllAsync(int? tenantId);
        Task<ResponseDto<object>> FilterAsync(MaintenanceTypeFilterDto model);
        Task<ResponseDto<bool>> DeleteAsync(int id);
    }
}
