using AssetFlow.Common.Enum;
using AssetFlow.Data.Entities.Maintenance;

namespace AssetFlow.Services.Core.Maintenance
{
    public static class PreventiveMaintenanceStatusHelper
    {
        public static int ResolveOpenStatus(DateTime dueDateUtc)
        {
            var today = DateTime.UtcNow.Date;
            var due = dueDateUtc.Date;
            if (due < today) return (int)Enums.PreventiveMaintenanceOccurrenceStatus.Overdue;
            if (due == today) return (int)Enums.PreventiveMaintenanceOccurrenceStatus.Due;
            return (int)Enums.PreventiveMaintenanceOccurrenceStatus.Upcoming;
        }

        public static void RefreshOccurrenceStatus(PreventiveMaintenanceOccurrence occurrence)
        {
            if (occurrence.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed
                || occurrence.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Cancelled
                || occurrence.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.InProgress)
                return;
            occurrence.Status = ResolveOpenStatus(occurrence.DueDate);
        }

        public static string FormatFrequency(int recurrenceType, int intervalValue, int? dayOfWeek)
        {
            var iv = Math.Max(1, intervalValue);
            return recurrenceType switch
            {
                (int)Enums.MaintenanceRecurrenceType.Daily => iv == 1 ? "Daily" : $"Every {iv} days",
                (int)Enums.MaintenanceRecurrenceType.Weekly => $"Every {iv} week(s)" + (dayOfWeek.HasValue ? $" ({(DayOfWeek)dayOfWeek.Value})" : ""),
                (int)Enums.MaintenanceRecurrenceType.Monthly => iv == 1 ? "Monthly" : $"Every {iv} month(s)",
                (int)Enums.MaintenanceRecurrenceType.Quarterly => "Quarterly",
                (int)Enums.MaintenanceRecurrenceType.HalfYearly => "Half-yearly",
                (int)Enums.MaintenanceRecurrenceType.Yearly => "Yearly",
                (int)Enums.MaintenanceRecurrenceType.EveryDays => $"Every {iv} days",
                (int)Enums.MaintenanceRecurrenceType.OperatingHours => $"Every {iv} operating hours",
                (int)Enums.MaintenanceRecurrenceType.Cycles => $"Every {iv} cycles",
                (int)Enums.MaintenanceRecurrenceType.Custom => $"Custom ({iv} days)",
                _ => "Unknown"
            };
        }
    }
}
