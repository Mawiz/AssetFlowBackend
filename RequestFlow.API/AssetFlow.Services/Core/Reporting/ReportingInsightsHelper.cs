using AssetFlow.Services.Dto.Reporting;

namespace AssetFlow.Services.Core.Reporting
{
    public static class ReportingInsightsHelper
    {
        public static List<AnalyticsInsightDto> Build(
            DashboardSummaryDto summary,
            decimal? prevBreakdowns,
            decimal? prevCost,
            decimal? prevCompliance,
            decimal? prevConsumed,
            TopFailingAssetDto? topAsset,
            TopReplacedPartDto? topPart)
        {
            var list = new List<AnalyticsInsightDto>();

            if (summary.TopFailingAssets.Count >= 3)
            {
                var top3 = summary.TopFailingAssets.Take(3).Sum(x => x.BreakdownCount);
                var total = summary.BreakdownTrend.Sum(x => x.Value);
                if (total > 0)
                {
                    var pct = Math.Round(top3 / total * 100, 0);
                    if (pct >= 20)
                        list.Add(new AnalyticsInsightDto
                        {
                            Text = $"Top 3 assets account for {pct}% of breakdowns in this period.",
                            Severity = "warn",
                            DrillRoute = "/modules/analytics/reliability"
                        });
                }
            }

            if (prevCompliance.HasValue && summary.Maintenance.PmCompliancePercent.HasValue)
            {
                var delta = summary.Maintenance.PmCompliancePercent.Value - prevCompliance.Value;
                if (Math.Abs(delta) >= 2)
                {
                    var dir = delta > 0 ? "improved" : "dropped";
                    list.Add(new AnalyticsInsightDto
                    {
                        Text = $"PM compliance {dir} from {prevCompliance:0.#}% to {summary.Maintenance.PmCompliancePercent:0.#}%.",
                        Severity = delta > 0 ? "success" : "warn",
                        DrillRoute = "/modules/analytics/maintenance"
                    });
                }
            }

            if (topAsset != null && topAsset.BreakdownCount > 0)
            {
                list.Add(new AnalyticsInsightDto
                {
                    Text = $"{topAsset.AssetCode} has the highest breakdown count ({topAsset.BreakdownCount}) in this period.",
                    Severity = "warn",
                    DrillRoute = $"/modules/assets/{topAsset.AssetId}"
                });
            }

            if (topPart != null && topPart.ReplacementCount > 1)
            {
                list.Add(new AnalyticsInsightDto
                {
                    Text = $"{topPart.PartNumber} is the most frequently replaced part ({topPart.ReplacementCount} replacements).",
                    Severity = "info",
                    DrillRoute = "/modules/analytics/spare-parts"
                });
            }

            if (prevConsumed.HasValue && prevConsumed > 0 && summary.SpareParts.PartsConsumedInPeriod > 0)
            {
                var ch = ReportingPeriodHelper.ChangePercent(summary.SpareParts.PartsConsumedInPeriod, prevConsumed);
                if (ch.HasValue && Math.Abs(ch.Value) >= 10)
                {
                    list.Add(new AnalyticsInsightDto
                    {
                        Text = $"Parts consumption changed {ch.Value:0.#}% compared with the previous period.",
                        Severity = "info",
                        DrillRoute = "/modules/analytics/spare-parts"
                    });
                }
            }

            if (prevCost.HasValue && prevCost > 0)
            {
                var currentCost = summary.Maintenance.MaintenanceCostTrend.Sum(x => x.Value);
                var ch = ReportingPeriodHelper.ChangePercent(currentCost, prevCost);
                if (ch.HasValue && Math.Abs(ch.Value) >= 10)
                    list.Add(new AnalyticsInsightDto
                    {
                        Text = $"Maintenance cost trend total changed {ch.Value:0.#}% vs previous period.",
                        Severity = "info",
                        DrillRoute = "/modules/analytics/cost"
                    });
            }

            return list.Take(6).ToList();
        }
    }
}
