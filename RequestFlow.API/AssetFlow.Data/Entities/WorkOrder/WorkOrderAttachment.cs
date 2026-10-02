using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.WorkOrder
{
    public class WorkOrderAttachment : BaseModel
    {
        public int WorkOrderId { get; set; }
        public virtual WorkOrder WorkOrder { get; set; }

        [StringLength(260)]
        public string FileName { get; set; } = string.Empty;

        [StringLength(120)]
        public string ContentType { get; set; } = string.Empty;

        [StringLength(500)]
        public string StoragePath { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }
    }
}
