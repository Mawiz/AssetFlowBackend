using AssetFlow.Data.Entities.Tenant;

namespace AssetFlow.Data.Entities.Workflow
{
    public class WorkflowModuleItemMapping : WorkflowAuditedEntity, ITenancyModel
    {
        public int ModuleItemMappingId { get; set; }
        public int ModuleId { get; set; }
        public string ReferenceItemType { get; set; } = string.Empty;
        public int? TenantId { get; set; }

        public WorkflowModule? Module { get; set; }
    }
}
