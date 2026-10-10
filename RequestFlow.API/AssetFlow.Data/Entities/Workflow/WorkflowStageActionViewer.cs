using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetFlow.Data.Entities.Workflow
{
    [Table("StageActionViewer", Schema = "Workflow")]
    public class WorkflowStageActionViewer : WorkflowAuditedEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StageActionViewerId { get; set; }

        public int StageId { get; set; }
        public virtual WorkflowModuleStage Stage { get; set; } = null!;

        public int ViewerTypeId { get; set; }
        public int ViewerValue { get; set; }
    }
}
