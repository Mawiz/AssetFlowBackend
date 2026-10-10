using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetFlow.Data.Entities.Workflow
{
    [Table("ModuleStages", Schema = "Workflow")]
    public class WorkflowModuleStage : WorkflowAuditedEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StageId { get; set; }

        [Required, StringLength(200)]
        public string StageNameEn { get; set; } = string.Empty;

        [StringLength(200)]
        public string? StageNameAr { get; set; }

        public int ModuleId { get; set; }
        public virtual WorkflowModule Module { get; set; } = null!;

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

        public virtual ICollection<WorkflowStageAction> StageActions { get; set; } = new List<WorkflowStageAction>();
        public virtual ICollection<WorkflowStageActionViewer> Viewers { get; set; } = new List<WorkflowStageActionViewer>();
    }
}
