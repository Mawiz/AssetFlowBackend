using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.Maintenance
{
    public class PreventiveMaintenanceOccurrenceChecklistItem : BaseModel
    {
        public int PreventiveMaintenanceOccurrenceId { get; set; }
        public virtual PreventiveMaintenanceOccurrence Occurrence { get; set; }

        public int? SourceChecklistItemId { get; set; }

        [StringLength(500)]
        public string ItemText { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public int ResponseType { get; set; }

        public bool IsRequired { get; set; }

        public int SortOrder { get; set; }

        /// <summary>JSON array of option strings captured at occurrence generation.</summary>
        [StringLength(4000)]
        public string OptionsJson { get; set; }

        public virtual PreventiveMaintenanceChecklistResponse Response { get; set; }
    }
}
