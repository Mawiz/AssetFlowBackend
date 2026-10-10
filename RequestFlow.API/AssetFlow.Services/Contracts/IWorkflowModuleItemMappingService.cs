using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Workflow;

namespace AssetFlow.Services.Contracts
{
    public interface IWorkflowModuleItemMappingService
    {
        Task<ResponseDto<WorkflowModuleItemMappingDto>> CreateAsync(CreateWorkflowModuleItemMappingDto dto);
        Task<ResponseDto<WorkflowModuleItemMappingDto>> UpdateAsync(UpdateWorkflowModuleItemMappingDto dto);
        Task<ResponseDto<WorkflowModuleItemMappingDto>> GetByIdAsync(int moduleItemMappingId);
        Task<ResponseDto<List<WorkflowModuleItemMappingDto>>> GetAllAsync(int? tenantId);
        Task<ResponseDto<object>> FilterAsync(WorkflowModuleItemMappingFilterDto model);
        Task<ResponseDto<bool>> DeleteAsync(int moduleItemMappingId);
    }
}
