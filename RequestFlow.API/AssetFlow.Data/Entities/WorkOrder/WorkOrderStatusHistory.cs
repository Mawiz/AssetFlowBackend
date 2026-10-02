using AssetFlow.Data.Identity;
using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.WorkOrder
{
    public class WorkOrderStatusHistory : BaseModel
    {
        public int WorkOrderId { get; set; }
        public virtual WorkOrder WorkOrder { get; set; }

        public int FromStatus { get; set; }

        public int ToStatus { get; set; }

        public int ChangedByUserId { get; set; }
        public virtual ApplicationUser ChangedByUser { get; set; }

        public DateTime ChangedAt { get; set; }

        [StringLength(2000)]
        public string Remarks { get; set; } = string.Empty;
    }
}
