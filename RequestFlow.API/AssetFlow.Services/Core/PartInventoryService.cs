using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AssetFlow.Common.Enum;
using AssetFlow.Common.Helper;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.SparePart;
using AssetFlow.Data.Provider;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.SparePart;
using System.Linq.Dynamic.Core;
using System.Net;
using X.PagedList;

namespace AssetFlow.Services.Core
{
    public class PartInventoryService : IPartInventoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PartInventoryService(ApplicationDbContext context, ITenantProvider tenantProvider, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseDto<List<PartInventoryDto>>> FilterAsync(PartInventoryFilterDto model)
        {
            var response = new ResponseDto<List<PartInventoryDto>>();
            IQueryable<PartInventory> query = BuildQuery();
            query = TenantScopeHelper.ApplyTenantScope(query, _tenantProvider, model.TenantId);

            query = query.Where(x =>
                (!model.PartId.HasValue || x.PartId == model.PartId) &&
                (!model.LocationId.HasValue || x.LocationId == model.LocationId) &&
                (!model.IsActive.HasValue || x.IsActive == model.IsActive));

            if (model.LowStockOnly == true)
            {
                query = query.Where(i =>
                    (_context.PartInventories.Where(x => x.PartId == i.PartId && x.IsActive)
                        .Sum(x => (decimal?)x.QuantityAvailable) ?? 0) < i.Part.MinStockLevel);
            }

            var projected = query.Select(ProjectToDto());
            model.OrderByProp ??= nameof(PartInventory.PartId);
            var ordered = model.SortDirection == (int)Enums.OrderBy.Ascending
                ? projected.OrderBy(model.OrderByProp)
                : projected.OrderBy($"{model.OrderByProp} descending");

            var paged = await ordered.ToPagedListAsync(model.PageNumber, model.PageSize);
            var list = paged.ToList();
            foreach (var item in list)
            {
                var stock = await PartStockHelper.GetAvailableStockAsync(_context, item.PartId);
                var part = await _context.Parts.FindAsync(item.PartId);
                item.IsLowStock = part != null && stock < part.MinStockLevel;
            }
            response.Result = list;
            return response;
        }

        public async Task<ResponseDto<PartInventoryDto>> ReceiptAsync(PartReceiptDto dto)
        {
            var batch = new PartBatchReceiptDto
            {
                TenantId = dto.TenantId,
                PartId = dto.PartId,
                LocationId = dto.LocationId,
                SupplierId = dto.SupplierId,
                Quantity = 1,
                ReceiptMode = 0,
                ReceivedDate = dto.ReceivedDate,
                WarrantyStartDate = dto.WarrantyStartDate,
                WarrantyEndDate = dto.WarrantyEndDate,
                Remarks = dto.Remarks
            };

            var batchResponse = await BatchReceiptAsync(batch);
            var response = new ResponseDto<PartInventoryDto> { StatusCode = batchResponse.StatusCode };
            if (!batchResponse.Success)
            {
                foreach (var err in batchResponse.Errors)
                    response.AddError(err);
                return response;
            }
            if (batchResponse.Result?.Items?.Count > 0)
                response.Result = batchResponse.Result.Items[0];
            return response;
        }

        public async Task<ResponseDto<PartBatchReceiptResultDto>> BatchReceiptAsync(PartBatchReceiptDto dto)
        {
            var response = new ResponseDto<PartBatchReceiptResultDto>();
            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            if (dto.Quantity < 1 || dto.Quantity > 500)
            {
                response.AddError("Quantity must be between 1 and 500.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var part = await _context.Parts.FindAsync(dto.PartId);
            if (part == null) { response.AddError("Part not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (part.TenantId != tenantResult.TenantId) { response.AddError("Part tenant mismatch."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var locError = await ValidateLocationAsync(dto.LocationId, tenantResult.TenantId);
            if (locError != null) { response.AddError(locError); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var supError = await ValidateSupplierAsync(dto.SupplierId, tenantResult.TenantId);
            if (supError != null) { response.AddError(supError); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var supplierRefs = dto.SupplierSerialReferences?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList() ?? new List<string>();

            const int scanMode = 1;
            if (dto.ReceiptMode == scanMode)
            {
                if (supplierRefs.Count != dto.Quantity)
                {
                    response.AddError("Enter or scan one supplier serial per unit (count must match quantity).");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                if (supplierRefs.Distinct(StringComparer.OrdinalIgnoreCase).Count() != supplierRefs.Count)
                {
                    response.AddError("Duplicate supplier serial references in this receipt.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }
            }
            else if (supplierRefs.Count > 0)
            {
                response.AddError("Supplier serial references are only used in scan mode.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            if (!part.IsSerialized)
            {
                part.IsSerialized = true;
                _context.Parts.Update(part);
            }

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var internalSerials = await PartSerialNumberGenerator.GetNextSerialsAsync(
                    _context, part.Id, tenantResult.TenantId, dto.Quantity);

                var result = new PartBatchReceiptResultDto();
                var received = dto.ReceivedDate ?? DateTime.UtcNow;

                var receiptTransactionId = await CreateTransactionAsync(
                    part.Id,
                    (int)Enums.PartTransactionType.Receipt,
                    dto.Quantity,
                    null,
                    dto.LocationId,
                    dto.Remarks,
                    tenantResult.TenantId,
                    dto.SupplierId,
                    dto.ReceivedDate);

                result.ReceiptTransactionId = receiptTransactionId;

                for (var i = 0; i < dto.Quantity; i++)
                {
                    var internalSerial = internalSerials[i];
                    if (await PartSerialNumberGenerator.SerialExistsAsync(_context, tenantResult.TenantId, internalSerial))
                    {
                        await tx.RollbackAsync();
                        response.AddError($"Internal serial conflict: {internalSerial}. Retry the receipt.");
                        response.StatusCode = HttpStatusCode.Conflict;
                        return response;
                    }

                    // Auto: our generated serial is the item identifier. Scan: store supplier label separately; internal serial is still system-generated.
                    var supplierRef = dto.ReceiptMode == scanMode
                        ? supplierRefs[i].Trim()
                        : internalSerial;

                    var serialEntity = new PartSerialNumber
                    {
                        TenantId = tenantResult.TenantId,
                        PartId = part.Id,
                        SerialNumber = internalSerial,
                        SupplierId = dto.SupplierId,
                        SupplierSerialReference = supplierRef,
                        Status = (int)Enums.PartInventoryStatus.InStock,
                        ReceivedDate = received,
                        LocationId = dto.LocationId,
                        WarrantyStartDate = dto.WarrantyStartDate,
                        WarrantyEndDate = dto.WarrantyEndDate,
                        IsActive = true
                    };
                    _context.PartSerialNumbers.Add(serialEntity);
                    await _context.SaveChangesAsync();

                    var inventory = new PartInventory
                    {
                        TenantId = tenantResult.TenantId,
                        PartId = part.Id,
                        LocationId = dto.LocationId,
                        PartSerialNumberId = serialEntity.Id,
                        PartTransactionId = receiptTransactionId,
                        QuantityAvailable = 1,
                        QuantityReserved = 0,
                        Status = (int)Enums.PartInventoryStatus.InStock,
                        IsActive = true
                    };
                    _context.PartInventories.Add(inventory);
                    await _context.SaveChangesAsync();

                    var mapped = await MapToDtoAsync(inventory.Id);
                    if (mapped != null)
                        result.Items.Add(mapped);
                    result.GeneratedSerialNumbers.Add(internalSerial);
                }

                await tx.CommitAsync();
                response.Result = result;
                return response;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<ResponseDto<PartInventoryDto>> TransferAsync(PartTransferDto dto)
        {
            var response = new ResponseDto<PartInventoryDto>();
            var inv = await _context.PartInventories.FindAsync(dto.PartInventoryId);
            if (inv == null) { response.AddError("Inventory not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (dto.Quantity <= 0 || dto.Quantity > inv.QuantityAvailable)
            { response.AddError("Invalid transfer quantity."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId ?? inv.TenantId);
            var locError = await ValidateLocationAsync(dto.ToLocationId, tenantResult.TenantId);
            if (locError != null) { response.AddError(locError); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var fromLocationId = inv.LocationId;
            inv.QuantityAvailable -= dto.Quantity;

            PartInventory target = await _context.PartInventories.FirstOrDefaultAsync(x =>
                x.PartId == inv.PartId && x.LocationId == dto.ToLocationId && x.PartSerialNumberId == inv.PartSerialNumberId);

            if (target == null)
            {
                target = new PartInventory
                {
                    TenantId = inv.TenantId,
                    PartId = inv.PartId,
                    LocationId = dto.ToLocationId,
                    PartSerialNumberId = inv.PartSerialNumberId,
                    QuantityAvailable = dto.Quantity,
                    QuantityReserved = 0,
                    Status = inv.Status,
                    IsActive = true
                };
                _context.PartInventories.Add(target);
            }
            else
                target.QuantityAvailable += dto.Quantity;

            if (inv.PartSerialNumberId.HasValue)
            {
                var serial = await _context.PartSerialNumbers.FindAsync(inv.PartSerialNumberId.Value);
                if (serial != null) serial.LocationId = dto.ToLocationId;
            }

            await _context.SaveChangesAsync();
            await CreateTransactionAsync(inv.PartId, (int)Enums.PartTransactionType.Transfer,
                dto.Quantity, fromLocationId, dto.ToLocationId, dto.Remarks, inv.TenantId, null, null);

            response.Result = await MapToDtoAsync(target.Id);
            return response;
        }

        public async Task<ResponseDto<PartInventoryDto>> AdjustAsync(PartAdjustmentDto dto)
        {
            var response = new ResponseDto<PartInventoryDto>();
            var inv = await _context.PartInventories.FindAsync(dto.PartInventoryId);
            if (inv == null) { response.AddError("Inventory not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var newQty = inv.QuantityAvailable + dto.QuantityChange;
            if (newQty < 0) { response.AddError("Adjustment would result in negative quantity."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            inv.QuantityAvailable = newQty;
            await _context.SaveChangesAsync();
            await CreateTransactionAsync(inv.PartId, (int)Enums.PartTransactionType.Adjustment,
                Math.Abs(dto.QuantityChange), inv.LocationId, inv.LocationId, dto.Remarks, inv.TenantId, null, null);

            response.Result = await MapToDtoAsync(inv.Id);
            return response;
        }

        public async Task<ResponseDto<bool>> DeleteAsync(int id)
        {
            var response = new ResponseDto<bool>();
            var entity = await _context.PartInventories.FindAsync(id);
            if (entity == null) { response.AddError("Inventory not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            entity.IsDeleted = true;
            entity.IsActive = false;
            await _context.SaveChangesAsync();
            response.Result = true;
            return response;
        }

        private async Task<int> CreateTransactionAsync(
            int partId,
            int type,
            decimal qty,
            int? fromLoc,
            int? toLoc,
            string remarks,
            int? tenantId,
            int? supplierId,
            DateTime? transactionDate)
        {
            int? userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var entity = new PartTransaction
            {
                TenantId = tenantId,
                PartId = partId,
                PartSerialNumberId = null,
                TransactionType = type,
                Quantity = qty,
                FromLocationId = fromLoc,
                ToLocationId = toLoc,
                TransactionDate = transactionDate ?? DateTime.UtcNow,
                PerformedByUserId = userId,
                Remarks = remarks,
                SupplierId = supplierId,
                IsActive = true
            };
            _context.PartTransactions.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        private async Task<string?> ValidateSupplierAsync(int supplierId, int? tenantId)
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);
            if (supplier == null || !supplier.IsActive) return "Supplier not found or inactive.";
            if (supplier.TenantId != tenantId) return "Supplier must belong to the same tenant.";
            return null;
        }

        private async Task<string?> ValidateLocationAsync(int locationId, int? tenantId)
        {
            var loc = await _context.Locations.FindAsync(locationId);
            if (loc == null || !loc.IsActive) return "Location not found or inactive.";
            if (loc.TenantId != tenantId) return "Location must belong to the same tenant.";
            return null;
        }

        private IQueryable<PartInventory> BuildQuery() =>
            _context.PartInventories
                .Include(x => x.Part)
                .Include(x => x.Location)
                .Include(x => x.PartSerialNumber).ThenInclude(s => s.Supplier)
                .Include(x => x.Tenant);

        private static System.Linq.Expressions.Expression<Func<PartInventory, PartInventoryDto>> ProjectToDto() =>
            x => new PartInventoryDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,
                PartId = x.PartId,
                PartNumber = x.Part.PartNumber,
                PartName = x.Part.PartName,
                LocationId = x.LocationId,
                LocationName = x.Location.Name,
                PartSerialNumberId = x.PartSerialNumberId,
                SerialNumber = x.PartSerialNumber != null ? x.PartSerialNumber.SerialNumber : null,
                SupplierSerialReference = x.PartSerialNumber != null ? x.PartSerialNumber.SupplierSerialReference : null,
                SupplierName = x.PartSerialNumber != null && x.PartSerialNumber.Supplier != null ? x.PartSerialNumber.Supplier.Name : null,
                QuantityAvailable = x.QuantityAvailable,
                QuantityReserved = x.QuantityReserved,
                Status = x.Status,
                IsActive = x.IsActive,
                PartTransactionId = x.PartTransactionId
            };

        private async Task<PartInventoryDto?> MapToDtoAsync(int id) =>
            await BuildQuery().Where(x => x.Id == id).Select(ProjectToDto()).FirstOrDefaultAsync();
    }
}
