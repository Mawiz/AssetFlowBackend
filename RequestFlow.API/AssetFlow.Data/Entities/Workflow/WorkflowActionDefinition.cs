using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetFlow.Data.Entities.Workflow
{
    [Table("Actions", Schema = "Workflow")]
    public class WorkflowActionDefinition : WorkflowAuditedEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ActionId { get; set; }

        [Required, StringLength(150)]
        public string BeforeActionEn { get; set; } = string.Empty;

        [StringLength(150)]
        public string BeforeActionAr { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string AfterActionEn { get; set; } = string.Empty;

        [StringLength(150)]
        public string AfterActionAr { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string BaseCssClass { get; set; } = string.Empty;

        public bool IsNotesRequired { get; set; }

        public virtual ICollection<WorkflowStageAction> StageActions { get; set; } = new List<WorkflowStageAction>();
    }
}
