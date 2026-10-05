using AssetFlow.Common.Enum;
using AssetFlow.Common.Helper;
using AssetFlow.Data.Data;
using AssetFlow.Data.Provider;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.History;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AssetFlow.Services.Core.History
{
    public class AssetHistoryService : IAssetHistoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IMaintenanceCostService _costService;

        public AssetHistoryService(ApplicationDbContext context, ITenantProvider tenantProvider, IMaintenanceCostService costService)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _costService = costService;
        }

        public async Task<ResponseDto<AssetHistorySummaryDto>> GetSummaryAsync(int assetId)
        {
            var response = new ResponseDto<AssetHistorySummaryDto>();
            var asset = await LoadAssetAsync(assetId, response);
            if (asset == null) return response;

            var pmCount = await _context.PreventiveMaintenanceOccurrences.AsNoTracking()
                .CountAsync(x => x.AssetId == assetId && !x.IsDeleted && x.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed);
            var breakdowns = await _context.AssetIssues.AsNoTracking().CountAsync(x => x.AssetId == assetId && !x.IsDeleted);
            var woCount = await _context.WorkOrders.AsNoTracking().CountAsync(x => x.AssetId == assetId && !x.IsDeleted);
            var replacements = await _context.PartReplacements.AsNoTracking().CountAsync(x => x.AssetId == assetId && !x.IsDeleted);

            var downtime = await BuildDowntimeIntervalsAsync(assetId);
            var costFilter = new AssetHistoryFilterDto { AssetId = assetId };
            var costSummary = await ComputeCostSummaryAsync(assetId, costFilter);

            response.Result = new AssetHistorySummaryDto
            {
                AssetId = assetId,
                TotalMaintenanceEvents = pmCount + woCount + replacements,
                TotalBreakdowns = breakdowns,
                TotalWorkOrders = woCount,
                TotalPartsReplaced = replacements,
                TotalDowntimeMinutes = downtime.Sum(d => d.DurationMinutes ?? 0),
                TotalMaintenanceCost = costSummary.TotalMaintenanceCost,
                PartsCost = costSummary.PartsCost,
                LaborCost = costSummary.LaborCost,
                ExternalServiceCost = costSummary.ExternalServiceCost,
                OtherCost = costSummary.OtherCost
            };
            return response;
        }

        public async Task<ResponseDto<AssetHistoryPagedDto>> GetTimelineAsync(AssetHistoryFilterDto filter)
        {
            var response = new ResponseDto<AssetHistoryPagedDto>();
            if (await LoadAssetAsync(filter.AssetId, response) == null) return response;

            var events = await CollectTimelineEventsAsync(filter);
            if (filter.EventType.HasValue)
                events = events.Where(e => e.EventType == filter.EventType.Value).ToList();
            if (filter.WorkOrderId.HasValue)
                events = events.Where(e => e.WorkOrderId == filter.WorkOrderId).ToList();
            if (filter.UserId.HasValue)
                events = events.Where(e => e.UserName != null).ToList(); // lightweight filter

            events = events.OrderByDescending(e => e.EventAt).ToList();
            var total = events.Count;
            var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var size = filter.PageSize < 1 ? 15 : filter.PageSize;
            var items = events.Skip((page - 1) * size).Take(size).ToList();

            response.Result = new AssetHistoryPagedDto
            {
                Items = items,
                TotalCount = total,
                PageNumber = page,
                PageSize = size
            };
            return response;
        }

        public async Task<ResponseDto<List<AssetMaintenanceHistoryDto>>> GetMaintenanceHistoryAsync(AssetHistoryFilterDto filter)
        {
            var response = new ResponseDto<List<AssetMaintenanceHistoryDto>>();
            if (await LoadAssetAsync(filter.AssetId, response) == null) return response;

            var q = _context.PreventiveMaintenanceOccurrences.AsNoTracking()
                .Where(x => x.AssetId == filter.AssetId && !x.IsDeleted && x.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed);
            if (filter.StartDate.HasValue)
                q = q.Where(x => (x.CompletedAt ?? x.ScheduledDate ?? x.CreatedOn) >= filter.StartDate);
            if (filter.EndDate.HasValue)
                q = q.Where(x => (x.CompletedAt ?? x.ScheduledDate ?? x.CreatedOn) <= filter.EndDate);

            var rows = await q.OrderByDescending(x => x.CompletedAt ?? x.ScheduledDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new AssetMaintenanceHistoryDto
                {
                    Id = x.Id,
                    MaintenanceDate = x.CompletedAt ?? x.ScheduledDate,
                    MaintenanceTypeName = x.MaintenanceType.Name,
                    ScheduleName = x.MaintenanceSchedule.Name,
                    PmOccurrenceId = x.Id,
                    ChecklistName = x.MaintenanceChecklist != null ? x.MaintenanceChecklist.Name : null,
                    Status = x.Status,
                    StartedAt = x.StartedAt,
                    CompletedAt = x.CompletedAt,
                    CompletedByUserName = x.CompletedByUser != null ? (x.CompletedByUser.FullName ?? x.CompletedByUser.UserName) : null,
                    Remarks = x.Remarks,
                    WorkOrderId = x.WorkOrderId
                }).ToListAsync();

            foreach (var r in rows)
            {
                r.StatusName = ((Enums.PreventiveMaintenanceOccurrenceStatus)r.Status).ToString();
                if (r.WorkOrderId.HasValue)
                    r.WorkOrderNumber = await _context.WorkOrders.AsNoTracking().Where(w => w.Id == r.WorkOrderId).Select(w => w.WorkOrderNumber).FirstOrDefaultAsync();
            }
            response.Result = rows;
            return response;
        }

        public async Task<ResponseDto<List<AssetBreakdownHistoryDto>>> GetBreakdownHistoryAsync(AssetHistoryFilterDto filter)
        {
            var response = new ResponseDto<List<AssetBreakdownHistoryDto>>();
            if (await LoadAssetAsync(filter.AssetId, response) == null) return response;

            var q = _context.AssetIssues.AsNoTracking().Where(x => x.AssetId == filter.AssetId && !x.IsDeleted);
            if (filter.StartDate.HasValue) q = q.Where(x => x.ReportedAt >= filter.StartDate);
            if (filter.EndDate.HasValue) q = q.Where(x => x.ReportedAt <= filter.EndDate);

            var issues = await q.OrderByDescending(x => x.ReportedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = new List<AssetBreakdownHistoryDto>();
            foreach (var i in issues)
            {
                string woNum = null;
                string diagnosis = null, root = null;
                decimal? down = null;
                if (i.WorkOrderId.HasValue)
                {
                    var wo = await _context.WorkOrders.AsNoTracking()
                        .Include(w => w.Diagnosis)
                        .FirstOrDefaultAsync(w => w.Id == i.WorkOrderId);
                    if (wo != null)
                    {
                        woNum = wo.WorkOrderNumber;
                        diagnosis = wo.Diagnosis?.Diagnosis;
                        root = wo.Diagnosis?.RootCause;
                        var end = wo.AssetRestoredAt ?? wo.CompletedAt ?? i.ResolvedAt;
                        if (end.HasValue && end > i.ReportedAt)
                            down = (decimal)(end.Value - i.ReportedAt).TotalMinutes;
                    }
                }

                result.Add(new AssetBreakdownHistoryDto
                {
                    Id = i.Id,
                    IssueNumber = i.IssueNumber,
                    ReportedAt = i.ReportedAt,
                    IssueCategoryName = (await _context.IssueCategories.AsNoTracking().Where(c => c.Id == i.IssueCategoryId).Select(c => c.Name).FirstOrDefaultAsync()) ?? "",
                    Priority = i.Priority,
                    Description = i.Description,
                    ReportedByUserName = (await _context.ApplicationUsers.AsNoTracking().Where(u => u.Id == i.ReportedByUserId).Select(u => u.FullName ?? u.UserName).FirstOrDefaultAsync()) ?? "",
                    AssetStatusAtReport = i.AssetStatusAtReport,
                    ImmediateAction = i.ImmediateAction,
                    WorkOrderId = i.WorkOrderId,
                    WorkOrderNumber = woNum,
                    Diagnosis = diagnosis,
                    RootCause = root,
                    ResolutionRemarks = i.ResolutionRemarks,
                    ResolvedAt = i.ResolvedAt,
                    ResolvedByUserName = i.ResolvedByUserId.HasValue
                        ? await _context.ApplicationUsers.AsNoTracking().Where(u => u.Id == i.ResolvedByUserId).Select(u => u.FullName ?? u.UserName).FirstOrDefaultAsync()
                        : null,
                    DowntimeMinutes = down
                });
            }
            response.Result = result;
            return response;
        }

        public async Task<ResponseDto<List<AssetWorkOrderHistoryDto>>> GetWorkOrderHistoryAsync(AssetHistoryFilterDto filter)
        {
            var response = new ResponseDto<List<AssetWorkOrderHistoryDto>>();
            if (await LoadAssetAsync(filter.AssetId, response) == null) return response;

            var q = _context.WorkOrders.AsNoTracking()
                .Where(x => x.AssetId == filter.AssetId && !x.IsDeleted);
            if (filter.StartDate.HasValue) q = q.Where(x => x.CreatedOn >= filter.StartDate);
            if (filter.EndDate.HasValue) q = q.Where(x => x.CreatedOn <= filter.EndDate);

            var wos = await q.OrderByDescending(x => x.CreatedOn)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Include(x => x.Diagnosis)
                .Include(x => x.AssignedToUser)
                .ToListAsync();

            var list = new List<AssetWorkOrderHistoryDto>();
            foreach (var wo in wos)
            {
                var cost = await _costService.BuildWorkOrderCostSummaryAsync(wo.Id);
                var partsCount = await _context.PartReplacements.CountAsync(p => p.WorkOrderId == wo.Id && !p.IsDeleted);
                list.Add(new AssetWorkOrderHistoryDto
                {
                    Id = wo.Id,
                    WorkOrderNumber = wo.WorkOrderNumber,
                    SourceType = wo.SourceType,
                    SourceTypeName = ((Enums.WorkOrderSourceType)wo.SourceType).ToString(),
                    IssueId = wo.AssetIssueId,
                    PmOccurrenceId = wo.PreventiveMaintenanceOccurrenceId,
                    Title = wo.Title,
                    Priority = wo.Priority,
                    Status = wo.Status,
                    StatusName = ((Enums.WorkOrderStatus)wo.Status).ToString(),
                    AssignedToUserName = wo.AssignedToUser?.FullName ?? wo.AssignedToUser?.UserName,
                    AssignedAt = wo.AssignedAt,
                    AcceptedAt = wo.AcceptedAt,
                    StartedAt = wo.StartedAt,
                    CompletedAt = wo.CompletedAt,
                    ClosedAt = wo.ClosedAt,
                    Diagnosis = wo.Diagnosis?.Diagnosis,
                    RootCause = wo.Diagnosis?.RootCause,
                    WorkPerformed = wo.WorkPerformed,
                    FinalResult = wo.FinalResult,
                    PartsReplacedCount = partsCount,
                    LaborCost = cost.LaborCost,
                    PartsCost = cost.PartsCost,
                    ExternalServiceCost = cost.ExternalServiceCost,
                    OtherCost = cost.OtherCost,
                    TotalCost = cost.TotalCost,
                    Remarks = wo.Remarks
                });
            }
            response.Result = list;
            return response;
        }

        public async Task<ResponseDto<List<AssetPartHistoryDto>>> GetPartsHistoryAsync(AssetHistoryFilterDto filter)
        {
            var response = new ResponseDto<List<AssetPartHistoryDto>>();
            if (await LoadAssetAsync(filter.AssetId, response) == null) return response;

            var replacements = await _context.PartReplacements.AsNoTracking()
                .Where(x => x.AssetId == filter.AssetId && !x.IsDeleted)
                .OrderByDescending(x => x.InstalledAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Include(x => x.NewPart)
                .Include(x => x.InstalledByUser)
                .Include(x => x.WorkOrder)
                .Include(x => x.OldAssetComponent)
                .Include(x => x.NewPartSerialNumber)
                .ToListAsync();

            var list = new List<AssetPartHistoryDto>();
            foreach (var r in replacements)
            {
                list.Add(new AssetPartHistoryDto
                {
                    PartReplacementId = r.Id,
                    AssetComponentId = r.NewAssetComponentId,
                    PartNumber = r.NewPart?.PartNumber ?? r.OldPartNumber,
                    PartName = r.NewPart?.PartName,
                    SerialNumber = r.NewPartSerialNumber != null ? r.NewPartSerialNumber.SerialNumber : "",
                    ComponentName = r.OldAssetComponent?.ComponentName,
                    InstalledAt = r.InstalledAt,
                    RemovedAt = r.RemovedAt,
                    InstallationLocation = r.InstallationLocation,
                    WorkOrderId = r.WorkOrderId,
                    WorkOrderNumber = r.WorkOrder?.WorkOrderNumber,
                    EngineerName = r.InstalledByUser?.FullName ?? r.InstalledByUser?.UserName,
                    RemovalReason = r.RemovalReason,
                    FailureReason = r.FailureReason,
                    IsActive = true
                });
                if (r.OldAssetComponentId.HasValue)
                {
                    list.Add(new AssetPartHistoryDto
                    {
                        PartReplacementId = r.Id,
                        AssetComponentId = r.OldAssetComponentId,
                        PartNumber = r.OldPartNumber,
                        PartName = r.OldAssetComponent?.ComponentName,
                        SerialNumber = r.OldSerialNumber,
                        ComponentName = r.OldAssetComponent?.ComponentName,
                        InstalledAt = r.OldAssetComponent?.InstallationDate,
                        RemovedAt = r.RemovedAt,
                        WorkOrderId = r.WorkOrderId,
                        WorkOrderNumber = r.WorkOrder?.WorkOrderNumber,
                        FailureReason = r.FailureReason,
                        IsActive = false
                    });
                }
            }

            foreach (var p in list)
            {
                if (p.InstalledAt.HasValue && p.RemovedAt.HasValue)
                    p.LifeDays = (int)(p.RemovedAt.Value.Date - p.InstalledAt.Value.Date).TotalDays;
                p.Cost = await _context.MaintenanceCostRecords.AsNoTracking()
                    .Where(c => c.PartReplacementId == p.PartReplacementId && c.CostType == (int)Enums.MaintenanceCostType.Parts && !c.IsDeleted)
                    .Select(c => (decimal?)c.Amount).FirstOrDefaultAsync();
            }

            response.Result = list;
            return response;
        }

        public async Task<ResponseDto<List<AssetDowntimeHistoryDto>>> GetDowntimeHistoryAsync(AssetHistoryFilterDto filter)
        {
            var response = new ResponseDto<List<AssetDowntimeHistoryDto>>();
            if (await LoadAssetAsync(filter.AssetId, response) == null) return response;
            var intervals = await BuildDowntimeIntervalsAsync(filter.AssetId);
            if (filter.StartDate.HasValue) intervals = intervals.Where(i => i.DowntimeStart >= filter.StartDate).ToList();
            if (filter.EndDate.HasValue) intervals = intervals.Where(i => i.DowntimeStart <= filter.EndDate).ToList();
            response.Result = intervals
                .OrderByDescending(i => i.DowntimeStart)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();
            return response;
        }

        public async Task<ResponseDto<AssetCostSummaryDto>> GetCostSummaryAsync(int assetId, AssetHistoryFilterDto filter)
        {
            var response = new ResponseDto<AssetCostSummaryDto>();
            if (await LoadAssetAsync(assetId, response) == null) return response;
            response.Result = await ComputeCostSummaryAsync(assetId, filter);
            return response;
        }

        public async Task<ResponseDto<List<AssetCostHistoryDto>>> GetCostHistoryAsync(AssetHistoryFilterDto filter)
        {
            var response = new ResponseDto<List<AssetCostHistoryDto>>();
            if (await LoadAssetAsync(filter.AssetId, response) == null) return response;
            response.Result = await _costService.QueryCostHistoryAsync(
                filter.AssetId, filter.WorkOrderId, null, filter.StartDate, filter.EndDate);
            response.Result = response.Result
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();
            return response;
        }

        private async Task<AssetCostSummaryDto> ComputeCostSummaryAsync(int assetId, AssetHistoryFilterDto filter)
        {
            var q = _context.MaintenanceCostRecords.AsNoTracking()
                .Where(x => x.AssetId == assetId && !x.IsDeleted);
            if (filter.StartDate.HasValue) q = q.Where(x => x.CostDate >= filter.StartDate);
            if (filter.EndDate.HasValue) q = q.Where(x => x.CostDate <= filter.EndDate);

            var grouped = await q.GroupBy(x => x.CostType).Select(g => new { Type = g.Key, Sum = g.Sum(c => c.Amount), Count = g.Count() }).ToListAsync();
            var laborRecords = await _context.WorkOrderLaborRecords.AsNoTracking()
                .Where(x => !x.IsDeleted && _context.WorkOrders.Any(w => w.Id == x.WorkOrderId && w.AssetId == assetId && !w.IsDeleted))
                .SumAsync(x => x.LaborCost);

            decimal parts = grouped.Where(g => g.Type == (int)Enums.MaintenanceCostType.Parts).Sum(g => g.Sum);
            decimal labor = laborRecords + grouped.Where(g => g.Type == (int)Enums.MaintenanceCostType.Labor).Sum(g => g.Sum);
            decimal ext = grouped.Where(g => g.Type == (int)Enums.MaintenanceCostType.ExternalService).Sum(g => g.Sum);
            decimal other = grouped.Where(g => g.Type == (int)Enums.MaintenanceCostType.Other).Sum(g => g.Sum);
            var count = grouped.Sum(g => g.Count);

            return new AssetCostSummaryDto
            {
                PartsCost = parts,
                LaborCost = labor,
                ExternalServiceCost = ext,
                OtherCost = other,
                TotalMaintenanceCost = parts + labor + ext + other,
                RecordCount = count
            };
        }

        private async Task<List<AssetDowntimeHistoryDto>> BuildDowntimeIntervalsAsync(int assetId)
        {
            var issues = await _context.AssetIssues.AsNoTracking()
                .Where(i => i.AssetId == assetId && !i.IsDeleted)
                .ToListAsync();

            var raw = new List<(DateTime Start, DateTime End, int IssueId, string IssueNumber, int? WoId, string WoNum, string Reason)>();
            foreach (var i in issues)
            {
                DateTime? end = i.ResolvedAt;
                int? woId = i.WorkOrderId;
                string woNum = null;
                if (i.WorkOrderId.HasValue)
                {
                    var wo = await _context.WorkOrders.AsNoTracking().FirstOrDefaultAsync(w => w.Id == i.WorkOrderId);
                    if (wo != null)
                    {
                        woNum = wo.WorkOrderNumber;
                        end = wo.AssetRestoredAt ?? wo.CompletedAt ?? wo.ClosedAt ?? i.ResolvedAt;
                    }
                }
                if (!end.HasValue || end <= i.ReportedAt) continue;
                raw.Add((i.ReportedAt, end.Value, i.Id, i.IssueNumber, woId, woNum, i.Description));
            }

            raw = raw.OrderBy(r => r.Start).ToList();
            var merged = new List<(DateTime Start, DateTime End)>();
            foreach (var r in raw)
            {
                if (merged.Count == 0) { merged.Add((r.Start, r.End)); continue; }
                var last = merged[^1];
                if (r.Start <= last.End)
                    merged[^1] = (last.Start, r.End > last.End ? r.End : last.End);
                else
                    merged.Add((r.Start, r.End));
            }

            var result = new List<AssetDowntimeHistoryDto>();
            for (var idx = 0; idx < raw.Count; idx++)
            {
                var r = raw[idx];
                result.Add(new AssetDowntimeHistoryDto
                {
                    AssetId = assetId,
                    IssueId = r.IssueId,
                    IssueNumber = r.IssueNumber,
                    WorkOrderId = r.WoId,
                    WorkOrderNumber = r.WoNum,
                    DowntimeStart = r.Start,
                    DowntimeEnd = r.End,
                    DurationMinutes = (decimal)(r.End - r.Start).TotalMinutes,
                    Reason = r.Reason,
                    Status = "Recorded"
                });
            }
            return result;
        }

        private async Task<List<AssetHistoryEventDto>> CollectTimelineEventsAsync(AssetHistoryFilterDto filter)
        {
            var assetId = filter.AssetId;
            var events = new List<AssetHistoryEventDto>();
            var asset = await _context.Assets.AsNoTracking().FirstAsync(a => a.Id == assetId);
            events.Add(new AssetHistoryEventDto
            {
                EventAt = asset.CreatedOn,
                EventType = (int)Enums.AssetHistoryEventType.AssetCreated,
                EventTypeName = nameof(Enums.AssetHistoryEventType.AssetCreated),
                Title = "Asset created",
                Description = asset.Name,
                ReferenceNumber = asset.AssetCode
            });

            if (filter.StartDate.HasValue && asset.CreatedOn < filter.StartDate) events.Clear();
            else if (filter.EndDate.HasValue && asset.CreatedOn > filter.EndDate) events.RemoveAt(0);

            var pm = await _context.PreventiveMaintenanceOccurrences.AsNoTracking()
                .Where(x => x.AssetId == assetId && !x.IsDeleted && x.Status == (int)Enums.PreventiveMaintenanceOccurrenceStatus.Completed)
                .ToListAsync();
            foreach (var p in pm)
            {
                var at = p.CompletedAt ?? p.ScheduledDate ?? p.CreatedOn;
                events.Add(new AssetHistoryEventDto
                {
                    EventAt = at,
                    EventType = (int)Enums.AssetHistoryEventType.PreventiveMaintenance,
                    EventTypeName = "PreventiveMaintenance",
                    Title = "Preventive maintenance completed",
                    Description = p.Remarks,
                    PmOccurrenceId = p.Id,
                    WorkOrderId = p.WorkOrderId
                });
            }

            var issues = await _context.AssetIssues.AsNoTracking().Where(x => x.AssetId == assetId && !x.IsDeleted).ToListAsync();
            foreach (var i in issues)
            {
                events.Add(new AssetHistoryEventDto
                {
                    EventAt = i.ReportedAt,
                    EventType = (int)Enums.AssetHistoryEventType.BreakdownIssue,
                    EventTypeName = "BreakdownIssue",
                    Title = "Issue reported",
                    Description = i.Description,
                    ReferenceNumber = i.IssueNumber,
                    IssueId = i.Id,
                    WorkOrderId = i.WorkOrderId
                });
            }

            var statusHist = await _context.WorkOrderStatusHistories.AsNoTracking()
                .Where(h => !h.IsDeleted && _context.WorkOrders.Any(w => w.Id == h.WorkOrderId && w.AssetId == assetId))
                .ToListAsync();
            foreach (var h in statusHist)
            {
                events.Add(new AssetHistoryEventDto
                {
                    EventAt = h.ChangedAt,
                    EventType = (int)Enums.AssetHistoryEventType.WorkOrderStatusChange,
                    EventTypeName = "WorkOrderStatusChange",
                    Title = "Work order status changed",
                    Description = h.Remarks,
                    WorkOrderId = h.WorkOrderId,
                    Status = h.ToStatus,
                    StatusName = ((Enums.WorkOrderStatus)h.ToStatus).ToString(),
                    UserName = await _context.ApplicationUsers.AsNoTracking()
                        .Where(u => u.Id == h.ChangedByUserId)
                        .Select(u => u.FullName ?? u.UserName)
                        .FirstOrDefaultAsync()
                });
            }

            var replacements = await _context.PartReplacements.AsNoTracking().Where(x => x.AssetId == assetId && !x.IsDeleted).ToListAsync();
            foreach (var r in replacements)
            {
                events.Add(new AssetHistoryEventDto
                {
                    EventAt = r.InstalledAt,
                    EventType = (int)Enums.AssetHistoryEventType.PartReplaced,
                    EventTypeName = "PartReplaced",
                    Title = "Part replaced",
                    Description = r.FailureReason,
                    WorkOrderId = r.WorkOrderId,
                    PartReplacementId = r.Id,
                    ReferenceNumber = r.OldPartNumber
                });
            }

            var costs = await _context.MaintenanceCostRecords.AsNoTracking().Where(x => x.AssetId == assetId && !x.IsDeleted).ToListAsync();
            foreach (var c in costs)
            {
                events.Add(new AssetHistoryEventDto
                {
                    EventAt = c.CostDate,
                    EventType = (int)Enums.AssetHistoryEventType.CostRecorded,
                    EventTypeName = "CostRecorded",
                    Title = "Cost recorded",
                    Description = c.Description,
                    WorkOrderId = c.WorkOrderId,
                    Cost = c.Amount,
                    Currency = c.Currency
                });
            }

            if (filter.StartDate.HasValue) events = events.Where(e => e.EventAt >= filter.StartDate).ToList();
            if (filter.EndDate.HasValue) events = events.Where(e => e.EventAt <= filter.EndDate).ToList();
            return events;
        }

        private async Task<Data.Entities.Asset.Asset?> LoadAssetAsync<T>(int assetId, ResponseDto<T> response)
        {
            var asset = await _context.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Id == assetId && !a.IsDeleted);
            if (asset == null) { response.AddError("Asset not found."); response.StatusCode = HttpStatusCode.NotFound; return null; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, asset.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return null; }
            return asset;
        }
    }
}
