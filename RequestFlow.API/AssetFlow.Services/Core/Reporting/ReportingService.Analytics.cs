using AssetFlow.Common.Enum;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Reporting;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AssetFlow.Services.Core.Reporting
{
    public partial class ReportingService
    {
        public async Task<ResponseDto<ManagementAnalyticsDto>> GetManagementAnalyticsAsync(ReportingFilterDto filter)
        {
            var response = new ResponseDto<ManagementAnalyticsDto>();
            if (!await ValidateFilterAsync(filter, response)) return response;
            ResolvePeriod(filter);

            var summaryResp = await GetDashboardSummaryAsync(CloneFilter(filter));
            if (!summaryResp.Success) { response.AddError(summaryResp.Errors); return response; }

            var prevFilter = PreviousFilter(filter);
            var prevBreakdowns = await FilteredIssuesQuery(prevFilter).CountAsync();
            var prevCost = await FilteredCostQuery(prevFilter).SumAsync(c => (decimal?)c.Amount) ?? 0;
            var prevCompliance = await ComputePmComplianceAsync(prevFilter);
            var prevConsumed = await TenantScopeHelper.ApplyTenantScope(_context.PartTransactions.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(t => !t.IsDeleted && t.TransactionType == (int)Enums.PartTransactionType.Issue)
                .Where(t => !prevFilter.StartDate.HasValue || t.TransactionDate >= prevFilter.StartDate)
                .Where(t => !prevFilter.EndDate.HasValue || t.TransactionDate <= prevFilter.EndDate)
                .SumAsync(t => (decimal?)t.Quantity) ?? 0;

            var currentBreakdowns = await FilteredIssuesQuery(filter).CountAsync();
            var currentCost = await FilteredCostQuery(filter).SumAsync(c => (decimal?)c.Amount) ?? 0;
            var topParts = await BuildTopReplacedPartsAsync(filter);

            var hero = BuildManagementHeroKpis(summaryResp.Result!, currentBreakdowns, prevBreakdowns, currentCost, prevCost);
            var attention = await BuildAttentionAssetsAsync(filter);
            var activity = await BuildMaintenanceActivitySeriesAsync(filter);
            var pipeline = await BuildWorkOrderPipelineAsync(filter);
            var costStack = await BuildStackedCostTrendAsync(filter);
            var downtimeTrend = await BuildDowntimeTrendAsync(filter);
            var topDowntime = (await BuildTopFailingAssetsAsync(filter, 8)).OrderByDescending(x => x.TotalDowntimeMinutes).ToList();
            var stockLoc = await BuildStockByLocationAsync(filter);
            var engineers = (await BuildEngineerPerformanceAsync(filter)).Take(8).ToList();

            var insights = ReportingInsightsHelper.Build(
                summaryResp.Result!,
                prevBreakdowns,
                prevCost,
                prevCompliance,
                prevConsumed,
                summaryResp.Result!.TopFailingAssets.FirstOrDefault(),
                topParts.FirstOrDefault());

            response.Result = new ManagementAnalyticsDto
            {
                Summary = summaryResp.Result!,
                HeroKpis = hero,
                Insights = insights,
                AttentionAssets = attention,
                MaintenanceActivity = activity,
                WorkOrderPipeline = pipeline,
                CostTrendStacked = costStack,
                DowntimeTrend = downtimeTrend,
                TopDowntimeAssets = topDowntime,
                TopReplacedParts = topParts.Take(8).ToList(),
                StockByLocation = stockLoc,
                TeamPerformance = engineers,
                LastUpdatedUtc = DateTime.UtcNow
            };
            return response;
        }

        public async Task<ResponseDto<AssetAnalyticsDashboardDto>> GetAssetAnalyticsDashboardAsync(ReportingFilterDto filter)
        {
            var response = new ResponseDto<AssetAnalyticsDashboardDto>();
            if (!await ValidateFilterAsync(filter, response)) return response;
            ResolvePeriod(filter);

            var summary = await BuildAssetSummaryAsync(filter, true);
            var byType = await AssetGroupChartAsync(filter, true);
            var byLoc = await BuildLocationAssetMetricsAsync(filter);
            var age = await BuildAssetAgeDistributionAsync(filter);
            var reliability = await BuildAssetReliabilityAsync(filter);
            var scatter = reliability
                .Where(r => r.MtbfHours.HasValue && r.MttrHours.HasValue)
                .Select(r => new ScatterPointDto
                {
                    AssetId = r.AssetId,
                    Label = r.AssetCode,
                    X = r.MtbfHours!.Value,
                    Y = r.MttrHours!.Value,
                    Size = r.Breakdowns,
                    Tooltip = $"{r.AssetCode}: MTBF {r.MtbfHours:0.#}h, MTTR {r.MttrHours:0.#}h, breakdowns {r.Breakdowns}"
                }).ToList();

            var total = summary.TotalAssets;
            var kpis = new List<KpiMetricDto>
            {
                Kpi("total", "Total Assets", total, null, "number", "/modules/assets"),
                Kpi("critical", "Critical Assets", summary.Critical, null, "number", "/modules/assets", false, "criticality=1"),
                Kpi("operationalPct", "Operational %", total == 0 ? null : Math.Round((decimal)summary.Operational / total * 100, 1), null, "percent"),
                Kpi("breakdownPct", "Breakdown %", total == 0 ? null : Math.Round((decimal)summary.Breakdown / total * 100, 1), null, "percent", "/modules/assets", false, "status=3"),
                Kpi("underMaint", "Under Maintenance", summary.UnderMaintenance, null, "number", "/modules/assets", false, "status=2"),
                Kpi("overduePm", "Assets w/ Overdue PM", await CountAssetsWithOverduePmAsync(filter), null, "number", "/modules/analytics/maintenance"),
                Kpi("repeatFail", "Repeated Failures", await CountRepeatedFailureAssetsAsync(filter), null, "number", "/modules/analytics/reliability")
            };

            response.Result = new AssetAnalyticsDashboardDto
            {
                Summary = summary,
                Kpis = kpis,
                ByType = byType,
                ByLocation = byLoc,
                AgeDistribution = age,
                Reliability = reliability,
                MtbfMttrScatter = scatter
            };
            return response;
        }

        public async Task<ResponseDto<MaintenanceAnalyticsDashboardDto>> GetMaintenanceAnalyticsDashboardAsync(ReportingFilterDto filter)
        {
            var response = new ResponseDto<MaintenanceAnalyticsDashboardDto>();
            if (!await ValidateFilterAsync(filter, response)) return response;
            ResolvePeriod(filter);

            var maintReport = await GetMaintenanceReportAsync(CloneFilter(filter));
            if (!maintReport.Success) { response.AddError(maintReport.Errors); return response; }

            var pm = maintReport.Result!.PreventiveMaintenance;
            var onTime = pm.Completed; // simplified; compliance uses due logic in service
            var complianceBreakdown = new List<ChartPointDto>
            {
                new() { Label = "Completed on time", Value = onTime },
                new() { Label = "Overdue", Value = pm.Overdue },
                new() { Label = "Due", Value = pm.Due }
            };

            var pmTrend = new TimeSeriesMultiDto
            {
                Labels = pm.CompletionTrend.Select(x => x.Label).ToList(),
                Series = new List<NamedSeriesDto>
                {
                    new() { Name = "Completed", Values = pm.CompletionTrend.Select(x => x.Value).ToList() }
                }
            };

            var overdueRows = await BuildPmOverdueTableAsync(filter);
            var woPriority = await BuildWorkOrderPriorityDistAsync(filter);
            var pmByCat = await BuildPmComplianceByCategoryAsync(filter);

            response.Result = new MaintenanceAnalyticsDashboardDto
            {
                Summary = maintReport.Result!.Summary,
                PreventiveMaintenance = pm,
                OverduePm = overdueRows,
                PmComplianceBreakdown = complianceBreakdown,
                PmTrend = pmTrend,
                WorkOrdersByPriority = woPriority,
                PmByCategory = pmByCat
            };
            return response;
        }

        public async Task<ResponseDto<ReliabilityAnalyticsDashboardDto>> GetReliabilityAnalyticsDashboardAsync(ReportingFilterDto filter)
        {
            var response = new ResponseDto<ReliabilityAnalyticsDashboardDto>();
            if (!await ValidateFilterAsync(filter, response)) return response;
            ResolvePeriod(filter);

            var breakdown = await GetBreakdownReportAsync(CloneFilter(filter));
            var performance = await BuildPerformanceSummaryAsync(filter);
            var reliability = await BuildAssetReliabilityAsync(filter);

            var mtbfMttr = reliability.Where(r => r.MtbfHours.HasValue && r.MttrHours.HasValue)
                .Select(r => new ScatterPointDto
                {
                    AssetId = r.AssetId, Label = r.AssetCode, X = r.MtbfHours!.Value, Y = r.MttrHours!.Value,
                    Tooltip = $"{r.AssetCode}: MTBF {r.MtbfHours:0.#}h, MTTR {r.MttrHours:0.#}h"
                }).ToList();

            var costScatter = reliability.Where(r => r.Breakdowns > 0)
                .Select(r => new ScatterPointDto
                {
                    AssetId = r.AssetId, Label = r.AssetCode, X = r.Breakdowns, Y = r.MaintenanceCost,
                    Tooltip = $"{r.AssetCode}: {r.Breakdowns} breakdowns, cost {r.MaintenanceCost:0.##}"
                }).ToList();

            var repeated = breakdown.Result!.TopFailingAssets.Where(x => x.BreakdownCount > 1).ToList();

            response.Result = new ReliabilityAnalyticsDashboardDto
            {
                Breakdown = breakdown.Result!,
                Performance = performance,
                MtbfMttrScatter = mtbfMttr,
                CostVsBreakdownScatter = costScatter,
                RepeatedFailures = repeated
            };
            return response;
        }

        public async Task<ResponseDto<WorkOrderAnalyticsDashboardDto>> GetWorkOrderAnalyticsDashboardAsync(ReportingFilterDto filter)
        {
            var response = new ResponseDto<WorkOrderAnalyticsDashboardDto>();
            if (!await ValidateFilterAsync(filter, response)) return response;
            ResolvePeriod(filter);

            var assetIds = ScopedAssets(filter).Select(a => a.Id);
            var woQ = TenantScopeHelper.ApplyTenantScope(_context.WorkOrders.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(w => !w.IsDeleted && assetIds.Contains(w.AssetId));

            var pipeline = await woQ.GroupBy(w => w.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync();
            var pipelineChart = pipeline.Select(g => new ChartPointDto
            {
                Label = Enum.GetName(typeof(Enums.WorkOrderStatus), g.Status) ?? g.Status.ToString(),
                Value = g.Count
            }).OrderByDescending(x => x.Value).ToList();

            var openCreated = await woQ.Where(w => OpenWorkOrderStatuses.Contains(w.Status))
                .Select(w => w.CreatedOn).ToListAsync();
            var now = DateTime.UtcNow;
            int AgeCount(Func<double, bool> pred) => openCreated.Count(d => pred((now - d).TotalDays));
            var aging = new List<WorkOrderAgingBucketDto>
            {
                Bucket("0–1 day", AgeCount(days => days <= 1)),
                Bucket("2–3 days", AgeCount(days => days > 1 && days <= 3)),
                Bucket("4–7 days", AgeCount(days => days > 3 && days <= 7)),
                Bucket("8–14 days", AgeCount(days => days > 7 && days <= 14)),
                Bucket("15+ days", AgeCount(days => days > 14))
            };

            var byPriority = await woQ.GroupBy(w => w.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() }).ToListAsync();
            var priorityChart = byPriority.Select(g => new ChartPointDto
            {
                Label = Enum.GetName(typeof(Enums.IssuePriority), g.Priority) ?? g.Priority.ToString(),
                Value = g.Count
            }).ToList();

            var createdTrend = await BuildWoCreatedSeriesAsync(woQ, filter);
            var completedTrend = await BuildWoCompletedSeriesAsync(woQ, filter);
            var trendLabels = createdTrend.Labels.Count >= completedTrend.Labels.Count ? createdTrend.Labels : completedTrend.Labels;

            var kpis = new List<KpiMetricDto>
            {
                Kpi("total", "Total Work Orders", await woQ.CountAsync(), null, "number", "/modules/work-orders"),
                Kpi("open", "Open", await woQ.CountAsync(w => OpenWorkOrderStatuses.Contains(w.Status)), null, "number", "/modules/work-orders"),
                Kpi("inProgress", "In Progress", await woQ.CountAsync(w => w.Status == (int)Enums.WorkOrderStatus.InProgress), null, "number", "/modules/work-orders", false, "status=6"),
                Kpi("waitingParts", "Waiting for Parts", await woQ.CountAsync(w => w.Status == (int)Enums.WorkOrderStatus.WaitingForParts), null, "number", "/modules/work-orders", false, "status=7"),
                Kpi("reopened", "Reopened", await woQ.CountAsync(w => w.Status == (int)Enums.WorkOrderStatus.Reopened), null, "number", "/modules/work-orders", false, "status=11")
            };

            response.Result = new WorkOrderAnalyticsDashboardDto
            {
                StatusPipeline = pipelineChart,
                Aging = aging,
                ByPriority = priorityChart,
                CompletionTrend = new TimeSeriesMultiDto
                {
                    Labels = trendLabels,
                    Series = new List<NamedSeriesDto>
                    {
                        new() { Name = createdTrend.Name, Values = AlignSeries(trendLabels, createdTrend.Labels, createdTrend.Values) },
                        new() { Name = completedTrend.Name, Values = AlignSeries(trendLabels, completedTrend.Labels, completedTrend.Values) }
                    }
                },
                Kpis = kpis
            };
            return response;
        }

        public async Task<ResponseDto<SparePartsAnalyticsDashboardDto>> GetSparePartsAnalyticsDashboardAsync(ReportingFilterDto filter)
        {
            var response = new ResponseDto<SparePartsAnalyticsDashboardDto>();
            if (!await ValidateFilterAsync(filter, response)) return response;
            ResolvePeriod(filter);

            var report = await GetSparePartsReportAsync(CloneFilter(filter));
            var invQ = TenantScopeHelper.ApplyTenantScope(_context.PartInventories.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(i => !i.IsDeleted);
            var totalStock = await invQ.SumAsync(i => (decimal?)i.AvailableQuantity) ?? 0;
            var faulty = await invQ.SumAsync(i => (decimal?)i.FaultyQuantity) ?? 0;

            var consumption = TenantScopeHelper.ApplyTenantScope(_context.PartTransactions.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(t => !t.IsDeleted && t.TransactionType == (int)Enums.PartTransactionType.Issue);
            if (filter.StartDate.HasValue) consumption = consumption.Where(t => t.TransactionDate >= filter.StartDate);
            if (filter.EndDate.HasValue) consumption = consumption.Where(t => t.TransactionDate <= filter.EndDate);
            var consTrend = await consumption.GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Sum = g.Sum(x => x.Quantity) })
                .OrderBy(x => x.Year).ThenBy(x => x.Month).ToListAsync();

            var partsQ = TenantScopeHelper.ApplyTenantScope(_context.Parts.AsQueryable(), _tenantProvider, filter.TenantId).Where(p => !p.IsDeleted);
            var stockByPart = await invQ.GroupBy(i => i.PartId).Select(g => new { PartId = g.Key, Qty = g.Sum(x => x.AvailableQuantity) }).ToListAsync();
            var minLevels = await partsQ.Select(p => new { p.Id, p.MinStockLevel }).ToListAsync();
            int low = minLevels.Count(p => (stockByPart.FirstOrDefault(s => s.PartId == p.Id)?.Qty ?? 0) < p.MinStockLevel);
            int healthy = minLevels.Count - low;

            response.Result = new SparePartsAnalyticsDashboardDto
            {
                Report = report.Result!,
                TotalStockQuantity = totalStock,
                FaultyStockQuantity = faulty,
                ConsumptionTrend = consTrend.Select(c => new ChartPointDto { Label = YearMonthLabel(c.Year, c.Month), Value = c.Sum }).ToList(),
                InventoryHealth = new List<ChartPointDto>
                {
                    new() { Label = "Healthy", Value = healthy },
                    new() { Label = "Low stock", Value = low }
                }
            };
            return response;
        }

        public async Task<ResponseDto<CostAnalyticsDashboardDto>> GetCostAnalyticsDashboardAsync(ReportingFilterDto filter)
        {
            var response = new ResponseDto<CostAnalyticsDashboardDto>();
            if (!await ValidateFilterAsync(filter, response)) return response;
            ResolvePeriod(filter);

            var report = await GetCostReportAsync(CloneFilter(filter));
            var prev = PreviousFilter(filter);
            var prevTotal = await FilteredCostQuery(prev).SumAsync(c => (decimal?)c.Amount) ?? 0;
            var woCount = await TenantScopeHelper.ApplyTenantScope(_context.WorkOrders.AsQueryable(), _tenantProvider, filter.TenantId)
                .CountAsync(w => !w.IsDeleted);
            var avgWo = woCount == 0 ? (decimal?)null : report.Result!.GrandTotal / woCount;

            var kpis = new List<KpiMetricDto>
            {
                Kpi("total", "Total Maintenance Cost", report.Result!.GrandTotal, prevTotal, "currency", "/modules/analytics/cost"),
                Kpi("labor", "Labor", report.Result.TotalLabor, null, "currency"),
                Kpi("parts", "Parts", report.Result.TotalParts, null, "currency"),
                Kpi("avgWo", "Avg cost / WO", avgWo, null, "currency")
            };

            var breakdownByAsset = await FilteredIssuesQuery(filter).GroupBy(i => i.AssetId)
                .Select(g => new { AssetId = g.Key, Count = g.Count() }).ToListAsync();
            var scatter = report.Result.ByAsset.Where(a => a.TotalMaintenanceCost > 0)
                .Select(a =>
                {
                    var b = breakdownByAsset.FirstOrDefault(x => x.AssetId == a.AssetId);
                    if (b == null || b.Count == 0) return null;
                    return new ScatterPointDto
                    {
                        AssetId = a.AssetId, Label = a.AssetCode, X = b.Count, Y = a.TotalMaintenanceCost,
                        Tooltip = $"{a.AssetCode}: {b.Count} breakdowns, {a.TotalMaintenanceCost:0.##} cost"
                    };
                }).Where(x => x != null).Cast<ScatterPointDto>().ToList();

            response.Result = new CostAnalyticsDashboardDto
            {
                Report = report.Result,
                Kpis = kpis,
                CostVsBreakdownScatter = scatter
            };
            return response;
        }

        public async Task<ResponseDto<AssetDetailAnalyticsDto>> GetAssetDetailAnalyticsAsync(int assetId, ReportingFilterDto filter)
        {
            filter.AssetId = assetId;
            var response = new ResponseDto<AssetDetailAnalyticsDto>();
            if (!await ValidateFilterAsync(filter, response)) return response;
            ResolvePeriod(filter);

            var asset = await _context.Assets.AsNoTracking().Include(a => a.AssetCategory).FirstOrDefaultAsync(a => a.Id == assetId && !a.IsDeleted);
            if (asset == null) { response.AddError("Asset not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (!EnsureAccess(asset.TenantId, response)) return response;

            var f = CloneFilterForAsset(filter, assetId);
            var summary = await _context.AssetIssues.AsNoTracking().CountAsync(i => i.AssetId == assetId && !i.IsDeleted);
            var downtime = await ComputeTotalDowntimeMinutesAsync(f);
            var cost = await FilteredCostQuery(f).SumAsync(c => (decimal?)c.Amount) ?? 0;
            var repl = await _context.PartReplacements.AsNoTracking().CountAsync(r => r.AssetId == assetId && !r.IsDeleted);
            var mtbf = await ComputeMtbfHoursForAssetAsync(assetId, f);
            var mttr = await ComputeMttrHoursForAssetAsync(assetId, f);
            var lastPm = await TenantScopeHelper.ApplyTenantScope(_context.PreventiveMaintenanceOccurrences.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(p => p.AssetId == assetId && p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed)
                .OrderByDescending(p => p.CompletedAt).Select(p => p.CompletedAt).FirstOrDefaultAsync();
            var nextPm = await TenantScopeHelper.ApplyTenantScope(_context.PreventiveMaintenanceOccurrences.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(p => p.AssetId == assetId && p.Status != (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed && p.Status != (int)Enums.PreventiveMaintenanceOccurrenceStatus.Cancelled)
                .OrderBy(p => p.DueDate).Select(p => (DateTime?)p.DueDate).FirstOrDefaultAsync();

            var kpis = new List<KpiMetricDto>
            {
                Kpi("breakdowns", "Breakdowns", summary, null, "number"),
                Kpi("downtime", "Downtime (min)", downtime, null, "number"),
                Kpi("cost", "Maintenance cost", cost, null, "currency"),
                Kpi("replacements", "Parts replaced", repl, null, "number")
            };

            response.Result = new AssetDetailAnalyticsDto
            {
                AssetId = assetId,
                AssetCode = asset.AssetCode,
                AssetName = asset.Name,
                Status = asset.Status,
                Criticality = asset.Criticality,
                Kpis = kpis,
                BreakdownTrend = await BuildBreakdownTrendAsync(f),
                CostTrend = await BuildStackedCostTrendAsync(f),
                MtbfHours = mtbf,
                MttrHours = mttr,
                LastMaintenance = lastPm,
                NextMaintenanceDue = nextPm
            };
            return response;
        }

        private static ReportingFilterDto CloneFilter(ReportingFilterDto f) => new()
        {
            TenantId = f.TenantId,
            PeriodPreset = f.PeriodPreset,
            StartDate = f.StartDate,
            EndDate = f.EndDate,
            AssetId = f.AssetId,
            AssetCategoryId = f.AssetCategoryId,
            AssetTypeId = f.AssetTypeId,
            LocationId = f.LocationId,
            EngineerUserId = f.EngineerUserId,
            PartId = f.PartId,
            IssueCategoryId = f.IssueCategoryId,
            Priority = f.Priority,
            PageSize = f.PageSize,
            PageNumber = f.PageNumber
        };

        private static ReportingFilterDto PreviousFilter(ReportingFilterDto f)
        {
            var (ps, pe) = ReportingPeriodHelper.PreviousPeriod(f.StartDate, f.EndDate);
            var c = CloneFilter(f);
            c.StartDate = ps;
            c.EndDate = pe;
            c.PeriodPreset = null;
            return c;
        }

        private static KpiMetricDto Kpi(string key, string label, decimal? value, decimal? prev, string unit,
            string? route = null, bool higherIsBetter = true, string? query = null)
        {
            return new KpiMetricDto
            {
                Key = key,
                Label = label,
                Value = value,
                PreviousValue = prev,
                ChangePercent = ReportingPeriodHelper.ChangePercent(value, prev),
                Unit = unit,
                DrillRoute = route,
                DrillQuery = query,
                HigherIsBetter = higherIsBetter
            };
        }

        private static WorkOrderAgingBucketDto Bucket(string label, int count) => new() { Label = label, Count = count };

        private List<KpiMetricDto> BuildManagementHeroKpis(DashboardSummaryDto s, int breakdowns, int prevBreakdowns, decimal cost, decimal prevCost)
        {
            return new List<KpiMetricDto>
            {
                Kpi("assets", "Total Assets", s.Asset.TotalAssets, null, "number", "/modules/assets"),
                Kpi("operational", "Operational", s.Asset.Operational, null, "number", "/modules/assets", true, "status=1"),
                Kpi("underMaint", "Under Maintenance", s.Asset.UnderMaintenance, null, "number", "/modules/assets", false, "status=2"),
                Kpi("breakdown", "Breakdown", s.Asset.Breakdown, null, "number", "/modules/assets", false, "status=3"),
                Kpi("critical", "Critical Assets", s.Asset.Critical, null, "number", "/modules/assets", false, "criticality=1"),
                Kpi("openWo", "Open Work Orders", s.Maintenance.OpenWorkOrders, null, "number", "/modules/work-orders"),
                Kpi("overduePm", "Overdue PM", s.Maintenance.OverdueMaintenance, null, "number", "/modules/analytics/maintenance"),
                Kpi("maintCost", "Maintenance Cost", cost, prevCost, "currency", "/modules/analytics/cost", false),
                Kpi("periodBreakdowns", "Breakdowns (period)", breakdowns, prevBreakdowns, "number", "/modules/analytics/reliability", false)
            };
        }

        private async Task<List<AssetAttentionDto>> BuildAttentionAssetsAsync(ReportingFilterDto filter)
        {
            var top = await BuildTopFailingAssetsAsync(filter, 15);
            var assetIds = top.Select(t => t.AssetId).ToList();
            var overduePm = await TenantScopeHelper.ApplyTenantScope(_context.PreventiveMaintenanceOccurrences.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(p => !p.IsDeleted && assetIds.Contains(p.AssetId))
                .Where(p => p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Overdue
                    || (p.DueDate.Date < DateTime.UtcNow.Date && p.Status != (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed))
                .GroupBy(p => p.AssetId).Select(g => new { AssetId = g.Key, Count = g.Count() }).ToListAsync();

            var list = new List<AssetAttentionDto>();
            foreach (var t in top.Take(10))
            {
                var reasons = new List<string>();
                if (t.BreakdownCount > 1) reasons.Add($"{t.BreakdownCount} breakdowns");
                var od = overduePm.FirstOrDefault(o => o.AssetId == t.AssetId)?.Count ?? 0;
                if (od > 0) reasons.Add($"{od} overdue PM");
                if (t.TotalDowntimeMinutes >= 60) reasons.Add($"{Math.Round(t.TotalDowntimeMinutes / 60, 1)}h downtime");
                if (t.MaintenanceCost > 0) reasons.Add($"{t.MaintenanceCost:0.##} maintenance cost");
                if (reasons.Count == 0) continue;

                var mttr = await ComputeMttrHoursForAssetAsync(t.AssetId, CloneFilterForAsset(filter, t.AssetId));
                list.Add(new AssetAttentionDto
                {
                    AssetId = t.AssetId,
                    AssetCode = t.AssetCode,
                    AssetName = t.AssetName,
                    Reasons = reasons,
                    BreakdownCount = t.BreakdownCount,
                    OverduePmCount = od,
                    DowntimeMinutes = t.TotalDowntimeMinutes,
                    MaintenanceCost = t.MaintenanceCost,
                    MttrHours = mttr
                });
            }
            return list;
        }

        private async Task<TimeSeriesMultiDto> BuildMaintenanceActivitySeriesAsync(ReportingFilterDto filter)
        {
            var issues = await FilteredIssuesQuery(filter).GroupBy(i => new { i.ReportedAt.Year, i.ReportedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() }).ToListAsync();
            var assetIds = ScopedAssets(filter).Select(a => a.Id);
            var woQ = TenantScopeHelper.ApplyTenantScope(_context.WorkOrders.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(w => !w.IsDeleted && assetIds.Contains(w.AssetId));
            if (filter.StartDate.HasValue) woQ = woQ.Where(w => w.CreatedOn >= filter.StartDate);
            if (filter.EndDate.HasValue) woQ = woQ.Where(w => w.CreatedOn <= filter.EndDate);
            var wos = await woQ.GroupBy(w => new { w.CreatedOn.Year, w.CreatedOn.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() }).ToListAsync();
            var pmQ = TenantScopeHelper.ApplyTenantScope(_context.PreventiveMaintenanceOccurrences.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(p => !p.IsDeleted && assetIds.Contains(p.AssetId) && p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed);
            if (filter.StartDate.HasValue) pmQ = pmQ.Where(p => p.CompletedAt >= filter.StartDate);
            if (filter.EndDate.HasValue) pmQ = pmQ.Where(p => p.CompletedAt <= filter.EndDate);
            var pms = await pmQ.GroupBy(p => new { p.CompletedAt!.Value.Year, p.CompletedAt.Value.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() }).ToListAsync();

            var keys = issues.Select(i => (i.Year, i.Month))
                .Union(wos.Select(w => (w.Year, w.Month)))
                .Union(pms.Select(p => (p.Year, p.Month)))
                .OrderBy(k => k.Year).ThenBy(k => k.Month).ToList();

            return new TimeSeriesMultiDto
            {
                Labels = keys.Select(k => YearMonthLabel(k.Year, k.Month)).ToList(),
                Series = new List<NamedSeriesDto>
                {
                    new() { Name = "Issues", Values = keys.Select(k => (decimal)(issues.FirstOrDefault(i => i.Year == k.Year && i.Month == k.Month)?.Count ?? 0)).ToList() },
                    new() { Name = "Work orders", Values = keys.Select(k => (decimal)(wos.FirstOrDefault(w => w.Year == k.Year && w.Month == k.Month)?.Count ?? 0)).ToList() },
                    new() { Name = "PM completed", Values = keys.Select(k => (decimal)(pms.FirstOrDefault(p => p.Year == k.Year && p.Month == k.Month)?.Count ?? 0)).ToList() }
                }
            };
        }

        private async Task<List<ChartPointDto>> BuildWorkOrderPipelineAsync(ReportingFilterDto filter)
        {
            var assetIds = ScopedAssets(filter).Select(a => a.Id);
            var woQ = TenantScopeHelper.ApplyTenantScope(_context.WorkOrders.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(w => !w.IsDeleted && assetIds.Contains(w.AssetId) && OpenWorkOrderStatuses.Contains(w.Status));
            var rows = await woQ.GroupBy(w => w.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync();
            return rows.Select(r => new ChartPointDto
            {
                Label = Enum.GetName(typeof(Enums.WorkOrderStatus), r.Status) ?? r.Status.ToString(),
                Value = r.Count
            }).OrderByDescending(x => x.Value).ToList();
        }

        private async Task<List<StackedCostMonthDto>> BuildStackedCostTrendAsync(ReportingFilterDto filter)
        {
            var costs = await FilteredCostQuery(filter)
                .GroupBy(c => new { c.CostDate.Year, c.CostDate.Month, c.CostType })
                .Select(g => new { g.Key.Year, g.Key.Month, g.Key.CostType, Sum = g.Sum(x => x.Amount) }).ToListAsync();
            var months = costs.Select(c => (c.Year, c.Month)).Distinct().OrderBy(m => m.Year).ThenBy(m => m.Month);
            return months.Select(m =>
            {
                decimal S(int t) => costs.Where(c => c.Year == m.Year && c.Month == m.Month && c.CostType == t).Sum(c => c.Sum);
                return new StackedCostMonthDto
                {
                    Label = YearMonthLabel(m.Year, m.Month),
                    Labor = S((int)Enums.MaintenanceCostType.Labor),
                    Parts = S((int)Enums.MaintenanceCostType.Parts),
                    External = S((int)Enums.MaintenanceCostType.ExternalService),
                    Other = S((int)Enums.MaintenanceCostType.Other)
                };
            }).ToList();
        }

        private async Task<List<ChartPointDto>> BuildDowntimeTrendAsync(ReportingFilterDto filter)
        {
            return await BuildBreakdownTrendAsync(filter);
        }

        private async Task<List<ChartPointDto>> BuildStockByLocationAsync(ReportingFilterDto filter)
        {
            var inv = TenantScopeHelper.ApplyTenantScope(_context.PartInventories.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(i => !i.IsDeleted);
            return await inv.Include(i => i.Location).GroupBy(i => i.Location.Name)
                .Select(g => new ChartPointDto { Label = g.Key ?? "Unknown", Value = g.Sum(x => x.AvailableQuantity) })
                .OrderByDescending(x => x.Value).Take(12).ToListAsync();
        }

        private async Task<List<LocationAssetMetricDto>> BuildLocationAssetMetricsAsync(ReportingFilterDto filter)
        {
            var assets = await ScopedAssets(filter).Select(a => new { a.Id, a.Location.Name }).ToListAsync();
            var issues = await FilteredIssuesQuery(filter).Select(i => new { i.AssetId }).ToListAsync();
            var groups = assets.GroupBy(a => a.Name ?? "Unknown");
            var result = new List<LocationAssetMetricDto>();
            foreach (var g in groups)
            {
                var ids = g.Select(x => assets.First(a => a.Id == x.Id).Id).ToHashSet();
                var assetIdsInLoc = assets.Where(a => a.Name == g.Key).Select(a => a.Id).ToList();
                var br = issues.Count(i => assetIdsInLoc.Contains(i.AssetId));
                var dt = 0m;
                foreach (var id in assetIdsInLoc.Take(20))
                    dt += await ComputeTotalDowntimeMinutesAsync(CloneFilterForAsset(filter, id));
                result.Add(new LocationAssetMetricDto { LocationName = g.Key, AssetCount = g.Count(), BreakdownCount = br, DowntimeMinutes = dt });
            }
            return result.OrderByDescending(x => x.AssetCount).Take(15).ToList();
        }

        private async Task<List<AssetAgeBucketDto>> BuildAssetAgeDistributionAsync(ReportingFilterDto filter)
        {
            var assets = await ScopedAssets(filter).Select(a => new { a.InstallationDate, a.PurchaseDate }).ToListAsync();
            if (!assets.Any(a => a.InstallationDate.HasValue || a.PurchaseDate.HasValue)) return new List<AssetAgeBucketDto>();

            int Bucket(DateTime? d)
            {
                if (!d.HasValue) return -1;
                var years = (DateTime.UtcNow - d.Value).TotalDays / 365.25;
                if (years < 1) return 0;
                if (years < 3) return 1;
                if (years < 5) return 2;
                if (years < 10) return 3;
                return 4;
            }
            var labels = new[] { "<1 year", "1–3 years", "3–5 years", "5–10 years", "10+ years" };
            var counts = new int[5];
            foreach (var a in assets)
            {
                var b = Bucket(a.InstallationDate ?? a.PurchaseDate);
                if (b >= 0) counts[b]++;
            }
            return labels.Select((l, i) => new AssetAgeBucketDto { Label = l, Count = counts[i] }).Where(x => x.Count > 0).ToList();
        }

        private async Task<int> CountAssetsWithOverduePmAsync(ReportingFilterDto filter)
        {
            var assetIds = ScopedAssets(filter).Select(a => a.Id);
            return await TenantScopeHelper.ApplyTenantScope(_context.PreventiveMaintenanceOccurrences.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(p => !p.IsDeleted && assetIds.Contains(p.AssetId))
                .Where(p => p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Overdue
                    || (p.DueDate.Date < DateTime.UtcNow.Date && p.Status != (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed))
                .Select(p => p.AssetId).Distinct().CountAsync();
        }

        private async Task<int> CountRepeatedFailureAssetsAsync(ReportingFilterDto filter)
        {
            return await FilteredIssuesQuery(filter).GroupBy(i => i.AssetId).Where(g => g.Count() > 1).CountAsync();
        }

        private async Task<List<PmOverdueRowDto>> BuildPmOverdueTableAsync(ReportingFilterDto filter)
        {
            var assetIds = ScopedAssets(filter).Select(a => a.Id);
            var today = DateTime.UtcNow.Date;
            var rows = await TenantScopeHelper.ApplyTenantScope(_context.PreventiveMaintenanceOccurrences.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(p => !p.IsDeleted && assetIds.Contains(p.AssetId))
                .Where(p => p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Overdue
                    || (p.DueDate.Date < today && p.Status != (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed && p.Status != (int)Enums.PreventiveMaintenanceOccurrenceStatus.Cancelled))
                .Include(p => p.Asset).Include(p => p.MaintenanceType)
                .OrderBy(p => p.DueDate).Take(50).ToListAsync();

            return rows.Select(p => new PmOverdueRowDto
            {
                OccurrenceId = p.Id,
                AssetCode = p.Asset?.AssetCode ?? "",
                AssetName = p.Asset?.Name ?? "",
                MaintenanceTypeName = p.MaintenanceType?.Name ?? "",
                ScheduledDate = p.ScheduledDate,
                DueDate = p.DueDate,
                DaysOverdue = Math.Max(0, (today - p.DueDate.Date).Days),
                Status = p.Status
            }).ToList();
        }

        private async Task<List<ChartPointDto>> BuildWorkOrderPriorityDistAsync(ReportingFilterDto filter)
        {
            var assetIds = ScopedAssets(filter).Select(a => a.Id);
            var rows = await TenantScopeHelper.ApplyTenantScope(_context.WorkOrders.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(w => !w.IsDeleted && assetIds.Contains(w.AssetId))
                .GroupBy(w => w.Priority).Select(g => new { Priority = g.Key, Count = g.Count() }).ToListAsync();
            return rows.Select(r => new ChartPointDto
            {
                Label = Enum.GetName(typeof(Enums.IssuePriority), r.Priority) ?? r.Priority.ToString(),
                Value = r.Count
            }).ToList();
        }

        private async Task<List<ChartPointDto>> BuildPmComplianceByCategoryAsync(ReportingFilterDto filter)
        {
            var assetIds = ScopedAssets(filter).Select(a => a.Id);
            var pms = await TenantScopeHelper.ApplyTenantScope(_context.PreventiveMaintenanceOccurrences.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(p => !p.IsDeleted && assetIds.Contains(p.AssetId))
                .Include(p => p.Asset).ThenInclude(a => a.AssetCategory).ToListAsync();
            if (filter.StartDate.HasValue) pms = pms.Where(p => p.DueDate >= filter.StartDate).ToList();
            if (filter.EndDate.HasValue) pms = pms.Where(p => p.DueDate <= filter.EndDate).ToList();

            return pms.GroupBy(p => p.Asset?.AssetCategory?.Name ?? "Unknown").Select(g =>
            {
                var due = g.Count(x => x.Status != (int)Enums.PreventiveMaintenanceOccurrenceStatus.Cancelled && x.Status != (int)Enums.PreventiveMaintenanceOccurrenceStatus.Upcoming);
                var onTime = g.Count(x => x.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed && x.CompletedAt <= x.DueDate);
                return new ChartPointDto
                {
                    Label = g.Key,
                    Value = due == 0 ? 0 : Math.Round((decimal)onTime / due * 100, 1)
                };
            }).OrderByDescending(x => x.Value).Take(12).ToList();
        }

        private async Task<(string Name, List<string> Labels, List<decimal> Values)> BuildWoCreatedSeriesAsync(
            IQueryable<Data.Entities.WorkOrder.WorkOrder> woQ, ReportingFilterDto filter)
        {
            var q = woQ;
            if (filter.StartDate.HasValue) q = q.Where(w => w.CreatedOn >= filter.StartDate);
            if (filter.EndDate.HasValue) q = q.Where(w => w.CreatedOn <= filter.EndDate);
            var rows = await q.GroupBy(w => new { w.CreatedOn.Year, w.CreatedOn.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month).ToListAsync();
            return ("Created", rows.Select(r => YearMonthLabel(r.Year, r.Month)).ToList(), rows.Select(r => (decimal)r.Count).ToList());
        }

        private async Task<(string Name, List<string> Labels, List<decimal> Values)> BuildWoCompletedSeriesAsync(
            IQueryable<Data.Entities.WorkOrder.WorkOrder> woQ, ReportingFilterDto filter)
        {
            var list = await woQ.Where(w => w.CompletedAt.HasValue || w.ClosedAt.HasValue)
                .Select(w => new { Done = w.CompletedAt ?? w.ClosedAt }).ToListAsync();
            var filtered = list.Where(w =>
                (!filter.StartDate.HasValue || w.Done >= filter.StartDate) &&
                (!filter.EndDate.HasValue || w.Done <= filter.EndDate)).ToList();
            var rows = filtered.GroupBy(w => new { w.Done!.Value.Year, w.Done.Value.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month).ToList();
            return ("Completed", rows.Select(r => YearMonthLabel(r.Year, r.Month)).ToList(), rows.Select(r => (decimal)r.Count).ToList());
        }

        private static List<decimal> AlignSeries(List<string> masterLabels, List<string> seriesLabels, List<decimal> values)
        {
            var map = seriesLabels.Select((l, i) => (l, values.ElementAtOrDefault(i))).ToDictionary(x => x.l, x => x.Item2);
            return masterLabels.Select(l => map.TryGetValue(l, out var v) ? v : 0).ToList();
        }
    }
}
