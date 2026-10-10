namespace AssetFlow.Services.Dto.Workflow
{
    public class WorkflowModuleItemMappingDto
    {
        public int ModuleItemMappingId { get; set; }
        public int ModuleId { get; set; }
        public string? ModuleNameEn { get; set; }
        public string ReferenceItemType { get; set; } = string.Empty;
        public int? TenantId { get; set; }
        public string? TenantName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateWorkflowModuleItemMappingDto
    {
        public int ModuleId { get; set; }
        public string ReferenceItemType { get; set; } = string.Empty;
        public int? TenantId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkflowModuleItemMappingDto : CreateWorkflowModuleItemMappingDto
    {
        public int ModuleItemMappingId { get; set; }
    }

    public class WorkflowModuleItemMappingFilterDto : SearchViewDto
    {
        public int? TenantId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class WorkflowMasterDto
    {
        public Guid WorkflowMasterId { get; set; }
        public int ModuleId { get; set; }
        public string? ModuleNameEn { get; set; }
        public int ReferenceItemId { get; set; }
        public string ReferenceItemType { get; set; } = string.Empty;
        public int SubmitBy { get; set; }
        public string? SubmitByDisplayName { get; set; }
        public int? CurrentStageId { get; set; }
        public string? CurrentStageName { get; set; }
        public bool IsCompleted { get; set; }
        public int? TenantId { get; set; }
        public Guid? ActiveWorkflowTaskId { get; set; }
        public string? PendingAssigneeDisplayName { get; set; }
        public string? LastOperationDisplay { get; set; }
    }

    public class WorkflowTaskDto
    {
        public Guid WorkflowTaskId { get; set; }
        public Guid WorkflowMasterId { get; set; }
        public int ModuleStageId { get; set; }
        public string? StageNameEn { get; set; }
        public int? StageActionId { get; set; }
        public int? ActionId { get; set; }
        public string? ActionDisplayName { get; set; }
        public int? AssigneeTypeId { get; set; }
        public int? AssignmentValue { get; set; }
        public string? AssigneeDisplayName { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? PerformedByUserId { get; set; }
        public string? PerformedByDisplayName { get; set; }
        public int? PerformedByRoleId { get; set; }
        public string? Notes { get; set; }
        public DateTime? DueDate { get; set; }
        public int? DueInHours { get; set; }
        public int ReferenceItemId { get; set; }
        public string ReferenceItemType { get; set; } = string.Empty;
        public string? ModuleNameEn { get; set; }
    }

    public class WorkflowTaskDetailDto : WorkflowTaskDto
    {
        public WorkflowMasterDto? Master { get; set; }
        public List<WorkflowStageActionOptionDto> AvailableActions { get; set; } = new();
        public List<WorkflowTaskDto> History { get; set; } = new();
    }

    public class WorkflowStageActionOptionDto
    {
        public int StageActionId { get; set; }
        public int ActionId { get; set; }
        public string StageActionDisplayName { get; set; } = string.Empty;
        public bool IsNotesRequired { get; set; }
        public int? NextSageId { get; set; }
    }

    public class PerformWorkflowActionDto
    {
        public Guid WorkflowTaskId { get; set; }
        public int StageActionId { get; set; }
        public string? Notes { get; set; }
    }

    public class WorkflowInboxFilterDto : SearchViewDto
    {
        public int Category { get; set; }
    }

    public class GetWorkflowByReferenceQuery
    {
        public string ReferenceItemType { get; set; } = string.Empty;
        public int ReferenceItemId { get; set; }
    }
}
