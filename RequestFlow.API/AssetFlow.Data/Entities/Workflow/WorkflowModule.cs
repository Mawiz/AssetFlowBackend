using AssetFlow.Data.Entities.Tenant;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetFlow.Data.Entities.Workflow
{
    [Table("Modules", Schema = "Workflow")]
    public class WorkflowModule : WorkflowAuditedEntity, ITenancyModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ModuleId { get; set; }

        [Required, StringLength(150)]
        public string ModuleNameEn { get; set; } = string.Empty;

        [StringLength(150)]
        public string? ModuleNameAr { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsReOpen { get; set; }

        public int? TenantId { get; set; }
        public virtual Tenant.Tenant? Tenant { get; set; }

        public virtual ICollection<WorkflowModuleStage> Stages { get; set; } = new List<WorkflowModuleStage>();
    }
}
