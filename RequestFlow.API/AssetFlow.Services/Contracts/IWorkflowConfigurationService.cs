using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Workflow;

namespace AssetFlow.Services.Contracts
{
    public interface IWorkflowConfigurationService
    {
        Task<ResponseDto<WorkflowModuleDto>> CreateModuleAsync(CreateWorkflowModuleDto dto);
        Task<ResponseDto<WorkflowModuleDto>> UpdateModuleAsync(UpdateWorkflowModuleDto dto);
        Task<ResponseDto<WorkflowModuleDto>> GetModuleByIdAsync(int moduleId);
        Task<ResponseDto<List<WorkflowModuleDto>>> GetAllModulesAsync(int? tenantId);
        Task<ResponseDto<object>> FilterModulesAsync(WorkflowModuleFilterDto model);
        Task<ResponseDto<bool>> DeleteModuleAsync(int moduleId);

        Task<ResponseDto<WorkflowModuleStageDto>> CreateStageAsync(CreateWorkflowModuleStageDto dto);
        Task<ResponseDto<WorkflowModuleStageDto>> UpdateStageAsync(UpdateWorkflowModuleStageDto dto);
        Task<ResponseDto<WorkflowModuleStageDto>> GetStageByIdAsync(int stageId);
        Task<ResponseDto<List<WorkflowModuleStageDto>>> GetAllStagesAsync(int moduleId);
        Task<ResponseDto<bool>> DeleteStageAsync(int stageId);

        Task<ResponseDto<WorkflowActionDto>> CreateActionAsync(CreateWorkflowActionDto dto);
        Task<ResponseDto<WorkflowActionDto>> UpdateActionAsync(UpdateWorkflowActionDto dto);
        Task<ResponseDto<WorkflowActionDto>> GetActionByIdAsync(int actionId);
        Task<ResponseDto<List<WorkflowActionDto>>> GetAllActionsAsync();
        Task<ResponseDto<object>> FilterActionsAsync(WorkflowActionFilterDto model);
        Task<ResponseDto<bool>> DeleteActionAsync(int actionId);

        Task<ResponseDto<WorkflowStageActionDto>> CreateStageActionAsync(CreateWorkflowStageActionDto dto);
        Task<ResponseDto<WorkflowStageActionDto>> UpdateStageActionAsync(UpdateWorkflowStageActionDto dto);
        Task<ResponseDto<WorkflowStageActionDto>> GetStageActionByIdAsync(int stageActionId);
        Task<ResponseDto<List<WorkflowStageActionDto>>> GetAllStageActionsAsync(int stageId);
        Task<ResponseDto<bool>> DeleteStageActionAsync(int stageActionId);

        Task<ResponseDto<WorkflowStageActionViewerDto>> CreateStageActionViewerAsync(CreateWorkflowStageActionViewerDto dto);
        Task<ResponseDto<WorkflowStageActionViewerDto>> UpdateStageActionViewerAsync(UpdateWorkflowStageActionViewerDto dto);
        Task<ResponseDto<WorkflowStageActionViewerDto>> GetStageActionViewerByIdAsync(int stageActionViewerId);
        Task<ResponseDto<List<WorkflowStageActionViewerDto>>> GetAllStageActionViewersAsync(int stageId);
        Task<ResponseDto<bool>> DeleteStageActionViewerAsync(int stageActionViewerId);
    }
}
