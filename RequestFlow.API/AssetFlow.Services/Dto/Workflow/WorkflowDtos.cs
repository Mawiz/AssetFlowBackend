namespace AssetFlow.Services.Dto.Workflow
{
    public class WorkflowModuleDto
    {
        public int ModuleId { get; set; }
        public string ModuleNameEn { get; set; } = string.Empty;
        public string? ModuleNameAr { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsReOpen { get; set; }
        public int? TenantId { get; set; }
        public string? TenantName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateWorkflowModuleDto
    {
        public string ModuleNameEn { get; set; } = string.Empty;
        public string? ModuleNameAr { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsReOpen { get; set; }
        public int? TenantId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkflowModuleDto : CreateWorkflowModuleDto
    {
        public int ModuleId { get; set; }
    }

    public class WorkflowModuleFilterDto : SearchViewDto { }

    public class WorkflowModuleStageDto
    {
        public int StageId { get; set; }
        public int ModuleId { get; set; }
        public string StageNameEn { get; set; } = string.Empty;
        public string? StageNameAr { get; set; }
        public int SequenceNo { get; set; }
        public int DueInHours { get; set; }
        public int VersionNo { get; set; }
        public int EmailTemplateId { get; set; }
        public bool IsFinalApprover { get; set; }
        public int? AcknowledgementTemplateId { get; set; }
        public bool IsClosingStage { get; set; }
        public bool IsInitialStage { get; set; }
        public bool IsLogOpen { get; set; }
        public bool IsTATExtension { get; set; }
        public int? AssigneeTypeId { get; set; }
        public int? AssignmentValue { get; set; }
        public string? AssigneeDisplayName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateWorkflowModuleStageDto
    {
        public int ModuleId { get; set; }
        public string StageNameEn { get; set; } = string.Empty;
        public string? StageNameAr { get; set; }
        public int SequenceNo { get; set; }
        public int DueInHours { get; set; }
        public int VersionNo { get; set; }
        public int EmailTemplateId { get; set; }
        public bool IsFinalApprover { get; set; }
        public int? AcknowledgementTemplateId { get; set; }
        public bool IsClosingStage { get; set; }
        public bool IsInitialStage { get; set; }
        public bool IsLogOpen { get; set; }
        public bool IsTATExtension { get; set; }
        public int? AssigneeTypeId { get; set; }
        public int? AssignmentValue { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkflowModuleStageDto : CreateWorkflowModuleStageDto
    {
        public int StageId { get; set; }
    }

    public class WorkflowActionDto
    {
        public int ActionId { get; set; }
        public string BeforeActionEn { get; set; } = string.Empty;
        public string BeforeActionAr { get; set; } = string.Empty;
        public string AfterActionEn { get; set; } = string.Empty;
        public string AfterActionAr { get; set; } = string.Empty;
        public string BaseCssClass { get; set; } = string.Empty;
        public bool IsNotesRequired { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateWorkflowActionDto
    {
        public string BeforeActionEn { get; set; } = string.Empty;
        public string BeforeActionAr { get; set; } = string.Empty;
        public string AfterActionEn { get; set; } = string.Empty;
        public string AfterActionAr { get; set; } = string.Empty;
        public string BaseCssClass { get; set; } = string.Empty;
        public bool IsNotesRequired { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkflowActionDto : CreateWorkflowActionDto
    {
        public int ActionId { get; set; }
    }

    public class WorkflowActionFilterDto : SearchViewDto { }

    public class WorkflowStageActionDto
    {
        public int StageActionId { get; set; }
        public int StageId { get; set; }
        public int ActionId { get; set; }
        public string? ActionLabel { get; set; }
        public string StageActionDisplayName { get; set; } = string.Empty;
        public string? StageActionDisplayNameAr { get; set; }
        public int EmailTemplateId { get; set; }
        public int DisplayOrderNo { get; set; }
        public string? CssClass { get; set; }
        public int? NextSageId { get; set; }
        public string? NextStageName { get; set; }
        public int? AssigneeTypeId { get; set; }
        public int? AssignmentValue { get; set; }
        public string? AssigneeDisplayName { get; set; }
        public int? SelectionTypeId { get; set; }
        public int? AcknowledgementTemplateId { get; set; }
        public string? ActionPopUpRenderingComponents { get; set; }
        public Guid? TemplateId { get; set; }
        public Guid? SMSTemplateId { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateWorkflowStageActionDto
    {
        public int StageId { get; set; }
        public int ActionId { get; set; }
        public string StageActionDisplayName { get; set; } = string.Empty;
        public string? StageActionDisplayNameAr { get; set; }
        public int EmailTemplateId { get; set; }
        public int DisplayOrderNo { get; set; }
        public string? CssClass { get; set; }
        public int? NextSageId { get; set; }
        public int? AssigneeTypeId { get; set; }
        public int? AssignmentValue { get; set; }
        public int? SelectionTypeId { get; set; }
        public int? AcknowledgementTemplateId { get; set; }
        public string? ActionPopUpRenderingComponents { get; set; }
        public Guid? TemplateId { get; set; }
        public Guid? SMSTemplateId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkflowStageActionDto : CreateWorkflowStageActionDto
    {
        public int StageActionId { get; set; }
    }

    public class WorkflowStageActionViewerDto
    {
        public int StageActionViewerId { get; set; }
        public int StageId { get; set; }
        public int ViewerTypeId { get; set; }
        public int ViewerValue { get; set; }
        public string? ViewerDisplayName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateWorkflowStageActionViewerDto
    {
        public int StageId { get; set; }
        public int ViewerTypeId { get; set; }
        public int ViewerValue { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkflowStageActionViewerDto : CreateWorkflowStageActionViewerDto
    {
        public int StageActionViewerId { get; set; }
    }
}
