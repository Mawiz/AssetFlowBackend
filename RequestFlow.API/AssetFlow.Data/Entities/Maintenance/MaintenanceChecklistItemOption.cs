using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.Maintenance
{
    public class MaintenanceChecklistItemOption : BaseModel
    {
        public int MaintenanceChecklistItemId { get; set; }
        public virtual MaintenanceChecklistItem MaintenanceChecklistItem { get; set; }

        [StringLength(200)]
        public string OptionText { get; set; }

        public int SortOrder { get; set; }
    }
}
