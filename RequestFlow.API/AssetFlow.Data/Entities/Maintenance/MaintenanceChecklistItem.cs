using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.Maintenance
{
    public class MaintenanceChecklistItem : BaseModel
    {
        public int MaintenanceChecklistId { get; set; }
        public virtual MaintenanceChecklist MaintenanceChecklist { get; set; }

        [StringLength(500)]
        public string ItemText { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public int ResponseType { get; set; }

        public bool IsRequired { get; set; }

        public int SortOrder { get; set; }

        public virtual ICollection<MaintenanceChecklistItemOption> Options { get; set; } = new List<MaintenanceChecklistItemOption>();
    }
}
