using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetFlow.Data.Entities.Workflow
{
    [Table("StagesActions", Schema = "Workflow")]
    public class WorkflowStageAction : WorkflowAuditedEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StageActionId { get; set; }

        public int StageId { get; set; }
        public virtual WorkflowModuleStage Stage { get; set; } = null!;

        public int ActionId { get; set; }
        public virtual WorkflowActionDefinition Action { get; set; } = null!;

        [Required, StringLength(200)]
        public string StageActionDisplayName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? StageActionDisplayNameAr { get; set; }

        public int EmailTemplateId { get; set; }
        public int DisplayOrderNo { get; set; }

        [StringLength(150)]
        public string? CssClass { get; set; }

        public int? NextSageId { get; set; }

        public int? AssigneeTypeId { get; set; }
        public int? AssignmentValue { get; set; }
        public int? SelectionTypeId { get; set; }
        public int? AcknowledgementTemplateId { get; set; }

        [StringLength(2000)]
        public string? ActionPopUpRenderingComponents { get; set; }

        public Guid? TemplateId { get; set; }
        public Guid? SMSTemplateId { get; set; }
    }
}
