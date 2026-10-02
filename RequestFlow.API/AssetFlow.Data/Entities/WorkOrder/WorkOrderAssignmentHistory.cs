using AssetFlow.Data.Identity;
using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.WorkOrder
{
    public class WorkOrderAssignmentHistory : BaseModel
    {
        public int WorkOrderId { get; set; }
        public virtual WorkOrder WorkOrder { get; set; }

        public int AssignedToUserId { get; set; }
        public virtual ApplicationUser AssignedToUser { get; set; }

        public int AssignedByUserId { get; set; }
        public virtual ApplicationUser AssignedByUser { get; set; }

        public DateTime AssignedAt { get; set; }

        public DateTime? UnassignedAt { get; set; }

        [StringLength(2000)]
        public string Remarks { get; set; } = string.Empty;
    }
}
