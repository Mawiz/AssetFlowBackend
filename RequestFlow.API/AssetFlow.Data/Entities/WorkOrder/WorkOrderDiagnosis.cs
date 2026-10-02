using AssetFlow.Data.Identity;
using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.WorkOrder
{
    public class WorkOrderDiagnosis : BaseModel
    {
        public int WorkOrderId { get; set; }
        public virtual WorkOrder WorkOrder { get; set; }

        [StringLength(2000)]
        public string InitialProblem { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Diagnosis { get; set; } = string.Empty;

        [StringLength(2000)]
        public string RootCause { get; set; } = string.Empty;

        [StringLength(2000)]
        public string ActionTaken { get; set; } = string.Empty;

        [StringLength(2000)]
        public string FinalResult { get; set; } = string.Empty;

        public int DiagnosedByUserId { get; set; }
        public virtual ApplicationUser DiagnosedByUser { get; set; }

        public DateTime DiagnosedAt { get; set; }

        [StringLength(2000)]
        public string Remarks { get; set; } = string.Empty;
    }
}
