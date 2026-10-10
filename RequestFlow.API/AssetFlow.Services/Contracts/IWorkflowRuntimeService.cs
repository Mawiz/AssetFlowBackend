using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Workflow;

namespace AssetFlow.Services.Contracts
{
    public interface IWorkflowRuntimeService
    {
        /// <summary>
        /// Starts workflow when mapping exists. Returns Ok with no error when skipped (no mapping).
        /// Returns failure when mapping exists but configuration is invalid (caller should roll back item creation).
        /// </summary>
        Task<(bool Ok, string? Error)> InitiateIfMappedAsync(int? tenantId, string referenceItemType, int referenceItemId, int submitByUserId, CancellationToken cancellationToken = default);

        Task<ResponseDto<WorkflowMasterDto>> GetMasterByIdAsync(Guid workflowMasterId);
        Task<ResponseDto<WorkflowMasterDto>> GetMasterByReferenceAsync(string referenceItemType, int referenceItemId);
        Task<ResponseDto<WorkflowTaskDetailDto>> GetTaskDetailAsync(Guid workflowTaskId);
        Task<ResponseDto<List<WorkflowStageActionOptionDto>>> GetAvailableActionsAsync(Guid workflowTaskId);
        Task<ResponseDto<WorkflowMasterDto>> PerformActionAsync(PerformWorkflowActionDto dto);
        Task<ResponseDto<List<WorkflowTaskDto>>> GetHistoryAsync(Guid workflowMasterId);
        Task<ResponseDto<object>> FilterInboxAsync(WorkflowInboxFilterDto model);
    }
}
