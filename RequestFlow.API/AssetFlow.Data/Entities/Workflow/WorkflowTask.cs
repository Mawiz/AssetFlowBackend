namespace AssetFlow.Data.Entities.Workflow
{
    public class WorkflowTask : WorkflowAuditedEntity
    {
        public Guid WorkflowTaskId { get; set; }
        public Guid WorkflowMasterId { get; set; }
        public int ModuleStageId { get; set; }
        public int? StageActionId { get; set; }
        public int? ActionId { get; set; }
        public int? AssigneeTypeId { get; set; }
        public int? AssignmentValue { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? PerformedByUserId { get; set; }
        public int? PerformedByRoleId { get; set; }
        public string? Notes { get; set; }
        public DateTime? DueDate { get; set; }
        public int? DueInHours { get; set; }

        public WorkflowMaster? WorkflowMaster { get; set; }
        public WorkflowModuleStage? ModuleStage { get; set; }
    }
}
