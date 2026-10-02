using AssetFlow.Common.Enum;

namespace AssetFlow.Services.Core.Maintenance
{
    /// <summary>
    /// Central recurrence calculations for PM schedules.
    /// Monthly (and multi-month) rule: preserve day-of-month when possible; clamp to last day of target month (e.g. 31-Jan → 28/29-Feb).
    /// </summary>
    public static class RecurrenceCalculator
    {
        public static bool IsMeterBased(int recurrenceType) =>
            recurrenceType == (int)Enums.MaintenanceRecurrenceType.OperatingHours
            || recurrenceType == (int)Enums.MaintenanceRecurrenceType.Cycles;

        public static DateTime? CalculateFirstDueDate(DateTime startDate, int recurrenceType, int intervalValue, int? dayOfWeek)
        {
            if (IsMeterBased(recurrenceType)) return null;
            var start = startDate.Date;
            var today = DateTime.UtcNow.Date;
            var cursor = start;
            if (cursor < today) cursor = today;
            return AlignToRecurrence(start, cursor, recurrenceType, intervalValue, dayOfWeek, forward: true);
        }

        public static DateTime? CalculateNextDueDate(DateTime? previousDueDate, DateTime scheduleStartDate, int recurrenceType, int intervalValue, int? dayOfWeek)
        {
            if (IsMeterBased(recurrenceType)) return null;
            var interval = Math.Max(1, intervalValue);
            var anchor = previousDueDate?.Date ?? scheduleStartDate.Date;
            return recurrenceType switch
            {
                (int)Enums.MaintenanceRecurrenceType.Daily => anchor.AddDays(interval),
                (int)Enums.MaintenanceRecurrenceType.Weekly => NextWeekly(anchor, interval, dayOfWeek, includeSameDay: false),
                (int)Enums.MaintenanceRecurrenceType.Monthly => AddCalendarMonths(anchor, interval),
                (int)Enums.MaintenanceRecurrenceType.Quarterly => AddCalendarMonths(anchor, 3 * interval),
                (int)Enums.MaintenanceRecurrenceType.HalfYearly => AddCalendarMonths(anchor, 6 * interval),
                (int)Enums.MaintenanceRecurrenceType.Yearly => AddCalendarMonths(anchor, 12 * interval),
                (int)Enums.MaintenanceRecurrenceType.EveryDays => anchor.AddDays(interval),
                (int)Enums.MaintenanceRecurrenceType.Custom => anchor.AddDays(interval),
                _ => anchor.AddDays(interval)
            };
        }

        public static decimal? CalculateNextMeterThreshold(decimal? previousThreshold, int intervalValue)
        {
            var interval = Math.Max(1, intervalValue);
            return (previousThreshold ?? 0) + interval;
        }

        private static DateTime? AlignToRecurrence(DateTime scheduleStart, DateTime candidate, int recurrenceType, int intervalValue, int? dayOfWeek, bool forward)
        {
            var interval = Math.Max(1, intervalValue);
            if (recurrenceType == (int)Enums.MaintenanceRecurrenceType.Weekly)
                return NextWeekly(candidate, interval, dayOfWeek, includeSameDay: true);

            if (candidate <= scheduleStart) return scheduleStart;

            var cursor = scheduleStart;
            var guard = 0;
            while (cursor < candidate && guard++ < 5000)
            {
                var next = CalculateNextDueDate(cursor, scheduleStart, recurrenceType, interval, dayOfWeek);
                if (next == null) return scheduleStart;
                cursor = next.Value;
            }
            return cursor;
        }

        private static DateTime NextWeekly(DateTime from, int intervalWeeks, int? dayOfWeek, bool includeSameDay)
        {
            var targetDow = dayOfWeek ?? (int)from.DayOfWeek;
            if (targetDow < 0 || targetDow > 6) targetDow = (int)from.DayOfWeek;
            var date = from.Date;
            var delta = (targetDow - (int)date.DayOfWeek + 7) % 7;
            if (delta == 0 && !includeSameDay) delta = 7 * Math.Max(1, intervalWeeks);
            else if (delta == 0 && includeSameDay) return date;
            else if (delta > 0 && intervalWeeks > 1 && !includeSameDay)
                return date.AddDays(delta + 7 * (Math.Max(1, intervalWeeks) - 1));
            return date.AddDays(delta == 0 ? 7 * Math.Max(1, intervalWeeks) : delta);
        }

        private static DateTime AddCalendarMonths(DateTime from, int months)
        {
            if (months <= 0) months = 1;
            var target = from.AddMonths(months);
            var maxDay = DateTime.DaysInMonth(target.Year, target.Month);
            var day = Math.Min(from.Day, maxDay);
            return new DateTime(target.Year, target.Month, day, 0, 0, 0, DateTimeKind.Utc);
        }
    }
}
