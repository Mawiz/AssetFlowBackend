using System.ComponentModel.DataAnnotations;

namespace AssetFlow.Data.Entities.Maintenance
{
    public class PreventiveMaintenanceChecklistResponse : BaseModel
    {
        public int PreventiveMaintenanceOccurrenceId { get; set; }
        public virtual PreventiveMaintenanceOccurrence Occurrence { get; set; }

        public int OccurrenceChecklistItemId { get; set; }
        public virtual PreventiveMaintenanceOccurrenceChecklistItem OccurrenceChecklistItem { get; set; }

        [StringLength(500)]
        public string ResponseValue { get; set; } = string.Empty;

        public decimal? NumericValue { get; set; }

        [StringLength(1000)]
        public string Remarks { get; set; } = string.Empty;
    }
}
