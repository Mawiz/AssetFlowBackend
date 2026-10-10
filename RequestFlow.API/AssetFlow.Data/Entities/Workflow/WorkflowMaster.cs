using AssetFlow.Data.Entities.Tenant;

namespace AssetFlow.Data.Entities.Workflow
{
    public class WorkflowMaster : WorkflowAuditedEntity, ITenancyModel
    {
        public Guid WorkflowMasterId { get; set; }
        public int ModuleId { get; set; }
        public int ReferenceItemId { get; set; }
        public string ReferenceItemType { get; set; } = string.Empty;
        public int SubmitBy { get; set; }
        public int? CurrentStageId { get; set; }
        public bool IsCompleted { get; set; }
        public int? TenantId { get; set; }

        public WorkflowModule? Module { get; set; }
        public ICollection<WorkflowTask> Tasks { get; set; } = new List<WorkflowTask>();
    }
}
