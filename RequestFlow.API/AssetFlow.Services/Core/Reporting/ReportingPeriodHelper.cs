namespace AssetFlow.Services.Core.Reporting
{
    public static class ReportingPeriodHelper
    {
        public static (DateTime? Start, DateTime? End) Resolve(string? preset, DateTime? customStart, DateTime? customEnd)
        {
            var today = DateTime.UtcNow.Date;
            if (string.IsNullOrWhiteSpace(preset) && customStart == null && customEnd == null)
                return (null, null);

            if (string.Equals(preset, "Custom", StringComparison.OrdinalIgnoreCase))
                return (customStart?.Date, customEnd?.Date.AddDays(1).AddTicks(-1));

            return preset?.Trim() switch
            {
                "Today" => (today, today.AddDays(1).AddTicks(-1)),
                "ThisWeek" => (StartOfWeek(today), today.AddDays(1).AddTicks(-1)),
                "ThisMonth" => (new DateTime(today.Year, today.Month, 1), today.AddDays(1).AddTicks(-1)),
                "LastMonth" => LastMonthRange(today),
                "ThisQuarter" => (StartOfQuarter(today), today.AddDays(1).AddTicks(-1)),
                "ThisYear" => (new DateTime(today.Year, 1, 1), today.AddDays(1).AddTicks(-1)),
                "7Days" => (today.AddDays(-6), today.AddDays(1).AddTicks(-1)),
                "30Days" => (today.AddDays(-29), today.AddDays(1).AddTicks(-1)),
                _ when customStart.HasValue || customEnd.HasValue =>
                    (customStart?.Date, (customEnd ?? customStart)?.Date.AddDays(1).AddTicks(-1)),
                _ => (null, null)
            };
        }

        private static DateTime StartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        private static DateTime StartOfQuarter(DateTime date)
        {
            var quarterMonth = ((date.Month - 1) / 3) * 3 + 1;
            return new DateTime(date.Year, quarterMonth, 1);
        }

        private static (DateTime Start, DateTime End) LastMonthRange(DateTime today)
        {
            var firstThisMonth = new DateTime(today.Year, today.Month, 1);
            var start = firstThisMonth.AddMonths(-1);
            var end = firstThisMonth.AddTicks(-1);
            return (start, end);
        }

        public static bool OverlapsPeriod(DateTime start, DateTime end, DateTime? periodStart, DateTime? periodEnd)
        {
            if (!periodStart.HasValue && !periodEnd.HasValue) return true;
            var ps = periodStart ?? DateTime.MinValue;
            var pe = periodEnd ?? DateTime.MaxValue;
            return start <= pe && end >= ps;
        }

        public static bool InPeriod(DateTime value, DateTime? periodStart, DateTime? periodEnd)
        {
            if (!periodStart.HasValue && !periodEnd.HasValue) return true;
            if (periodStart.HasValue && value < periodStart.Value) return false;
            if (periodEnd.HasValue && value > periodEnd.Value) return false;
            return true;
        }

        /// <summary>Previous period of equal length immediately before the current period.</summary>
        public static (DateTime? Start, DateTime? End) PreviousPeriod(DateTime? start, DateTime? end)
        {
            if (!start.HasValue || !end.HasValue) return (null, null);
            var span = end.Value - start.Value;
            if (span.TotalMinutes < 1) span = TimeSpan.FromDays(1);
            var prevEnd = start.Value.AddTicks(-1);
            var prevStart = prevEnd - span;
            return (prevStart, prevEnd);
        }

        public static decimal? ChangePercent(decimal? current, decimal? previous)
        {
            if (!current.HasValue || !previous.HasValue || previous.Value == 0) return null;
            return Math.Round((current.Value - previous.Value) / previous.Value * 100, 1);
        }
    }
}
