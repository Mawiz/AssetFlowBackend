namespace AssetFlow.Data.Entities.Workflow
{
    public abstract class WorkflowAuditedEntity
    {
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public int? CreatedById { get; set; }
        public DateTime? ModifiedOn { get; set; } = DateTime.UtcNow;
        public int? ModifiedById { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
    }
}
