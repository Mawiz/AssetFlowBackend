using AssetFlow.Common.Enum;
using AssetFlow.Common.Helper;
using AssetFlow.Data.Data;
using AssetFlow.Data.Provider;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Reporting;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AssetFlow.Services.Core.Reporting
{
    public partial class ReportingService : IReportingService
    {
        private static string YearMonthLabel(int year, int month) => $"{year}-{month:D2}";

        private static readonly int[] OpenWorkOrderStatuses =
        {
            (int)Enums.WorkOrderStatus.New,
            (int)Enums.WorkOrderStatus.Submitted,
            (int)Enums.WorkOrderStatus.PendingAssignment,
            (int)Enums.WorkOrderStatus.Assigned,
            (int)Enums.WorkOrderStatus.Accepted,
            (int)Enums.WorkOrderStatus.InProgress,
            (int)Enums.WorkOrderStatus.WaitingForParts,
            (int)Enums.WorkOrderStatus.WaitingForApproval,
            (int)Enums.WorkOrderStatus.Reopened
        };

        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public ReportingService(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<ResponseDto<DashboardSummaryDto>> GetDashboardSummaryAsync(ReportingFilterDto filter)
        {
            var response = new ResponseDto<DashboardSummaryDto>();
            if (!await ValidateFilterAsync(filter, response)) return response;

            var (start, end) = ReportingPeriodHelper.Resolve(filter.PeriodPreset, filter.StartDate, filter.EndDate);
            filter.StartDate = start;
            filter.EndDate = end;

            var assetSummary = await BuildAssetSummaryAsync(filter, currentStateOnly: true);
            var maintenance = await BuildMaintenanceSummaryAsync(filter);
            var performance = await BuildPerformanceSummaryAsync(filter);
            var spare = await BuildSparePartSummaryAsync(filter);
            var trend = await BuildBreakdownTrendAsync(filter);
            var topFailing = await BuildTopFailingAssetsAsync(filter, 5);

            response.Result = new DashboardSummaryDto
            {
                Asset = assetSummary,
                Maintenance = maintenance,
                Performance = performance,
                SpareParts = spare,
                BreakdownTrend = trend,
                TopFailingAssets = topFailing,
                PeriodStart = start,
                PeriodEnd = end
            };
            return response;
        }

        public async Task<ResponseDto<AssetReportBundleDto>> GetAssetReportAsync(ReportingFilterDto filter)
        {
            var response = new ResponseDto<AssetReportBundleDto>();
            if (!await ValidateFilterAsync(filter, response)) return response;
            ResolvePeriod(filter);

            var summary = await BuildAssetSummaryAsync(filter, currentStateOnly: true);
            var byType = await AssetGroupChartAsync(filter, groupByType: true);
            var reliability = await BuildAssetReliabilityAsync(filter);
            var topFailing = await BuildTopFailingAssetsAsync(filter, filter.PageSize > 0 ? filter.PageSize : 20);
            var highestCost = await BuildCostByAssetAsync(filter);

            response.Result = new AssetReportBundleDto
            {
                Summary = summary,
                ByType = byType,
                Reliability = reliability,
                TopFailing = topFailing,
                HighestCost = highestCost.Take(20).ToList()
            };
            return response;
        }

        public async Task<ResponseDto<MaintenanceReportBundleDto>> GetMaintenanceReportAsync(ReportingFilterDto filter)
        {
            var response = new ResponseDto<MaintenanceReportBundleDto>();
            if (!await ValidateFilterAsync(filter, response)) return response;
            ResolvePeriod(filter);

            var summary = await BuildMaintenanceSummaryAsync(filter);
            var pm = await BuildPmReportAsync(filter);
            var totalCost = await SumMaintenanceCostAsync(filter);
            var activity = await BuildMaintenanceActivityTrendAsync(filter);

            response.Result = new MaintenanceReportBundleDto
            {
                Summary = summary,
                PreventiveMaintenance = pm,
                TotalMaintenanceCost = totalCost,
                ActivityTrend = activity
            };
            return response;
        }

        public async Task<ResponseDto<BreakdownReportBundleDto>> GetBreakdownReportAsync(ReportingFilterDto filter)
        {
            var response = new ResponseDto<BreakdownReportBundleDto>();
            if (!await ValidateFilterAsync(filter, response)) return response;
            ResolvePeriod(filter);

            var issues = FilteredIssuesQuery(filter);
            var list = await issues.AsNoTracking().ToListAsync();
            var total = list.Count;
            var trend = await BuildBreakdownTrendAsync(filter);

            var byCat = list.GroupBy(i => i.Asset?.AssetCategory?.Name ?? "Unknown")
                .Select(g => new ChartPointDto { Label = g.Key, Value = g.Count() }).OrderByDescending(x => x.Value).Take(15).ToList();
            var byLoc = list.GroupBy(i => i.Location?.Name ?? "Unknown")
                .Select(g => new ChartPointDto { Label = g.Key, Value = g.Count() }).OrderByDescending(x => x.Value).Take(15).ToList();
            var byIssueCat = list.GroupBy(i => i.IssueCategory?.Name ?? "Unknown")
                .Select(g => new ChartPointDto { Label = g.Key, Value = g.Count() }).OrderByDescending(x => x.Value).ToList();
            var byPri = list.GroupBy(i => ((Enums.IssuePriority)i.Priority).ToString())
                .Select(g => new ChartPointDto { Label = g.Key, Value = g.Count() }).ToList();

            var top = await BuildTopFailingAssetsAsync(filter, 20);
            var repeated = top.Where(x => x.BreakdownCount > 1).Select(x => new RepeatedFailureDto
            {
                AssetId = x.AssetId,
                AssetCode = x.AssetCode,
                AssetName = x.AssetName,
                BreakdownCount = x.BreakdownCount
            }).ToList();

            var downtime = await ComputeTotalDowntimeMinutesAsync(filter);

            response.Result = new BreakdownReportBundleDto
            {
                TotalBreakdowns = total,
                Trend = trend,
                ByCategory = byCat,
                ByLocation = byLoc,
                ByIssueCategory = byIssueCat,
                ByPriority = byPri,
                TopFailingAssets = top,
                RepeatedFailures = repeated,
                TotalDowntimeMinutes = downtime
            };
            return response;
        }

        public async Task<ResponseDto<SparePartsReportBundleDto>> GetSparePartsReportAsync(ReportingFilterDto filter)
        {
            var response = new ResponseDto<SparePartsReportBundleDto>();
            if (!await ValidateFilterAsync(filter, response)) return response;
            ResolvePeriod(filter);

            var summary = await BuildSparePartSummaryAsync(filter);
            var stock = await BuildPartStockReportAsync(filter);
            var tx = await BuildPartTransactionSummaryAsync(filter);
            var replaced = await BuildTopReplacedPartsAsync(filter);
            var life = await BuildPartLifeRowsAsync(filter);
            var bySupplier = await PartGroupChartAsync(filter, bySupplier: true);
            var byMfg = await PartGroupChartAsync(filter, bySupplier: false);

            response.Result = new SparePartsReportBundleDto
            {
                Summary = summary,
                Stock = stock,
                Transactions = tx,
                MostReplaced = replaced,
                PartLife = life,
                BySupplier = bySupplier,
                ByManufacturer = byMfg
            };
            return response;
        }

        public async Task<ResponseDto<PerformanceReportBundleDto>> GetPerformanceReportAsync(ReportingFilterDto filter)
        {
            var response = new ResponseDto<PerformanceReportBundleDto>();
            if (!await ValidateFilterAsync(filter, response)) return response;
            ResolvePeriod(filter);

            var summary = await BuildPerformanceSummaryAsync(filter);
            var engineers = await BuildEngineerPerformanceAsync(filter);

            response.Result = new PerformanceReportBundleDto
            {
                Summary = summary,
                Engineers = engineers
            };
            return response;
        }

        public async Task<ResponseDto<CostReportBundleDto>> GetCostReportAsync(ReportingFilterDto filter)
        {
            var response = new ResponseDto<CostReportBundleDto>();
            if (!await ValidateFilterAsync(filter, response)) return response;
            ResolvePeriod(filter);

            var costs = FilteredCostQuery(filter);
            var grouped = await costs.GroupBy(c => c.CostType)
                .Select(g => new { Type = g.Key, Sum = g.Sum(x => x.Amount) }).ToListAsync();

            decimal labor = grouped.Where(g => g.Type == (int)Enums.MaintenanceCostType.Labor).Sum(g => g.Sum);
            decimal parts = grouped.Where(g => g.Type == (int)Enums.MaintenanceCostType.Parts).Sum(g => g.Sum);
            decimal ext = grouped.Where(g => g.Type == (int)Enums.MaintenanceCostType.ExternalService).Sum(g => g.Sum);
            decimal other = grouped.Where(g => g.Type == (int)Enums.MaintenanceCostType.Other).Sum(g => g.Sum);

            var byMonthRaw = await costs.GroupBy(c => new { c.CostDate.Year, c.CostDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Sum = g.Sum(x => x.Amount) })
                .OrderBy(x => x.Year).ThenBy(x => x.Month).ToListAsync();

            var byAsset = await BuildCostByAssetAsync(filter);

            response.Result = new CostReportBundleDto
            {
                TotalLabor = labor,
                TotalParts = parts,
                TotalExternal = ext,
                TotalOther = other,
                GrandTotal = labor + parts + ext + other,
                ByMonth = byMonthRaw.Select(m => new ChartPointDto
                {
                    Label = YearMonthLabel(m.Year, m.Month),
                    Value = m.Sum
                }).ToList(),
                ByAsset = byAsset
            };
            return response;
        }

        private void ResolvePeriod(ReportingFilterDto filter)
        {
            var (start, end) = ReportingPeriodHelper.Resolve(filter.PeriodPreset, filter.StartDate, filter.EndDate);
            filter.StartDate = start;
            filter.EndDate = end;
        }

        private async Task<bool> ValidateFilterAsync<T>(ReportingFilterDto filter, ResponseDto<T> response)
        {
            if (filter.AssetId.HasValue)
            {
                var asset = await _context.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Id == filter.AssetId && !a.IsDeleted);
                if (asset == null) { response.AddError("Asset not found."); response.StatusCode = HttpStatusCode.BadRequest; return false; }
                if (!EnsureAccess(asset.TenantId, response)) return false;
            }
            if (filter.LocationId.HasValue)
            {
                var loc = await _context.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == filter.LocationId && !l.IsDeleted);
                if (loc == null) { response.AddError("Location not found."); response.StatusCode = HttpStatusCode.BadRequest; return false; }
                if (!EnsureAccess(loc.TenantId, response)) return false;
            }
            if (filter.AssetCategoryId.HasValue)
            {
                var cat = await _context.AssetCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == filter.AssetCategoryId && !c.IsDeleted);
                if (cat == null) { response.AddError("Asset category not found."); response.StatusCode = HttpStatusCode.BadRequest; return false; }
                if (!EnsureAccess(cat.TenantId, response)) return false;
            }
            if (filter.EngineerUserId.HasValue)
            {
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == filter.EngineerUserId);
                if (user == null) { response.AddError("User not found."); response.StatusCode = HttpStatusCode.BadRequest; return false; }
            }
            return true;
        }

        private bool EnsureAccess<T>(int? entityTenantId, ResponseDto<T> response)
        {
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entityTenantId);
            if (!access.Ok)
            {
                response.AddError(access.Error);
                response.StatusCode = HttpStatusCode.Forbidden;
                return false;
            }
            return true;
        }

        private IQueryable<Data.Entities.Asset.Asset> ScopedAssets(ReportingFilterDto filter)
        {
            var q = TenantScopeHelper.ApplyTenantScope(_context.Assets.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(a => !a.IsDeleted);
            if (filter.AssetId.HasValue) q = q.Where(a => a.Id == filter.AssetId);
            if (filter.AssetCategoryId.HasValue) q = q.Where(a => a.AssetCategoryId == filter.AssetCategoryId);
            if (filter.AssetTypeId.HasValue) q = q.Where(a => a.AssetTypeId == filter.AssetTypeId);
            if (filter.LocationId.HasValue) q = q.Where(a => a.LocationId == filter.LocationId);
            return q;
        }

        private IQueryable<Data.Entities.Issue.AssetIssue> FilteredIssuesQuery(ReportingFilterDto filter)
        {
            var assetIds = ScopedAssets(filter).Select(a => a.Id);
            var q = TenantScopeHelper.ApplyTenantScope(_context.AssetIssues.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(i => !i.IsDeleted && assetIds.Contains(i.AssetId));
            if (filter.IssueCategoryId.HasValue) q = q.Where(i => i.IssueCategoryId == filter.IssueCategoryId);
            if (filter.Priority.HasValue) q = q.Where(i => i.Priority == filter.Priority);
            if (filter.StartDate.HasValue) q = q.Where(i => i.ReportedAt >= filter.StartDate);
            if (filter.EndDate.HasValue) q = q.Where(i => i.ReportedAt <= filter.EndDate);
            return q.Include(i => i.Asset).ThenInclude(a => a.AssetCategory).Include(i => i.Location).Include(i => i.IssueCategory);
        }

        private IQueryable<Data.Entities.History.MaintenanceCostRecord> FilteredCostQuery(ReportingFilterDto filter)
        {
            var assetIds = ScopedAssets(filter).Select(a => a.Id);
            var q = TenantScopeHelper.ApplyTenantScope(_context.MaintenanceCostRecords.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(c => !c.IsDeleted && assetIds.Contains(c.AssetId));
            if (filter.StartDate.HasValue) q = q.Where(c => c.CostDate >= filter.StartDate);
            if (filter.EndDate.HasValue) q = q.Where(c => c.CostDate <= filter.EndDate);
            return q;
        }

        private async Task<AssetSummaryDto> BuildAssetSummaryAsync(ReportingFilterDto filter, bool currentStateOnly)
        {
            var q = ScopedAssets(filter);
            var statusGroups = await q.GroupBy(a => a.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync();
            var critGroups = await q.GroupBy(a => a.Criticality).Select(g => new { Criticality = g.Key, Count = g.Count() }).ToListAsync();
            var byCat = await q.GroupBy(a => a.AssetCategory.Name).Select(g => new ChartPointDto { Label = g.Key ?? "Unknown", Value = g.Count() }).ToListAsync();
            var byLoc = await q.GroupBy(a => a.Location.Name).Select(g => new ChartPointDto { Label = g.Key ?? "Unknown", Value = g.Count() }).OrderByDescending(x => x.Value).Take(12).ToListAsync();

            int Total(int status) => statusGroups.FirstOrDefault(x => x.Status == status)?.Count ?? 0;

            return new AssetSummaryDto
            {
                TotalAssets = statusGroups.Sum(x => x.Count),
                Operational = Total((int)Enums.AssetStatus.Operational),
                UnderMaintenance = Total((int)Enums.AssetStatus.UnderMaintenance),
                Breakdown = Total((int)Enums.AssetStatus.Breakdown),
                Retired = Total((int)Enums.AssetStatus.Retired),
                Critical = critGroups.FirstOrDefault(x => x.Criticality == (int)Enums.AssetCriticality.Critical)?.Count ?? 0,
                StatusDistribution = statusGroups.Select(s => new ChartPointDto
                {
                    Label = Enum.GetName(typeof(Enums.AssetStatus), s.Status) ?? s.Status.ToString(),
                    Value = s.Count
                }).ToList(),
                CriticalityDistribution = critGroups.Select(c => new ChartPointDto
                {
                    Label = Enum.GetName(typeof(Enums.AssetCriticality), c.Criticality) ?? c.Criticality.ToString(),
                    Value = c.Count
                }).ToList(),
                ByCategory = byCat.OrderByDescending(x => x.Value).Take(12).ToList(),
                ByLocation = byLoc
            };
        }

        private async Task<List<ChartPointDto>> AssetGroupChartAsync(ReportingFilterDto filter, bool groupByType)
        {
            var q = ScopedAssets(filter);
            if (groupByType)
                return await q.GroupBy(a => a.AssetType.Name).Select(g => new ChartPointDto { Label = g.Key ?? "Unknown", Value = g.Count() }).ToListAsync();
            return new List<ChartPointDto>();
        }

        private async Task<MaintenanceSummaryDto> BuildMaintenanceSummaryAsync(ReportingFilterDto filter)
        {
            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = todayStart.AddDays(1).AddTicks(-1);
            var assetIds = ScopedAssets(filter).Select(a => a.Id);

            var openIssues = await TenantScopeHelper.ApplyTenantScope(_context.AssetIssues.AsQueryable(), _tenantProvider, filter.TenantId)
                .CountAsync(i => !i.IsDeleted && assetIds.Contains(i.AssetId)
                    && i.Status != (int)Enums.IssueStatus.Resolved && i.Status != (int)Enums.IssueStatus.Cancelled);

            var woQ = TenantScopeHelper.ApplyTenantScope(_context.WorkOrders.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(w => !w.IsDeleted && assetIds.Contains(w.AssetId));
            var openWo = await woQ.CountAsync(w => OpenWorkOrderStatuses.Contains(w.Status));
            var waitingParts = await woQ.CountAsync(w => w.Status == (int)Enums.WorkOrderStatus.WaitingForParts);

            var pmQ = TenantScopeHelper.ApplyTenantScope(_context.PreventiveMaintenanceOccurrences.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(p => !p.IsDeleted && assetIds.Contains(p.AssetId));

            var todaysPm = await pmQ.CountAsync(p =>
                p.Status != (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed
                && p.Status != (int)Enums.PreventiveMaintenanceOccurrenceStatus.Cancelled
                && ((p.ScheduledDate ?? p.DueDate).Date == todayStart || p.DueDate.Date == todayStart));

            var overduePm = await pmQ.CountAsync(p =>
                p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Overdue
                || (p.DueDate.Date < todayStart
                    && p.Status != (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed
                    && p.Status != (int)Enums.PreventiveMaintenanceOccurrenceStatus.Cancelled));

            var completedToday = await pmQ.CountAsync(p =>
                p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed
                && p.CompletedAt >= todayStart && p.CompletedAt <= todayEnd);

            var pmCompliance = await ComputePmComplianceAsync(filter);

            var woStatusDist = (await woQ.GroupBy(w => w.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync())
                .Select(g => new ChartPointDto
                {
                    Label = Enum.GetName(typeof(Enums.WorkOrderStatus), g.Status) ?? g.Status.ToString(),
                    Value = g.Count
                }).ToList();

            var pmStatusDist = (await pmQ.GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync())
                .Select(g => new ChartPointDto
                {
                    Label = Enum.GetName(typeof(Enums.PreventiveMaintenanceOccurrenceStatus), g.Status) ?? g.Status.ToString(),
                    Value = g.Count
                }).ToList();

            var costTrendRaw = await FilteredCostQuery(filter).GroupBy(c => new { c.CostDate.Year, c.CostDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Sum = g.Sum(x => x.Amount) })
                .OrderBy(x => x.Year).ThenBy(x => x.Month).Take(12).ToListAsync();
            var costTrend = costTrendRaw.Select(g => new ChartPointDto
            {
                Label = YearMonthLabel(g.Year, g.Month),
                Value = g.Sum
            }).ToList();

            return new MaintenanceSummaryDto
            {
                OpenIssues = openIssues,
                OpenWorkOrders = openWo,
                TodaysMaintenance = todaysPm,
                OverdueMaintenance = overduePm,
                CompletedToday = completedToday,
                WaitingForParts = waitingParts,
                PmCompliancePercent = pmCompliance,
                WorkOrderStatusDistribution = woStatusDist,
                PmStatusDistribution = pmStatusDist,
                MaintenanceCostTrend = costTrend
            };
        }

        private async Task<decimal?> ComputePmComplianceAsync(ReportingFilterDto filter)
        {
            var assetIds = ScopedAssets(filter).Select(a => a.Id);
            var pmQ = TenantScopeHelper.ApplyTenantScope(_context.PreventiveMaintenanceOccurrences.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(p => !p.IsDeleted && assetIds.Contains(p.AssetId));

            if (filter.StartDate.HasValue) pmQ = pmQ.Where(p => p.DueDate >= filter.StartDate);
            if (filter.EndDate.HasValue) pmQ = pmQ.Where(p => p.DueDate <= filter.EndDate);

            var eligible = await pmQ.Where(p =>
                p.Status != (int)Enums.PreventiveMaintenanceOccurrenceStatus.Cancelled
                && p.Status != (int)Enums.PreventiveMaintenanceOccurrenceStatus.Upcoming).ToListAsync();

            if (eligible.Count == 0) return null;

            var onTime = eligible.Count(p =>
                p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed
                && p.CompletedAt.HasValue && p.CompletedAt.Value <= p.DueDate);

            var dueCount = eligible.Count;
            return dueCount == 0 ? null : Math.Round((decimal)onTime / dueCount * 100, 1);
        }

        private async Task<PerformanceSummaryDto> BuildPerformanceSummaryAsync(ReportingFilterDto filter)
        {
            var pmCompliance = await ComputePmComplianceAsync(filter);
            var downtime = await ComputeTotalDowntimeMinutesAsync(filter);
            var mttr = await ComputeMttrHoursAsync(filter);
            var mtbf = await ComputeMtbfHoursAsync(filter);
            var (ftfr, _) = await ComputeFirstTimeFixRateAsync(filter);
            var responseTime = await ComputeAvgResponseMinutesAsync(filter);

            return new PerformanceSummaryDto
            {
                MtbfHours = mtbf,
                MttrHours = mttr,
                TotalDowntimeMinutes = downtime,
                PmCompliancePercent = pmCompliance,
                FirstTimeFixRatePercent = ftfr,
                AvgResponseTimeMinutes = responseTime
            };
        }

        private async Task<SparePartSummaryDto> BuildSparePartSummaryAsync(ReportingFilterDto filter)
        {
            var partsQ = TenantScopeHelper.ApplyTenantScope(_context.Parts.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(p => !p.IsDeleted);
            if (filter.PartId.HasValue) partsQ = partsQ.Where(p => p.Id == filter.PartId);

            var totalParts = await partsQ.CountAsync();

            var invQ = TenantScopeHelper.ApplyTenantScope(_context.PartInventories.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(i => !i.IsDeleted);
            var stockByPart = await invQ.GroupBy(i => i.PartId)
                .Select(g => new { PartId = g.Key, Qty = g.Sum(x => x.AvailableQuantity) }).ToListAsync();
            var minLevels = await partsQ.Select(p => new { p.Id, p.MinStockLevel }).ToListAsync();
            var lowStock = minLevels.Count(p =>
            {
                var qty = stockByPart.FirstOrDefault(s => s.PartId == p.Id)?.Qty ?? 0;
                return qty < p.MinStockLevel;
            });

            var txQ = TenantScopeHelper.ApplyTenantScope(_context.PartTransactions.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(t => !t.IsDeleted);
            if (filter.StartDate.HasValue) txQ = txQ.Where(t => t.TransactionDate >= filter.StartDate);
            if (filter.EndDate.HasValue) txQ = txQ.Where(t => t.TransactionDate <= filter.EndDate);
            if (filter.PartId.HasValue) txQ = txQ.Where(t => t.PartId == filter.PartId);

            var consumed = await txQ.Where(t => t.TransactionType == (int)Enums.PartTransactionType.Issue)
                .SumAsync(t => (decimal?)t.Quantity) ?? 0;

            var replQ = TenantScopeHelper.ApplyTenantScope(_context.PartReplacements.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(r => !r.IsDeleted);
            if (filter.StartDate.HasValue) replQ = replQ.Where(r => r.InstalledAt >= filter.StartDate);
            if (filter.EndDate.HasValue) replQ = replQ.Where(r => r.InstalledAt <= filter.EndDate);

            var replaced = await replQ.CountAsync();

            return new SparePartSummaryDto
            {
                TotalParts = totalParts,
                LowStockParts = lowStock,
                PartsConsumedInPeriod = consumed,
                PartsReplacedInPeriod = replaced
            };
        }

        private async Task<List<ChartPointDto>> BuildBreakdownTrendAsync(ReportingFilterDto filter)
        {
            var issues = FilteredIssuesQuery(filter);
            var rows = await issues.GroupBy(i => new { i.ReportedAt.Year, i.ReportedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();
            return rows.Select(r => new ChartPointDto
            {
                Label = YearMonthLabel(r.Year, r.Month),    
                Value = r.Count
            }).ToList();
        }

        private async Task<List<TopFailingAssetDto>> BuildTopFailingAssetsAsync(ReportingFilterDto filter, int take)
        {
            var issues = FilteredIssuesQuery(filter);
            var grouped = await issues.GroupBy(i => i.AssetId)
                .Select(g => new
                {
                    AssetId = g.Key,
                    Count = g.Count(),
                    Last = g.Max(x => x.ReportedAt),
                    Open = g.Count(x => x.Status != (int)Enums.IssueStatus.Resolved && x.Status != (int)Enums.IssueStatus.Cancelled),
                    Closed = g.Count(x => x.Status == (int)Enums.IssueStatus.Resolved)
                }).OrderByDescending(x => x.Count).Take(take).ToListAsync();

            var assetIds = grouped.Select(g => g.AssetId).ToList();
            var assets = await _context.Assets.AsNoTracking()
                .Include(a => a.AssetCategory).Include(a => a.Location)
                .Where(a => assetIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id);

            var costByAsset = await FilteredCostQuery(filter).Where(c => assetIds.Contains(c.AssetId))
                .GroupBy(c => c.AssetId).Select(g => new { AssetId = g.Key, Sum = g.Sum(x => x.Amount) }).ToListAsync();

            var result = new List<TopFailingAssetDto>();
            foreach (var g in grouped)
            {
                assets.TryGetValue(g.AssetId, out var asset);
                var dtFilter = CloneFilterForAsset(filter, g.AssetId);
                var downtime = await ComputeTotalDowntimeMinutesAsync(dtFilter);
                result.Add(new TopFailingAssetDto
                {
                    AssetId = g.AssetId,
                    AssetCode = asset?.AssetCode ?? "",
                    AssetName = asset?.Name ?? "",
                    CategoryName = asset?.AssetCategory?.Name ?? "",
                    LocationName = asset?.Location?.Name ?? "",
                    BreakdownCount = g.Count,
                    OpenIssues = g.Open,
                    ClosedIssues = g.Closed,
                    LastFailureDate = g.Last,
                    MaintenanceCost = costByAsset.FirstOrDefault(c => c.AssetId == g.AssetId)?.Sum ?? 0,
                    TotalDowntimeMinutes = downtime
                });
            }
            return result;
        }

        private static ReportingFilterDto CloneFilterForAsset(ReportingFilterDto filter, int assetId)
        {
            return new ReportingFilterDto
            {
                TenantId = filter.TenantId,
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                AssetId = assetId
            };
        }

        private async Task<List<AssetReliabilityRowDto>> BuildAssetReliabilityAsync(ReportingFilterDto filter)
        {
            var top = await BuildTopFailingAssetsAsync(filter, filter.PageSize > 0 ? filter.PageSize : 50);
            var rows = new List<AssetReliabilityRowDto>();
            foreach (var t in top)
            {
                var f = CloneFilterForAsset(filter, t.AssetId);
                var mtbf = await ComputeMtbfHoursForAssetAsync(t.AssetId, f);
                var mttr = await ComputeMttrHoursForAssetAsync(t.AssetId, f);
                var repl = await TenantScopeHelper.ApplyTenantScope(_context.PartReplacements.AsQueryable(), _tenantProvider, filter.TenantId)
                    .CountAsync(r => !r.IsDeleted && r.AssetId == t.AssetId);
                var lastPm = await TenantScopeHelper.ApplyTenantScope(_context.PreventiveMaintenanceOccurrences.AsQueryable(), _tenantProvider, filter.TenantId)
                    .Where(p => p.AssetId == t.AssetId && p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed)
                    .OrderByDescending(p => p.CompletedAt).Select(p => p.CompletedAt).FirstOrDefaultAsync();

                rows.Add(new AssetReliabilityRowDto
                {
                    AssetId = t.AssetId,
                    AssetCode = t.AssetCode,
                    AssetName = t.AssetName,
                    CategoryName = t.CategoryName,
                    Breakdowns = t.BreakdownCount,
                    MtbfHours = mtbf,
                    MttrHours = mttr,
                    DowntimeMinutes = t.TotalDowntimeMinutes,
                    MaintenanceCost = t.MaintenanceCost,
                    PartsReplaced = repl,
                    LastBreakdown = t.LastFailureDate,
                    LastMaintenance = lastPm
                });
            }
            return rows;
        }

        private async Task<List<AssetCostReportRowDto>> BuildCostByAssetAsync(ReportingFilterDto filter)
        {
            var costs = await FilteredCostQuery(filter)
                .GroupBy(c => new { c.AssetId, c.CostType })
                .Select(g => new { g.Key.AssetId, g.Key.CostType, Sum = g.Sum(x => x.Amount) }).ToListAsync();

            var assetIds = costs.Select(c => c.AssetId).Distinct().ToList();
            var assets = await _context.Assets.AsNoTracking().Include(a => a.AssetCategory)
                .Where(a => assetIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id);

            var woCounts = await TenantScopeHelper.ApplyTenantScope(_context.WorkOrders.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(w => !w.IsDeleted && assetIds.Contains(w.AssetId))
                .GroupBy(w => w.AssetId).Select(g => new { AssetId = g.Key, Count = g.Count() }).ToListAsync();

            return assetIds.Select(id =>
            {
                assets.TryGetValue(id, out var asset);
                decimal SumType(int type) => costs.Where(c => c.AssetId == id && c.CostType == type).Sum(c => c.Sum);
                var labor = SumType((int)Enums.MaintenanceCostType.Labor);
                var parts = SumType((int)Enums.MaintenanceCostType.Parts);
                var ext = SumType((int)Enums.MaintenanceCostType.ExternalService);
                var other = SumType((int)Enums.MaintenanceCostType.Other);
                return new AssetCostReportRowDto
                {
                    AssetId = id,
                    AssetCode = asset?.AssetCode ?? "",
                    AssetName = asset?.Name ?? "",
                    CategoryName = asset?.AssetCategory?.Name ?? "",
                    WorkOrderCount = woCounts.FirstOrDefault(w => w.AssetId == id)?.Count ?? 0,
                    LaborCost = labor,
                    PartsCost = parts,
                    ExternalServiceCost = ext,
                    OtherCost = other,
                    TotalMaintenanceCost = labor + parts + ext + other
                };
            }).OrderByDescending(x => x.TotalMaintenanceCost).ToList();
        }

        private async Task<PmReportSummaryDto> BuildPmReportAsync(ReportingFilterDto filter)
        {
            var assetIds = ScopedAssets(filter).Select(a => a.Id);
            var pmQ = TenantScopeHelper.ApplyTenantScope(_context.PreventiveMaintenanceOccurrences.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(p => !p.IsDeleted && assetIds.Contains(p.AssetId));
            if (filter.StartDate.HasValue) pmQ = pmQ.Where(p => p.DueDate >= filter.StartDate);
            if (filter.EndDate.HasValue) pmQ = pmQ.Where(p => p.DueDate <= filter.EndDate);

            var all = await pmQ.ToListAsync();
            var compliance = await ComputePmComplianceAsync(filter);

            var trend = all.Where(p => p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed && p.CompletedAt.HasValue)
                .GroupBy(p => new { p.CompletedAt!.Value.Year, p.CompletedAt.Value.Month })
                .Select(g => new ChartPointDto { Label = $"{g.Key.Year}-{g.Key.Month:D2}", Value = g.Count() })
                .OrderBy(x => x.Label).ToList();

            var complianceByMonth = all.Where(p => p.Status != (int)Enums.PreventiveMaintenanceOccurrenceStatus.Cancelled
                && p.Status != (int)Enums.PreventiveMaintenanceOccurrenceStatus.Upcoming)
                .GroupBy(p => new { p.DueDate.Year, p.DueDate.Month })
                .Select(g =>
                {
                    var due = g.Count();
                    var onTime = g.Count(x => x.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed
                        && x.CompletedAt.HasValue && x.CompletedAt <= x.DueDate);
                    return new ChartPointDto
                    {
                        Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Value = due == 0 ? 0 : Math.Round((decimal)onTime / due * 100, 1)
                    };
                }).OrderBy(x => x.Label).ToList();

            return new PmReportSummaryDto
            {
                TotalScheduled = all.Count,
                Completed = all.Count(p => p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed),
                Upcoming = all.Count(p => p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Upcoming),
                Due = all.Count(p => p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Due),
                Overdue = all.Count(p => p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Overdue),
                Cancelled = all.Count(p => p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Cancelled),
                CompliancePercent = compliance,
                CompletionTrend = trend,
                ComplianceByMonth = complianceByMonth
            };
        }

        private async Task<decimal> SumMaintenanceCostAsync(ReportingFilterDto filter)
        {
            return await FilteredCostQuery(filter).SumAsync(c => (decimal?)c.Amount) ?? 0;
        }

        private async Task<List<ChartPointDto>> BuildMaintenanceActivityTrendAsync(ReportingFilterDto filter)
        {
            var woQ = TenantScopeHelper.ApplyTenantScope(_context.WorkOrders.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(w => !w.IsDeleted);
            var assetIds = ScopedAssets(filter).Select(a => a.Id);
            woQ = woQ.Where(w => assetIds.Contains(w.AssetId));
            if (filter.StartDate.HasValue) woQ = woQ.Where(w => w.CreatedOn >= filter.StartDate);
            if (filter.EndDate.HasValue) woQ = woQ.Where(w => w.CreatedOn <= filter.EndDate);

            var rows = await woQ.GroupBy(w => new { w.CreatedOn.Year, w.CreatedOn.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();
            return rows.Select(r => new ChartPointDto
            {
                Label = YearMonthLabel(r.Year, r.Month),
                Value = r.Count
            }).ToList();
        }

        private async Task<List<TopReplacedPartDto>> BuildTopReplacedPartsAsync(ReportingFilterDto filter)
        {
            var q = TenantScopeHelper.ApplyTenantScope(_context.PartReplacements.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(r => !r.IsDeleted);
            if (filter.StartDate.HasValue) q = q.Where(r => r.InstalledAt >= filter.StartDate);
            if (filter.EndDate.HasValue) q = q.Where(r => r.InstalledAt <= filter.EndDate);

            var list = await q.GroupBy(r => r.NewPartId)
                .Select(g => new TopReplacedPartDto
                {
                    PartId = g.Key,
                    ReplacementCount = g.Count(),
                    LastReplacementDate = g.Max(x => x.InstalledAt)
                }).OrderByDescending(x => x.ReplacementCount).Take(25).ToListAsync();

            var partIds = list.Select(x => x.PartId).ToList();
            var parts = await _context.Parts.AsNoTracking().Where(p => partIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
            foreach (var row in list)
            {
                if (parts.TryGetValue(row.PartId, out var p))
                {
                    row.PartNumber = p.PartNumber;
                    row.PartName = p.PartName;
                    row.Manufacturer = p.Manufacturer;
                }
            }
            return list;
        }

        private async Task<List<PartStockReportRowDto>> BuildPartStockReportAsync(ReportingFilterDto filter)
        {
            var partsQ = TenantScopeHelper.ApplyTenantScope(_context.Parts.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(p => !p.IsDeleted);
            if (filter.PartId.HasValue) partsQ = partsQ.Where(p => p.Id == filter.PartId);

            var parts = await partsQ.Include(p => p.PartCategory).Take(100).ToListAsync();
            var partIds = parts.Select(p => p.Id).ToList();
            var inv = await TenantScopeHelper.ApplyTenantScope(_context.PartInventories.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(i => !i.IsDeleted && partIds.Contains(i.PartId))
                .Include(i => i.Location).ToListAsync();

            return parts.Select(p =>
            {
                var rows = inv.Where(i => i.PartId == p.Id).ToList();
                var qty = rows.Sum(i => i.AvailableQuantity);
                return new PartStockReportRowDto
                {
                    PartId = p.Id,
                    PartNumber = p.PartNumber,
                    PartName = p.PartName,
                    CategoryName = p.PartCategory?.Name ?? "",
                    TotalQuantity = qty,
                    Locations = string.Join(", ", rows.Select(r => r.Location?.Name).Where(n => n != null).Distinct()),
                    Manufacturer = p.Manufacturer,
                    IsLowStock = qty < p.MinStockLevel
                };
            }).ToList();
        }

        private async Task<PartTransactionSummaryDto> BuildPartTransactionSummaryAsync(ReportingFilterDto filter)
        {
            var txQ = TenantScopeHelper.ApplyTenantScope(_context.PartTransactions.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(t => !t.IsDeleted);
            if (filter.StartDate.HasValue) txQ = txQ.Where(t => t.TransactionDate >= filter.StartDate);
            if (filter.EndDate.HasValue) txQ = txQ.Where(t => t.TransactionDate <= filter.EndDate);
            if (filter.PartId.HasValue) txQ = txQ.Where(t => t.PartId == filter.PartId);

            var grouped = await txQ.GroupBy(t => t.TransactionType).Select(g => new { Type = g.Key, Sum = g.Sum(x => x.Quantity) }).ToListAsync();
            decimal Sum(int type) => grouped.FirstOrDefault(g => g.Type == type)?.Sum ?? 0;

            return new PartTransactionSummaryDto
            {
                QuantityReceived = Sum((int)Enums.PartTransactionType.Receipt),
                QuantityIssued = Sum((int)Enums.PartTransactionType.Issue),
                QuantityReturned = Sum((int)Enums.PartTransactionType.Return),
                QuantityAdjusted = Sum((int)Enums.PartTransactionType.Adjustment),
                QuantityTransferred = Sum((int)Enums.PartTransactionType.Transfer),
                QuantityFaulty = Sum((int)Enums.PartTransactionType.MarkFaulty),
                QuantityReturnedToSupplier = Sum((int)Enums.PartTransactionType.ReturnToSupplier),
                QuantityScrapped = Sum((int)Enums.PartTransactionType.Scrap)
            };
        }

        private async Task<List<PartLifeRowDto>> BuildPartLifeRowsAsync(ReportingFilterDto filter)
        {
            var q = TenantScopeHelper.ApplyTenantScope(_context.PartReplacements.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(r => !r.IsDeleted && r.RemovedAt.HasValue);
            if (filter.StartDate.HasValue) q = q.Where(r => r.RemovedAt >= filter.StartDate);
            if (filter.EndDate.HasValue) q = q.Where(r => r.RemovedAt <= filter.EndDate);

            var rows = await q.OrderByDescending(r => r.RemovedAt).Take(50)
                .Include(r => r.NewPart).Include(r => r.Asset).ToListAsync();

            return rows.Select(r => new PartLifeRowDto
            {
                PartId = r.NewPartId,
                PartNumber = r.NewPart?.PartNumber ?? r.OldPartNumber,
                SerialNumber = r.OldSerialNumber,
                AssetCode = r.Asset?.AssetCode ?? "",
                InstalledAt = r.InstalledAt,
                RemovedAt = r.RemovedAt,
                LifeDays = r.RemovedAt.HasValue ? (decimal)(r.RemovedAt.Value - r.InstalledAt).TotalDays : null,
                FailureReason = r.FailureReason,
                RemovalReason = r.RemovalReason
            }).ToList();
        }

        private async Task<List<ChartPointDto>> PartGroupChartAsync(ReportingFilterDto filter, bool bySupplier)
        {
            var q = TenantScopeHelper.ApplyTenantScope(_context.Parts.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(p => !p.IsDeleted);
            if (bySupplier)
            {
                return await q.GroupBy(p => string.IsNullOrEmpty(p.SupplierName) ? "Unknown" : p.SupplierName)
                    .Select(g => new ChartPointDto { Label = g.Key, Value = g.Count() })
                    .OrderByDescending(x => x.Value).Take(15).ToListAsync();
            }
            return await q.GroupBy(p => p.Manufacturer ?? "Unknown")
                .Select(g => new ChartPointDto { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value).Take(15).ToListAsync();
        }

        private async Task<List<EngineerPerformanceRowDto>> BuildEngineerPerformanceAsync(ReportingFilterDto filter)
        {
            var assetIds = ScopedAssets(filter).Select(a => a.Id);
            var woQ = TenantScopeHelper.ApplyTenantScope(_context.WorkOrders.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(w => !w.IsDeleted && w.AssignedToUserId.HasValue && assetIds.Contains(w.AssetId));
            if (filter.EngineerUserId.HasValue) woQ = woQ.Where(w => w.AssignedToUserId == filter.EngineerUserId);
            if (filter.StartDate.HasValue) woQ = woQ.Where(w => w.CreatedOn >= filter.StartDate);
            if (filter.EndDate.HasValue) woQ = woQ.Where(w => w.CreatedOn <= filter.EndDate);

            var wos = await woQ.Include(w => w.AssetIssue).Include(w => w.AssignedToUser).ToListAsync();
            var reopenedIds = await _context.WorkOrderStatusHistories.AsNoTracking()
                .Where(h => h.ToStatus == (int)Enums.WorkOrderStatus.Reopened)
                .Select(h => h.WorkOrderId).Distinct().ToListAsync();

            var groups = wos.GroupBy(w => w.AssignedToUserId!.Value);
            var rows = new List<EngineerPerformanceRowDto>();

            foreach (var g in groups)
            {
                var user = g.First().AssignedToUser;
                var completed = g.Count(w => w.Status == (int)Enums.WorkOrderStatus.Completed || w.Status == (int)Enums.WorkOrderStatus.Closed);
                var pending = g.Count(w => OpenWorkOrderStatuses.Contains(w.Status));
                var overdue = g.Count(w => w.DueDate.HasValue && w.DueDate < DateTime.UtcNow && OpenWorkOrderStatuses.Contains(w.Status));
                var assigned = g.Count();
                var reopened = g.Count(w => reopenedIds.Contains(w.Id));

                var repairTimes = g.Where(w => w.StartedAt.HasValue && w.CompletedAt.HasValue)
                    .Select(w => (w.CompletedAt!.Value - w.StartedAt!.Value).TotalMinutes).ToList();
                var responseTimes = g.Where(w => w.AssetIssue != null && w.StartedAt.HasValue)
                    .Select(w => (w.StartedAt!.Value - w.AssetIssue.ReportedAt).TotalMinutes).Where(m => m >= 0).ToList();

                var userId = g.Key;
                var pmCompleted = await TenantScopeHelper.ApplyTenantScope(_context.PreventiveMaintenanceOccurrences.AsQueryable(), _tenantProvider, filter.TenantId)
                    .CountAsync(p => p.CompletedByUserId == userId && p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed);
                var pmOverdue = await TenantScopeHelper.ApplyTenantScope(_context.PreventiveMaintenanceOccurrences.AsQueryable(), _tenantProvider, filter.TenantId)
                    .CountAsync(p => p.CompletedByUserId == userId && p.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Overdue);

                var partsUsed = await TenantScopeHelper.ApplyTenantScope(_context.PartReplacements.AsQueryable(), _tenantProvider, filter.TenantId)
                    .CountAsync(r => r.InstalledByUserId == userId && !r.IsDeleted);

                rows.Add(new EngineerPerformanceRowDto
                {
                    UserId = userId,
                    EngineerName = user?.FullName ?? user?.UserName ?? userId.ToString(),
                    AssignedJobs = assigned,
                    CompletedJobs = completed,
                    PendingJobs = pending,
                    OverdueJobs = overdue,
                    CompletionRatePercent = assigned == 0 ? 0 : Math.Round((decimal)completed / assigned * 100, 1),
                    AvgRepairTimeMinutes = repairTimes.Count == 0 ? null : (decimal?)Math.Round(repairTimes.Average(), 1),
                    AvgResponseTimeMinutes = responseTimes.Count == 0 ? null : (decimal?)Math.Round(responseTimes.Average(), 1),
                    ReopenedJobs = reopened,
                    PmCompleted = pmCompleted,
                    PmOverdue = pmOverdue,
                    PartsUsed = partsUsed
                });
            }

            return rows.OrderByDescending(r => r.CompletedJobs).ToList();
        }

        private async Task<decimal> ComputeTotalDowntimeMinutesAsync(ReportingFilterDto filter)
        {
            var assetIds = await ScopedAssets(filter).Select(a => a.Id).ToListAsync();
            if (assetIds.Count == 0) return 0;

            var issues = await TenantScopeHelper.ApplyTenantScope(_context.AssetIssues.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(i => !i.IsDeleted && assetIds.Contains(i.AssetId)).AsNoTracking().ToListAsync();
            var woIds = issues.Where(i => i.WorkOrderId.HasValue).Select(i => i.WorkOrderId!.Value).Distinct().ToList();
            var wos = await _context.WorkOrders.AsNoTracking().Where(w => woIds.Contains(w.Id)).ToDictionaryAsync(w => w.Id);

            decimal total = 0;
            foreach (var assetId in assetIds)
            {
                var assetIssues = issues.Where(i => i.AssetId == assetId).ToList();
                var intervals = new List<(DateTime Start, DateTime End)>();
                foreach (var i in assetIssues)
                {
                    DateTime? end = i.ResolvedAt;
                    if (i.WorkOrderId.HasValue && wos.TryGetValue(i.WorkOrderId.Value, out var wo))
                        end = wo.AssetRestoredAt ?? wo.CompletedAt ?? wo.ClosedAt ?? i.ResolvedAt;
                    if (!end.HasValue || end <= i.ReportedAt) continue;
                    intervals.Add((i.ReportedAt, end.Value));
                }
                intervals = intervals.OrderBy(x => x.Start).ToList();
                var merged = MergeIntervals(intervals);
                foreach (var m in merged)
                {
                    if (ReportingPeriodHelper.OverlapsPeriod(m.Start, m.End, filter.StartDate, filter.EndDate))
                        total += (decimal)(m.End - m.Start).TotalMinutes;
                }
            }
            return total;
        }

        private static List<(DateTime Start, DateTime End)> MergeIntervals(List<(DateTime Start, DateTime End)> raw)
        {
            var merged = new List<(DateTime Start, DateTime End)>();
            foreach (var r in raw.OrderBy(x => x.Start))
            {
                if (merged.Count == 0) { merged.Add(r); continue; }
                var last = merged[^1];
                if (r.Start <= last.End)
                    merged[^1] = (last.Start, r.End > last.End ? r.End : last.End);
                else merged.Add(r);
            }
            return merged;
        }

        private async Task<decimal?> ComputeMttrHoursAsync(ReportingFilterDto filter)
        {
            var assetIds = ScopedAssets(filter).Select(a => a.Id);
            var q = TenantScopeHelper.ApplyTenantScope(_context.WorkOrders.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(w => !w.IsDeleted && assetIds.Contains(w.AssetId) && w.StartedAt.HasValue && w.CompletedAt.HasValue);
            if (filter.StartDate.HasValue) q = q.Where(w => w.CompletedAt >= filter.StartDate);
            if (filter.EndDate.HasValue) q = q.Where(w => w.CompletedAt <= filter.EndDate);

            var rows = await q.Select(w => new { w.StartedAt, w.CompletedAt }).ToListAsync();
            if (rows.Count == 0) return null;
            var avgMinutes = rows.Average(w => (w.CompletedAt!.Value - w.StartedAt!.Value).TotalMinutes);
            return Math.Round((decimal)avgMinutes / 60, 2);
        }

        private async Task<decimal?> ComputeMttrHoursForAssetAsync(int assetId, ReportingFilterDto filter)
        {
            filter = CloneFilterForAsset(filter, assetId);
            return await ComputeMttrHoursAsync(filter);
        }

        private async Task<decimal?> ComputeMtbfHoursAsync(ReportingFilterDto filter)
        {
            var assetIds = await ScopedAssets(filter).Select(a => a.Id).ToListAsync();
            var mtbfValues = new List<double>();
            foreach (var assetId in assetIds)
            {
                var v = await ComputeMtbfHoursForAssetAsync(assetId, filter);
                if (v.HasValue) mtbfValues.Add((double)v.Value);
            }
            if (mtbfValues.Count == 0) return null;
            return Math.Round((decimal)mtbfValues.Average(), 2);
        }

        private async Task<decimal?> ComputeMtbfHoursForAssetAsync(int assetId, ReportingFilterDto filter)
        {
            var q = TenantScopeHelper.ApplyTenantScope(_context.AssetIssues.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(i => !i.IsDeleted && i.AssetId == assetId);
            if (filter.StartDate.HasValue) q = q.Where(i => i.ReportedAt >= filter.StartDate);
            if (filter.EndDate.HasValue) q = q.Where(i => i.ReportedAt <= filter.EndDate);

            var times = await q.OrderBy(i => i.ReportedAt).Select(i => i.ReportedAt).ToListAsync();
            if (times.Count < 2) return null;

            var gaps = new List<double>();
            for (var i = 1; i < times.Count; i++)
                gaps.Add((times[i] - times[i - 1]).TotalHours);
            return Math.Round((decimal)gaps.Average(), 2);
        }

        private async Task<(decimal? Rate, int Denominator)> ComputeFirstTimeFixRateAsync(ReportingFilterDto filter)
        {
            var issues = FilteredIssuesQuery(filter).Where(i => i.Status == (int)Enums.IssueStatus.Resolved);
            var resolved = await issues.Select(i => new { i.Id, i.WorkOrderId }).ToListAsync();
            if (resolved.Count == 0) return (null, 0);

            var woIds = resolved.Where(i => i.WorkOrderId.HasValue).Select(i => i.WorkOrderId!.Value).Distinct().ToList();
            var reopenedWo = await _context.WorkOrderStatusHistories.AsNoTracking()
                .Where(h => woIds.Contains(h.WorkOrderId) && h.ToStatus == (int)Enums.WorkOrderStatus.Reopened)
                .Select(h => h.WorkOrderId).Distinct().ToListAsync();

            var firstTime = resolved.Count(i => !i.WorkOrderId.HasValue || !reopenedWo.Contains(i.WorkOrderId.Value));
            return (Math.Round((decimal)firstTime / resolved.Count * 100, 1), resolved.Count);
        }

        private async Task<decimal?> ComputeAvgResponseMinutesAsync(ReportingFilterDto filter)
        {
            var assetIds = ScopedAssets(filter).Select(a => a.Id);
            var q = TenantScopeHelper.ApplyTenantScope(_context.WorkOrders.AsQueryable(), _tenantProvider, filter.TenantId)
                .Where(w => !w.IsDeleted && w.AssetIssueId.HasValue && w.StartedAt.HasValue && assetIds.Contains(w.AssetId));
            if (filter.StartDate.HasValue) q = q.Where(w => w.StartedAt >= filter.StartDate);
            if (filter.EndDate.HasValue) q = q.Where(w => w.StartedAt <= filter.EndDate);

            var rows = await q.Include(w => w.AssetIssue).ToListAsync();
            var minutes = rows.Where(w => w.AssetIssue != null)
                .Select(w => (w.StartedAt!.Value - w.AssetIssue!.ReportedAt).TotalMinutes)
                .Where(m => m >= 0).ToList();
            if (minutes.Count == 0) return null;
            return Math.Round((decimal)minutes.Average(), 1);
        }
    }
}
