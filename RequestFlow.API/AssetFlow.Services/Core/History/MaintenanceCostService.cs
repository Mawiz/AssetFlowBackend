using AssetFlow.Common.Enum;
using AssetFlow.Common.Helper;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.History;
using AssetFlow.Data.Provider;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.History;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AssetFlow.Services.Core.History
{
    public class MaintenanceCostService : IMaintenanceCostService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MaintenanceCostService(
            ApplicationDbContext context,
            ITenantProvider tenantProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseDto<WorkOrderCostSummaryDto>> GetWorkOrderCostSummaryAsync(int workOrderId)
        {
            var response = new ResponseDto<WorkOrderCostSummaryDto>();
            var wo = await _context.WorkOrders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == workOrderId);
            if (wo == null) { response.AddError("Work order not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (!EnsureAccess(wo.TenantId, response)) return response;
            response.Result = await BuildWorkOrderCostSummaryAsync(workOrderId);
            return response;
        }

        public async Task<ResponseDto<List<AssetCostHistoryDto>>> GetByWorkOrderAsync(int workOrderId)
        {
            var response = new ResponseDto<List<AssetCostHistoryDto>>();
            var wo = await _context.WorkOrders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == workOrderId);
            if (wo == null) { response.AddError("Work order not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (!EnsureAccess(wo.TenantId, response)) return response;
            response.Result = await QueryCostHistoryAsync(wo.AssetId, workOrderId, null, null, null);
            return response;
        }

        public async Task<ResponseDto<AssetCostHistoryDto>> UpsertCostAsync(MaintenanceCostUpsertDto dto)
        {
            var response = new ResponseDto<AssetCostHistoryDto>();
            if (dto.Amount < 0) { response.AddError("Amount must be zero or greater."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var asset = await _context.Assets.FindAsync(dto.AssetId);
            if (asset == null) { response.AddError("Asset not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId ?? asset.TenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            if (asset.TenantId != tenantResult.TenantId) { response.AddError("Asset tenant mismatch."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            if (dto.WorkOrderId.HasValue)
            {
                var wo = await _context.WorkOrders.FindAsync(dto.WorkOrderId.Value);
                if (wo == null || wo.AssetId != dto.AssetId) { response.AddError("Invalid work order for asset."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
                if (wo.Status == (int)Enums.WorkOrderStatus.Closed && dto.Id == null)
                { response.AddError("Cannot add costs to a closed work order."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            }

            var userId = UserHelper.GetCurrentUserId(_httpContextAccessor) ?? 0;
            MaintenanceCostRecord entity;
            if (dto.Id.HasValue)
            {
                entity = await _context.MaintenanceCostRecords.FirstOrDefaultAsync(x => x.Id == dto.Id);
                if (entity == null) { response.AddError("Cost record not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
                if (entity.WorkOrderId.HasValue)
                {
                    var wo = await _context.WorkOrders.FindAsync(entity.WorkOrderId);
                    if (wo?.Status == (int)Enums.WorkOrderStatus.Closed)
                    { response.AddError("Cannot modify costs on a closed work order."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
                }
            }
            else
            {
                entity = new MaintenanceCostRecord { CreatedByUserId = userId, IsActive = true };
                _context.MaintenanceCostRecords.Add(entity);
            }

            entity.TenantId = tenantResult.TenantId;
            entity.AssetId = dto.AssetId;
            entity.WorkOrderId = dto.WorkOrderId;
            entity.CostType = dto.CostType;
            entity.Description = dto.Description?.Trim() ?? string.Empty;
            entity.Amount = dto.Amount;
            entity.Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "USD" : dto.Currency.Trim();
            entity.CostDate = dto.CostDate;
            entity.PartReplacementId = dto.PartReplacementId;
            entity.PartId = dto.PartId;
            entity.Quantity = dto.Quantity;
            entity.UnitCost = dto.UnitCost;
            entity.ExternalServiceDescription = dto.ExternalServiceDescription?.Trim() ?? string.Empty;
            entity.ReferenceNumber = dto.ReferenceNumber?.Trim() ?? string.Empty;
            entity.Notes = dto.Notes?.Trim() ?? string.Empty;

            await _context.SaveChangesAsync();
            var list = await QueryCostHistoryAsync(entity.AssetId, entity.WorkOrderId, entity.Id, null, null);
            response.Result = list.FirstOrDefault();
            return response;
        }

        public async Task<ResponseDto<bool>> DeleteCostAsync(int id)
        {
            var response = new ResponseDto<bool>();
            var entity = await _context.MaintenanceCostRecords.FindAsync(id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (!EnsureAccess(entity.TenantId, response)) return response;
            if (entity.WorkOrderId.HasValue)
            {
                var wo = await _context.WorkOrders.FindAsync(entity.WorkOrderId);
                if (wo?.Status == (int)Enums.WorkOrderStatus.Closed)
                { response.AddError("Cannot delete costs on a closed work order."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            }
            entity.IsDeleted = true;
            entity.IsActive = false;
            await _context.SaveChangesAsync();
            response.Result = true;
            return response;
        }

        public async Task<ResponseDto<WorkOrderLaborUpsertDto>> UpsertLaborAsync(WorkOrderLaborUpsertDto dto)
        {
            var response = new ResponseDto<WorkOrderLaborUpsertDto>();
            if (dto.EndTime <= dto.StartTime) { response.AddError("End time must be after start time."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var wo = await _context.WorkOrders.FindAsync(dto.WorkOrderId);
            if (wo == null) { response.AddError("Work order not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (!EnsureAccess(wo.TenantId, response)) return response;
            if (wo.Status == (int)Enums.WorkOrderStatus.Closed && !dto.Id.HasValue)
            { response.AddError("Cannot add labor to a closed work order."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var duration = (int)Math.Round((dto.EndTime - dto.StartTime).TotalMinutes);
            if (duration <= 0) duration = 1;
            decimal laborCost = dto.LaborCost ?? 0;
            if (dto.HourlyRate.HasValue)
                laborCost = Math.Round(duration / 60m * dto.HourlyRate.Value, 2);

            WorkOrderLaborRecord entity;
            if (dto.Id.HasValue)
            {
                entity = await _context.WorkOrderLaborRecords.FirstOrDefaultAsync(x => x.Id == dto.Id);
                if (entity == null) { response.AddError("Labor record not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
                if (wo.Status == (int)Enums.WorkOrderStatus.Closed)
                { response.AddError("Cannot modify labor on a closed work order."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            }
            else
            {
                entity = new WorkOrderLaborRecord { IsActive = true };
                _context.WorkOrderLaborRecords.Add(entity);
            }

            entity.TenantId = wo.TenantId;
            entity.WorkOrderId = wo.Id;
            entity.UserId = dto.UserId;
            entity.StartTime = dto.StartTime;
            entity.EndTime = dto.EndTime;
            entity.DurationMinutes = duration;
            entity.HourlyRate = dto.HourlyRate;
            entity.LaborCost = laborCost;
            entity.Notes = dto.Notes?.Trim() ?? string.Empty;

            await _context.SaveChangesAsync();
            dto.Id = entity.Id;
            dto.LaborCost = laborCost;
            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<bool>> DeleteLaborAsync(int id)
        {
            var response = new ResponseDto<bool>();
            var entity = await _context.WorkOrderLaborRecords.FindAsync(id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (!EnsureAccess(entity.TenantId, response)) return response;
            var wo = await _context.WorkOrders.FindAsync(entity.WorkOrderId);
            if (wo?.Status == (int)Enums.WorkOrderStatus.Closed)
            { response.AddError("Cannot delete labor on a closed work order."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            entity.IsDeleted = true;
            entity.IsActive = false;
            await _context.SaveChangesAsync();
            response.Result = true;
            return response;
        }

        public async Task<ResponseDto<List<WorkOrderLaborUpsertDto>>> GetLaborByWorkOrderAsync(int workOrderId)
        {
            var response = new ResponseDto<List<WorkOrderLaborUpsertDto>>();
            var wo = await _context.WorkOrders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == workOrderId);
            if (wo == null) { response.AddError("Work order not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (!EnsureAccess(wo.TenantId, response)) return response;

            response.Result = await _context.WorkOrderLaborRecords.AsNoTracking()
                .Where(x => x.WorkOrderId == workOrderId && !x.IsDeleted)
                .OrderByDescending(x => x.StartTime)
                .Select(x => new WorkOrderLaborUpsertDto
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    WorkOrderId = x.WorkOrderId,
                    UserId = x.UserId,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    HourlyRate = x.HourlyRate,
                    LaborCost = x.LaborCost,
                    Notes = x.Notes
                }).ToListAsync();
            return response;
        }

        public async Task<WorkOrderCostSummaryDto> BuildWorkOrderCostSummaryAsync(int workOrderId)
        {
            var labor = await _context.WorkOrderLaborRecords.AsNoTracking()
                .Where(x => x.WorkOrderId == workOrderId && !x.IsDeleted)
                .SumAsync(x => x.LaborCost);

            var costs = await _context.MaintenanceCostRecords.AsNoTracking()
                .Where(x => x.WorkOrderId == workOrderId && !x.IsDeleted)
                .GroupBy(x => x.CostType)
                .Select(g => new { Type = g.Key, Sum = g.Sum(c => c.Amount) })
                .ToListAsync();

            decimal parts = costs.Where(c => c.Type == (int)Enums.MaintenanceCostType.Parts).Sum(c => c.Sum);
            decimal laborCost = labor + costs.Where(c => c.Type == (int)Enums.MaintenanceCostType.Labor).Sum(c => c.Sum);
            decimal external = costs.Where(c => c.Type == (int)Enums.MaintenanceCostType.ExternalService).Sum(c => c.Sum);
            decimal other = costs.Where(c => c.Type == (int)Enums.MaintenanceCostType.Other).Sum(c => c.Sum);

            return new WorkOrderCostSummaryDto
            {
                WorkOrderId = workOrderId,
                LaborCost = laborCost,
                PartsCost = parts,
                ExternalServiceCost = external,
                OtherCost = other,
                TotalCost = laborCost + parts + external + other
            };
        }

        public async Task<List<AssetCostHistoryDto>> QueryCostHistoryAsync(
            int assetId, int? workOrderId, int? costId, DateTime? start, DateTime? end)
        {
            var q = _context.MaintenanceCostRecords.AsNoTracking()
                .Where(x => x.AssetId == assetId && !x.IsDeleted);
            if (workOrderId.HasValue) q = q.Where(x => x.WorkOrderId == workOrderId);
            if (costId.HasValue) q = q.Where(x => x.Id == costId);
            if (start.HasValue) q = q.Where(x => x.CostDate >= start);
            if (end.HasValue) q = q.Where(x => x.CostDate <= end);

            var rows = await q.OrderByDescending(x => x.CostDate)
                .Select(x => new AssetCostHistoryDto
                {
                    Id = x.Id,
                    CostDate = x.CostDate,
                    WorkOrderId = x.WorkOrderId,
                    WorkOrderNumber = x.WorkOrder != null ? x.WorkOrder.WorkOrderNumber : null,
                    CostType = x.CostType,
                    Description = x.Description,
                    Amount = x.Amount,
                    Currency = x.Currency,
                    CreatedByUserName = x.CreatedByUser.FullName ?? x.CreatedByUser.UserName
                }).ToListAsync();
            foreach (var row in rows)
                row.CostTypeName = Enum.IsDefined(typeof(Enums.MaintenanceCostType), row.CostType)
                    ? ((Enums.MaintenanceCostType)row.CostType).ToString()
                    : row.CostType.ToString();
            return rows;
        }

        private bool EnsureAccess<T>(int? tenantId, ResponseDto<T> response)
        {
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, tenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return false; }
            return true;
        }
    }
}
