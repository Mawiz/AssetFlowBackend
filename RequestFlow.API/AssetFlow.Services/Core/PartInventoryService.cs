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



        public async Task<ResponseDto<PartInventoryDetailDto>> GetByIdAsync(int id)

        {

            var response = new ResponseDto<PartInventoryDetailDto>();

            var dto = await MapDetailAsync(id);

            if (dto == null) { response.AddError("Inventory not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            response.Result = dto;

            return response;

        }



        public async Task<ResponseDto<PartInventoryBatchDetailDto>> GetBatchByIdAsync(int batchId)

        {

            var response = new ResponseDto<PartInventoryBatchDetailDto>();

            var batch = await _context.PartInventoryBatches

                .Include(x => x.PartInventory).ThenInclude(i => i.Location)

                .Include(x => x.Part)

                .Include(x => x.Supplier)

                .FirstOrDefaultAsync(x => x.Id == batchId);

            if (batch == null) { response.AddError("Batch not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var batchSerials = await _context.PartSerialNumbers
                .Include(s => s.Location)
                .Include(s => s.PartInventoryBatch)
                .Where(s => s.IsActive && (
                    s.OriginPartInventoryBatchId == batchId
                    || (s.OriginPartInventoryBatchId == null && s.PartInventoryBatchId == batchId)))
                .OrderBy(s => s.SerialNumber)
                .ToListAsync();

            response.Result = new PartInventoryBatchDetailDto

            {

                Id = batch.Id,

                PartInventoryId = batch.PartInventoryId,

                PartId = batch.PartId,

                BatchReference = batch.BatchReference,

                SupplierId = batch.SupplierId,

                SupplierName = batch.Supplier?.Name,

                ReceivedDate = batch.ReceivedDate,

                ExpiryDate = batch.ExpiryDate,

                ExpectedLifeValue = batch.ExpectedLifeValue,

                ExpectedLifeUnit = batch.ExpectedLifeUnit,

                TotalQuantity = batch.TotalQuantity,

                AvailableQuantity = batch.AvailableQuantity,

                FaultyQuantity = batch.FaultyQuantity,

                QuarantineQuantity = batch.QuarantineQuantity,

                IssuedQuantity = batch.IssuedQuantity,

                Notes = batch.Notes,

                IsActive = batch.IsActive,

                PartNumber = batch.Part?.PartNumber,

                PartName = batch.Part?.PartName,

                LocationName = batch.PartInventory?.Location?.Name,

                SerialNumbers = batchSerials.Select(s => new PartSerialNumberDto

                {

                    Id = s.Id,

                    PartId = s.PartId,

                    SerialNumber = s.SerialNumber,

                    Status = s.Status,

                    LocationId = s.LocationId,

                    LocationName = s.Location?.Name,

                    ExpiryDate = s.ExpiryDate,

                    ExpectedLifeValue = s.ExpectedLifeValue,

                    ExpectedLifeUnit = s.ExpectedLifeUnit,

                    SupplierSerialReference = s.SupplierSerialReference,

                    SupplierId = s.SupplierId,

                    ReceivedDate = s.ReceivedDate,

                    WarrantyStartDate = s.WarrantyStartDate,

                    WarrantyEndDate = s.WarrantyEndDate,

                    IsActive = s.IsActive,

                    PartInventoryBatchId = s.PartInventoryBatchId,

                    OriginPartInventoryBatchId = s.OriginPartInventoryBatchId ?? s.PartInventoryBatchId,

                    BatchReference = s.PartInventoryBatch?.BatchReference,

                    IsAtOpenBatch = s.PartInventoryBatchId == batchId

                }).ToList()

            };

            return response;

        }



        public async Task<ResponseDto<List<PartInventoryDto>>> FilterAsync(PartInventoryFilterDto model)

        {

            var response = new ResponseDto<List<PartInventoryDto>>();

            IQueryable<PartInventory> query = BuildStockQuery();

            query = TenantScopeHelper.ApplyTenantScope(query, _tenantProvider, model.TenantId);



            query = query.Where(x =>

                (!model.PartId.HasValue || x.PartId == model.PartId) &&

                (!model.LocationId.HasValue || x.LocationId == model.LocationId) &&

                (!model.IsActive.HasValue || x.IsActive == model.IsActive));



            if (model.LowStockOnly == true)

            {

                query = query.Where(i =>

                    i.AvailableQuantity < i.Part.MinStockLevel);

            }



            var list = await query.ToListAsync();

            var dtos = new List<PartInventoryDto>();

            foreach (var inv in list)

            {

                var dto = await MapStockDtoAsync(inv);

                dtos.Add(dto);

            }



            model.OrderByProp ??= nameof(PartInventory.PartId);

            var ordered = model.SortDirection == (int)Enums.OrderBy.Ascending

                ? dtos.AsQueryable().OrderBy(model.OrderByProp)

                : dtos.AsQueryable().OrderBy($"{model.OrderByProp} descending");



            var paged = ordered.ToPagedList(model.PageNumber, model.PageSize);

            response.Result = paged.ToList();

            return response;

        }



        public async Task<ResponseDto<PartInventoryDto>> ReceiptAsync(PartReceiptDto dto)

        {

            var qty = (int)Math.Min(Math.Max(dto.Quantity, 1), 500);

            var receive = new PartReceiveStockDto

            {

                TenantId = dto.TenantId,

                PartId = dto.PartId,

                LocationId = dto.LocationId,

                Remarks = dto.Remarks,

                Lines = new List<PartReceiveLineDto>

                {

                    new()

                    {

                        SupplierId = dto.SupplierId,

                        Quantity = qty,

                        ReceivedDate = dto.ReceivedDate,

                        ExpiryDate = dto.ExpiryDate,

                        ExpectedLifeValue = dto.ExpectedLifeValue,

                        ExpectedLifeUnit = dto.ExpectedLifeUnit,

                        WarrantyStartDate = dto.WarrantyStartDate,

                        WarrantyEndDate = dto.WarrantyEndDate

                    }

                }

            };

            var batchResult = await ReceiveStockAsync(receive);

            var response = new ResponseDto<PartInventoryDto> { StatusCode = batchResult.StatusCode };

            if (!batchResult.Success) { response.Errors = batchResult.Errors; return response; }

            var stock = await _context.PartInventories.FirstOrDefaultAsync(x => x.PartId == dto.PartId && x.LocationId == dto.LocationId && x.IsActive);

            response.Result = stock != null ? await MapStockDtoAsync(stock) : null;

            return response;

        }



        public async Task<ResponseDto<PartBatchReceiptResultDto>> BatchReceiptAsync(PartBatchReceiptDto dto)

        {

            var receive = new PartReceiveStockDto

            {

                TenantId = dto.TenantId,

                PartId = dto.PartId,

                LocationId = dto.LocationId,

                Remarks = dto.Remarks,

                Lines = new List<PartReceiveLineDto>

                {

                    new()

                    {

                        SupplierId = dto.SupplierId,

                        Quantity = dto.Quantity,

                        ReceiptMode = dto.ReceiptMode,

                        SupplierSerialReferences = dto.SupplierSerialReferences ?? new List<string>(),

                        ReceivedDate = dto.ReceivedDate,

                        ExpiryDate = dto.ExpiryDate,

                        ExpectedLifeValue = dto.ExpectedLifeValue,

                        ExpectedLifeUnit = dto.ExpectedLifeUnit,

                        WarrantyStartDate = dto.WarrantyStartDate,

                        WarrantyEndDate = dto.WarrantyEndDate

                    }

                }

            };

            return await ReceiveStockAsync(receive);

        }



        public async Task<ResponseDto<PartBatchReceiptResultDto>> ReceiveStockAsync(PartReceiveStockDto dto)

        {

            var response = new ResponseDto<PartBatchReceiptResultDto>();

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);

            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }



            if (dto.Lines == null || dto.Lines.Count == 0)

            { response.AddError("At least one receipt line is required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }



            var part = await _context.Parts.FindAsync(dto.PartId);

            if (part == null || part.TenantId != tenantResult.TenantId)

            { response.AddError("Part not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }



            var locError = await ValidateLocationAsync(dto.LocationId, tenantResult.TenantId);

            if (locError != null) { response.AddError(locError); response.StatusCode = HttpStatusCode.BadRequest; return response; }



            foreach (var line in dto.Lines)

            {

                if (line.Quantity < 1 || line.Quantity > 500)

                { response.AddError("Each line quantity must be between 1 and 500."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

                var supErr = await ValidateSupplierAsync(line.SupplierId, tenantResult.TenantId);

                if (supErr != null) { response.AddError(supErr); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            }



            await using var tx = await _context.Database.BeginTransactionAsync();

            try

            {

                var stock = await GetOrCreateStockAsync(part.Id, dto.LocationId, tenantResult.TenantId);

                var result = new PartBatchReceiptResultDto();

                decimal totalReceived = 0;



                foreach (var line in dto.Lines)

                {

                    if (part.IsSerialized)

                    {

                        var lineResult = await ReceiveSerializedLineAsync(part, stock, line, dto.LocationId, tenantResult.TenantId);

                        if (!lineResult.Ok)

                        {

                            await tx.RollbackAsync();

                            response.AddError(lineResult.Error);

                            response.StatusCode = lineResult.StatusCode;

                            return response;

                        }

                        result.CreatedBatchIds.Add(lineResult.BatchId);

                        result.GeneratedSerialNumbers.AddRange(lineResult.Serials);

                        totalReceived += line.Quantity;

                    }

                    else

                    {

                        var batch = await CreateBatchAsync(stock, part, line, tenantResult.TenantId);

                        batch.TotalQuantity = line.Quantity;

                        batch.AvailableQuantity = line.Quantity;

                        await _context.SaveChangesAsync();

                        stock.TotalQuantity += line.Quantity;

                        stock.AvailableQuantity += line.Quantity;

                        result.CreatedBatchIds.Add(batch.Id);

                        totalReceived += line.Quantity;

                    }

                }



                await _context.SaveChangesAsync();



                var receiptTxnId = await CreateTransactionAsync(new TransactionCreateParams

                {

                    PartId = part.Id,

                    PartInventoryId = stock.Id,

                    Type = (int)Enums.PartTransactionType.Receipt,

                    Quantity = totalReceived,

                    ToLocationId = dto.LocationId,

                    Remarks = dto.Remarks,

                    TenantId = tenantResult.TenantId,

                    SupplierId = dto.Lines[0].SupplierId,

                    TransactionDate = dto.Lines[0].ReceivedDate

                });



                result.ReceiptTransactionId = receiptTxnId;

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

            var batch = await LoadBatchAsync(dto.PartInventoryBatchId);

            if (batch == null) { response.AddError("Batch not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }



            var part = batch.Part ?? await _context.Parts.FindAsync(batch.PartId);

            var stock = batch.PartInventory;

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId ?? stock.TenantId);

            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }



            var locError = await ValidateLocationAsync(dto.ToLocationId, tenantResult.TenantId);

            if (locError != null) { response.AddError(locError); response.StatusCode = HttpStatusCode.BadRequest; return response; }



            if (dto.ToLocationId == stock.LocationId)

            { response.AddError("Destination must differ from current location."); response.StatusCode = HttpStatusCode.BadRequest; return response; }



            await using var tx = await _context.Database.BeginTransactionAsync();

            try

            {

                decimal qty;

                List<PartSerialNumber> serials;

                if (part.IsSerialized)

                {

                    var serialResult = await ResolveSerialsAsync(batch, dto.PartSerialNumberIds, dto.Quantity,

                        (int)Enums.PartSerialStatus.InStock, stock.LocationId, part.IsSerialized);

                    if (!serialResult.Ok) { response.AddError(serialResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

                    serials = serialResult.Serials;

                    qty = serials.Count;

                }

                else

                {

                    qty = dto.Quantity;

                    if (qty <= 0 || qty > batch.AvailableQuantity)

                    { response.AddError("Invalid transfer quantity."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

                    serials = new List<PartSerialNumber>();

                }



                batch.AvailableQuantity -= qty;

                batch.TotalQuantity -= qty;

                stock.AvailableQuantity -= qty;

                stock.TotalQuantity -= qty;



                var destStock = await GetOrCreateStockAsync(part.Id, dto.ToLocationId, tenantResult.TenantId);

                var destBatch = await FindOrCreateMatchingBatchAsync(destStock, batch, tenantResult.TenantId);

                destBatch.AvailableQuantity += qty;

                destBatch.TotalQuantity += qty;

                destStock.AvailableQuantity += qty;

                destStock.TotalQuantity += qty;



                foreach (var s in serials)

                {

                    s.LocationId = dto.ToLocationId;

                    s.PartInventoryBatchId = destBatch.Id;

                }



                await _context.SaveChangesAsync();



                var txnId = await CreateTransactionAsync(new TransactionCreateParams

                {

                    PartId = part.Id,

                    PartInventoryId = stock.Id,

                    PartInventoryBatchId = batch.Id,

                    Type = (int)Enums.PartTransactionType.Transfer,

                    Quantity = qty,

                    FromLocationId = stock.LocationId,

                    ToLocationId = dto.ToLocationId,

                    Remarks = dto.Remarks,

                    TenantId = tenantResult.TenantId,

                    SerialIds = serials.Select(x => x.Id).ToList()

                });



                await tx.CommitAsync();

                response.Result = await MapStockDtoAsync(destStock);

                return response;

            }

            catch

            {

                await tx.RollbackAsync();

                throw;

            }

        }



        public async Task<ResponseDto<PartInventoryDto>> AdjustAsync(PartAdjustmentDto dto)

        {

            var response = new ResponseDto<PartInventoryDto>();

            var batch = await LoadBatchAsync(dto.PartInventoryBatchId);

            if (batch == null) { response.AddError("Batch not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }



            var part = batch.Part ?? await _context.Parts.FindAsync(batch.PartId);

            if (part.IsSerialized)

            { response.AddError("Use serial receipt or scrap operations for serialized parts; quantity adjustment is not allowed without serial handling."); response.StatusCode = HttpStatusCode.BadRequest; return response; }



            var stock = batch.PartInventory;

            var newBatchAvail = batch.AvailableQuantity + dto.QuantityChange;

            if (newBatchAvail < 0) { response.AddError("Adjustment would result in negative quantity."); response.StatusCode = HttpStatusCode.BadRequest; return response; }



            batch.AvailableQuantity = newBatchAvail;

            batch.TotalQuantity += dto.QuantityChange;

            if (batch.TotalQuantity < 0) batch.TotalQuantity = 0;



            stock.AvailableQuantity += dto.QuantityChange;

            stock.TotalQuantity += dto.QuantityChange;

            if (stock.TotalQuantity < 0) stock.TotalQuantity = 0;



            await _context.SaveChangesAsync();

            await CreateTransactionAsync(new TransactionCreateParams

            {

                PartId = part.Id,

                PartInventoryId = stock.Id,

                PartInventoryBatchId = batch.Id,

                Type = (int)Enums.PartTransactionType.Adjustment,

                Quantity = Math.Abs(dto.QuantityChange),

                FromLocationId = stock.LocationId,

                ToLocationId = stock.LocationId,

                Reason = dto.Reason,

                Remarks = dto.Remarks,

                TenantId = stock.TenantId

            });



            response.Result = await MapStockDtoAsync(stock);

            return response;

        }



        public async Task<ResponseDto<PartInventoryDto>> IssueAsync(PartIssueDto dto) =>

            await MoveAvailableToIssuedAsync(dto.PartInventoryBatchId, dto.Quantity, dto.PartSerialNumberIds,

                (int)Enums.PartTransactionType.Issue, dto.TenantId, dto.Reason, dto.Remarks,

                issuedToUserId: dto.IssuedToUserId);



        public async Task<ResponseDto<PartInventoryDto>> ReturnAsync(PartReturnDto dto)

        {

            var response = new ResponseDto<PartInventoryDto>();

            var batch = await LoadBatchAsync(dto.PartInventoryBatchId);

            if (batch == null) { response.AddError("Batch not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }



            var part = batch.Part ?? await _context.Parts.FindAsync(batch.PartId);

            var stock = batch.PartInventory;



            await using var tx = await _context.Database.BeginTransactionAsync();

            try

            {

                decimal qty;

                List<PartSerialNumber> serials;

                if (part.IsSerialized)

                {

                    var serialResult = await ResolveSerialsAsync(batch, dto.PartSerialNumberIds, dto.Quantity,

                        (int)Enums.PartSerialStatus.Issued, null, part.IsSerialized);

                    if (!serialResult.Ok) { response.AddError(serialResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

                    serials = serialResult.Serials;

                    qty = serials.Count;

                    foreach (var s in serials)

                    {

                        s.Status = (int)Enums.PartSerialStatus.InStock;

                        s.LocationId = dto.ToLocationId;

                    }

                }

                else

                {

                    qty = dto.Quantity;

                    if (qty <= 0 || qty > batch.IssuedQuantity)

                    { response.AddError("Invalid return quantity."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

                }



                batch.IssuedQuantity -= qty;

                batch.AvailableQuantity += qty;

                stock.IssuedQuantity -= qty;

                stock.AvailableQuantity += qty;



                if (dto.ToLocationId != stock.LocationId)

                {

                    var destStock = await GetOrCreateStockAsync(part.Id, dto.ToLocationId, stock.TenantId);

                    var destBatch = await FindOrCreateMatchingBatchAsync(destStock, batch, stock.TenantId);

                    destBatch.AvailableQuantity += qty;

                    destBatch.TotalQuantity += qty;

                    destStock.AvailableQuantity += qty;

                    destStock.TotalQuantity += qty;

                    batch.AvailableQuantity -= qty;

                    batch.TotalQuantity -= qty;

                    stock.AvailableQuantity -= qty;

                    stock.TotalQuantity -= qty;

                }



                await _context.SaveChangesAsync();

                await CreateTransactionAsync(new TransactionCreateParams

                {

                    PartId = part.Id,

                    PartInventoryId = stock.Id,

                    PartInventoryBatchId = batch.Id,

                    Type = (int)Enums.PartTransactionType.Return,

                    Quantity = qty,

                    FromLocationId = stock.LocationId,

                    ToLocationId = dto.ToLocationId,

                    Reason = dto.Reason,

                    Remarks = dto.Remarks,

                    TenantId = stock.TenantId,

                    ReturnedFromUserId = dto.ReturnedFromUserId,

                    SerialIds = part.IsSerialized ? dto.PartSerialNumberIds : null

                });

                await tx.CommitAsync();

                response.Result = await MapStockDtoAsync(stock);

                return response;

            }

            catch

            {

                await tx.RollbackAsync();

                throw;

            }

        }



        public async Task<ResponseDto<PartInventoryDto>> MarkFaultyAsync(PartInventoryStateChangeDto dto) =>
            await ShiftBucketAsync(dto, fromAvailable: true, toFaulty: true, txnType: (int)Enums.PartTransactionType.MarkFaulty, serialStatus: (int)Enums.PartSerialStatus.Faulty);



        public async Task<ResponseDto<PartInventoryDto>> QuarantineAsync(PartInventoryStateChangeDto dto) =>
            await ShiftBucketAsync(dto, fromAvailable: true, toQuarantine: true, txnType: (int)Enums.PartTransactionType.Quarantine, serialStatus: (int)Enums.PartSerialStatus.Quarantine);



        public async Task<ResponseDto<PartInventoryDto>> ReleaseFromQuarantineAsync(PartInventoryStateChangeDto dto) =>
            await ShiftBucketAsync(dto, fromQuarantine: true, toAvailable: true, txnType: (int)Enums.PartTransactionType.ReleaseFromQuarantine, serialStatus: (int)Enums.PartSerialStatus.InStock);



        public async Task<ResponseDto<PartInventoryDto>> ReturnToSupplierAsync(PartReturnToSupplierDto dto)

        {

            var response = new ResponseDto<PartInventoryDto>();

            var batch = await LoadBatchAsync(dto.PartInventoryBatchId);

            if (batch == null) { response.AddError("Batch not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }



            var supErr = await ValidateSupplierAsync(dto.SupplierId, batch.TenantId);

            if (supErr != null) { response.AddError(supErr); response.StatusCode = HttpStatusCode.BadRequest; return response; }



            var part = batch.Part ?? await _context.Parts.FindAsync(batch.PartId);

            var stock = batch.PartInventory;



            await using var tx = await _context.Database.BeginTransactionAsync();

            try

            {

                decimal qty;

                List<PartSerialNumber> serials;

                if (part.IsSerialized)

                {

                    var allowed = new[] { (int)Enums.PartSerialStatus.Faulty, (int)Enums.PartSerialStatus.InStock, (int)Enums.PartSerialStatus.Quarantine };

                    var serialResult = await ResolveSerialsAsync(batch, dto.PartSerialNumberIds, dto.Quantity, null, stock.LocationId, part.IsSerialized, allowed);

                    if (!serialResult.Ok) { response.AddError(serialResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

                    serials = serialResult.Serials;

                    qty = serials.Count;

                    foreach (var s in serials)
                    {
                        var prevStatus = s.Status;
                        DecrementSerialBucket(batch, stock, prevStatus);
                        s.Status = (int)Enums.PartSerialStatus.ReturnedToSupplier;
                    }
                    batch.TotalQuantity -= qty;
                    stock.TotalQuantity -= qty;
                }
                else
                {
                    qty = dto.Quantity;
                    if (qty <= 0 || qty > batch.AvailableQuantity + batch.FaultyQuantity + batch.QuarantineQuantity)
                    { response.AddError("Invalid return-to-supplier quantity."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
                    ReduceFromBuckets(batch, stock, qty);
                    batch.TotalQuantity -= qty;
                    stock.TotalQuantity -= qty;
                }



                await _context.SaveChangesAsync();

                await CreateTransactionAsync(new TransactionCreateParams

                {

                    PartId = part.Id,

                    PartInventoryId = stock.Id,

                    PartInventoryBatchId = batch.Id,

                    Type = (int)Enums.PartTransactionType.ReturnToSupplier,

                    Quantity = qty,

                    FromLocationId = stock.LocationId,

                    Reason = dto.Reason,

                    Remarks = dto.Remarks,

                    TenantId = stock.TenantId,

                    SupplierId = dto.SupplierId,

                    SerialIds = part.IsSerialized ? dto.PartSerialNumberIds : null

                });

                await tx.CommitAsync();

                response.Result = await MapStockDtoAsync(stock);

                return response;

            }

            catch

            {

                await tx.RollbackAsync();

                throw;

            }

        }



        public async Task<ResponseDto<PartInventoryDto>> ScrapAsync(PartInventoryStateChangeDto dto) =>

            await RemoveFromStockAsync(dto, (int)Enums.PartTransactionType.Scrap, (int)Enums.PartSerialStatus.Scrapped);



        public async Task<ResponseDto<bool>> DeleteAsync(int id)

        {

            var response = new ResponseDto<bool>();

            var entity = await _context.PartInventories.FindAsync(id);

            if (entity == null) { response.AddError("Inventory not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            if (entity.TotalQuantity > 0)

            { response.AddError("Cannot delete inventory with remaining stock. Use scrap or adjustment operations."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            entity.IsDeleted = true;

            entity.IsActive = false;

            await _context.SaveChangesAsync();

            response.Result = true;

            return response;

        }



        #region Internal operations



        private async Task<ResponseDto<PartInventoryDto>> MoveAvailableToIssuedAsync(

            int batchId, decimal quantity, List<int> serialIds, int txnType, int? tenantId, string reason, string remarks, int? issuedToUserId)

        {

            var response = new ResponseDto<PartInventoryDto>();

            var batch = await LoadBatchAsync(batchId);

            if (batch == null) { response.AddError("Batch not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }



            var part = batch.Part ?? await _context.Parts.FindAsync(batch.PartId);

            var stock = batch.PartInventory;



            await using var tx = await _context.Database.BeginTransactionAsync();

            try

            {

                decimal qty;

                List<PartSerialNumber> serials;

                if (part.IsSerialized)

                {

                    var serialResult = await ResolveSerialsAsync(batch, serialIds, quantity,

                        (int)Enums.PartSerialStatus.InStock, stock.LocationId, part.IsSerialized);

                    if (!serialResult.Ok) { response.AddError(serialResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

                    serials = serialResult.Serials;

                    qty = serials.Count;

                    foreach (var s in serials)

                        s.Status = (int)Enums.PartSerialStatus.Issued;

                }

                else

                {

                    qty = quantity;

                    if (qty <= 0 || qty > batch.AvailableQuantity)

                    { response.AddError("Insufficient available quantity."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

                }



                batch.AvailableQuantity -= qty;

                batch.IssuedQuantity += qty;

                stock.AvailableQuantity -= qty;

                stock.IssuedQuantity += qty;



                await _context.SaveChangesAsync();

                await CreateTransactionAsync(new TransactionCreateParams

                {

                    PartId = part.Id,

                    PartInventoryId = stock.Id,

                    PartInventoryBatchId = batch.Id,

                    Type = txnType,

                    Quantity = qty,

                    FromLocationId = stock.LocationId,

                    Reason = reason,

                    Remarks = remarks,

                    TenantId = stock.TenantId,

                    IssuedToUserId = issuedToUserId,

                    SerialIds = part.IsSerialized ? serialIds : null

                });

                await tx.CommitAsync();

                response.Result = await MapStockDtoAsync(stock);

                return response;

            }

            catch

            {

                await tx.RollbackAsync();

                throw;

            }

        }



        private async Task<ResponseDto<PartInventoryDto>> ShiftBucketAsync(

            PartInventoryStateChangeDto dto, bool fromAvailable = false, bool fromQuarantine = false,

            bool toAvailable = false, bool toFaulty = false, bool toQuarantine = false,

            int txnType = 0, int serialStatus = 0)

        {

            var response = new ResponseDto<PartInventoryDto>();

            var batch = await LoadBatchAsync(dto.PartInventoryBatchId);

            if (batch == null) { response.AddError("Batch not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }



            var part = batch.Part ?? await _context.Parts.FindAsync(batch.PartId);

            var stock = batch.PartInventory;



            await using var tx = await _context.Database.BeginTransactionAsync();

            try

            {

                decimal qty;

                List<PartSerialNumber> serials = new();

                if (part.IsSerialized)

                {

                    int requiredStatus = fromQuarantine ? (int)Enums.PartSerialStatus.Quarantine : (int)Enums.PartSerialStatus.InStock;

                    var serialResult = await ResolveSerialsAsync(batch, dto.PartSerialNumberIds, dto.Quantity, requiredStatus, stock.LocationId, part.IsSerialized);

                    if (!serialResult.Ok) { response.AddError(serialResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

                    serials = serialResult.Serials;

                    qty = serials.Count;

                    foreach (var s in serials) s.Status = serialStatus;

                }

                else

                {

                    qty = dto.Quantity;

                    if (qty <= 0) { response.AddError("Quantity must be greater than zero."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

                    if (fromAvailable && qty > batch.AvailableQuantity) { response.AddError("Insufficient available quantity."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

                    if (fromQuarantine && qty > batch.QuarantineQuantity) { response.AddError("Insufficient quarantine quantity."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

                }



                if (fromAvailable) { batch.AvailableQuantity -= qty; stock.AvailableQuantity -= qty; }

                if (fromQuarantine) { batch.QuarantineQuantity -= qty; stock.QuarantineQuantity -= qty; }

                if (toAvailable) { batch.AvailableQuantity += qty; stock.AvailableQuantity += qty; }

                if (toFaulty) { batch.FaultyQuantity += qty; stock.FaultyQuantity += qty; }

                if (toQuarantine) { batch.QuarantineQuantity += qty; stock.QuarantineQuantity += qty; }



                await _context.SaveChangesAsync();

                await CreateTransactionAsync(new TransactionCreateParams

                {

                    PartId = part.Id,

                    PartInventoryId = stock.Id,

                    PartInventoryBatchId = batch.Id,

                    Type = txnType,

                    Quantity = qty,

                    FromLocationId = stock.LocationId,

                    Reason = dto.Reason,

                    Remarks = dto.Remarks,

                    TenantId = stock.TenantId,

                    SerialIds = part.IsSerialized ? dto.PartSerialNumberIds : null

                });

                await tx.CommitAsync();

                response.Result = await MapStockDtoAsync(stock);

                return response;

            }

            catch

            {

                await tx.RollbackAsync();

                throw;

            }

        }



        private async Task<ResponseDto<PartInventoryDto>> RemoveFromStockAsync(PartInventoryStateChangeDto dto, int txnType, int serialStatus)

        {

            var response = new ResponseDto<PartInventoryDto>();

            var batch = await LoadBatchAsync(dto.PartInventoryBatchId);

            if (batch == null) { response.AddError("Batch not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }



            var part = batch.Part ?? await _context.Parts.FindAsync(batch.PartId);

            var stock = batch.PartInventory;



            await using var tx = await _context.Database.BeginTransactionAsync();

            try

            {

                decimal qty;

                if (part.IsSerialized)

                {

                    var allowed = new[] { (int)Enums.PartSerialStatus.InStock, (int)Enums.PartSerialStatus.Faulty, (int)Enums.PartSerialStatus.Quarantine, (int)Enums.PartSerialStatus.Issued };

                    var serialResult = await ResolveSerialsAsync(batch, dto.PartSerialNumberIds, dto.Quantity, null, stock.LocationId, part.IsSerialized, allowed);

                    if (!serialResult.Ok) { response.AddError(serialResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

                    qty = serialResult.Serials.Count;
                    foreach (var s in serialResult.Serials)
                    {
                        DecrementSerialBucket(batch, stock, s.Status);
                        s.Status = serialStatus;
                    }
                    batch.TotalQuantity -= qty;
                    stock.TotalQuantity -= qty;
                }
                else
                {
                    qty = dto.Quantity;
                    if (qty <= 0 || qty > batch.AvailableQuantity + batch.FaultyQuantity + batch.QuarantineQuantity)
                    { response.AddError("Invalid scrap quantity."); response.StatusCode = HttpStatusCode.BadRequest; return response; }
                    ReduceFromBuckets(batch, stock, qty);
                    batch.TotalQuantity -= qty;
                    stock.TotalQuantity -= qty;
                }



                await _context.SaveChangesAsync();

                await CreateTransactionAsync(new TransactionCreateParams

                {

                    PartId = part.Id,

                    PartInventoryId = stock.Id,

                    PartInventoryBatchId = batch.Id,

                    Type = txnType,

                    Quantity = qty,

                    FromLocationId = stock.LocationId,

                    Reason = dto.Reason,

                    Remarks = dto.Remarks,

                    TenantId = stock.TenantId,

                    SerialIds = part.IsSerialized ? dto.PartSerialNumberIds : null

                });

                await tx.CommitAsync();

                response.Result = await MapStockDtoAsync(stock);

                return response;

            }

            catch

            {

                await tx.RollbackAsync();

                throw;

            }

        }



        private static void DecrementSerialBucket(PartInventoryBatch batch, PartInventory stock, int serialStatus)
        {
            switch (serialStatus)
            {
                case (int)Enums.PartSerialStatus.InStock:
                    batch.AvailableQuantity = Math.Max(0, batch.AvailableQuantity - 1);
                    stock.AvailableQuantity = Math.Max(0, stock.AvailableQuantity - 1);
                    break;
                case (int)Enums.PartSerialStatus.Issued:
                    batch.IssuedQuantity = Math.Max(0, batch.IssuedQuantity - 1);
                    stock.IssuedQuantity = Math.Max(0, stock.IssuedQuantity - 1);
                    break;
                case (int)Enums.PartSerialStatus.Faulty:
                    batch.FaultyQuantity = Math.Max(0, batch.FaultyQuantity - 1);
                    stock.FaultyQuantity = Math.Max(0, stock.FaultyQuantity - 1);
                    break;
                case (int)Enums.PartSerialStatus.Quarantine:
                    batch.QuarantineQuantity = Math.Max(0, batch.QuarantineQuantity - 1);
                    stock.QuarantineQuantity = Math.Max(0, stock.QuarantineQuantity - 1);
                    break;
            }
        }

        private static void ReduceFromBuckets(PartInventoryBatch batch, PartInventory stock, decimal qty)
        {
            var rem = qty;
            var fromAvail = Math.Min(batch.AvailableQuantity, rem);
            batch.AvailableQuantity -= fromAvail;
            stock.AvailableQuantity -= fromAvail;
            rem -= fromAvail;
            var fromFaulty = Math.Min(batch.FaultyQuantity, rem);
            batch.FaultyQuantity -= fromFaulty;
            stock.FaultyQuantity -= fromFaulty;
            rem -= fromFaulty;
            batch.QuarantineQuantity -= rem;
            stock.QuarantineQuantity -= rem;
        }



        private async Task<(bool Ok, string Error, HttpStatusCode StatusCode, int BatchId, List<string> Serials)> ReceiveSerializedLineAsync(

            Part part, PartInventory stock, PartReceiveLineDto line, int locationId, int? tenantId)

        {

            const int scanMode = 1;

            var supplierRefs = line.SupplierSerialReferences?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList() ?? new();

            if (line.ReceiptMode == scanMode)

            {

                if (supplierRefs.Count != line.Quantity)

                    return (false, "Enter or scan one supplier serial per unit.", HttpStatusCode.BadRequest, 0, new List<string>());

                if (supplierRefs.Distinct(StringComparer.OrdinalIgnoreCase).Count() != supplierRefs.Count)

                    return (false, "Duplicate supplier serial references.", HttpStatusCode.BadRequest, 0, new List<string>());

            }

            else if (supplierRefs.Count > 0)

                return (false, "Supplier serial references are only used in scan mode.", HttpStatusCode.BadRequest, 0, new List<string>());



            var batch = await CreateBatchAsync(stock, part, line, tenantId);

            var received = line.ReceivedDate ?? DateTime.UtcNow;

            var internalSerials = await PartSerialNumberGenerator.GetNextSerialsAsync(_context, part.Id, tenantId, line.Quantity);

            var created = new List<string>();



            for (var i = 0; i < line.Quantity; i++)

            {

                var internalSerial = internalSerials[i];

                if (await PartSerialNumberGenerator.SerialExistsAsync(_context, tenantId, internalSerial))

                    return (false, $"Internal serial conflict: {internalSerial}.", HttpStatusCode.Conflict, 0, new List<string>());



                var supplierRef = line.ReceiptMode == scanMode ? supplierRefs[i] : internalSerial;

                var serialEntity = new PartSerialNumber

                {

                    TenantId = tenantId,

                    PartId = part.Id,

                    PartInventoryBatchId = batch.Id,

                    OriginPartInventoryBatchId = batch.Id,

                    SerialNumber = internalSerial,

                    SupplierId = line.SupplierId,

                    SupplierSerialReference = supplierRef,

                    Status = (int)Enums.PartSerialStatus.InStock,

                    ReceivedDate = received,

                    LocationId = locationId,

                    ExpiryDate = line.ExpiryDate,

                    ExpectedLifeValue = line.ExpectedLifeValue ?? part.ExpectedLifeValue,

                    ExpectedLifeUnit = line.ExpectedLifeUnit ?? part.ExpectedLifeUnit,

                    WarrantyStartDate = line.WarrantyStartDate,

                    WarrantyEndDate = line.WarrantyEndDate,

                    IsActive = true

                };

                _context.PartSerialNumbers.Add(serialEntity);

                created.Add(internalSerial);

            }



            batch.TotalQuantity = line.Quantity;

            batch.AvailableQuantity = line.Quantity;

            stock.TotalQuantity += line.Quantity;

            stock.AvailableQuantity += line.Quantity;

            await _context.SaveChangesAsync();

            return (true, null, HttpStatusCode.OK, batch.Id, created);

        }



        private async Task<PartInventoryBatch> CreateBatchAsync(PartInventory stock, Part part, PartReceiveLineDto line, int? tenantId)

        {

            var batchRef = await PartInventoryBatchReferenceGenerator.GetNextReferenceAsync(_context, tenantId);

            var batch = new PartInventoryBatch

            {

                TenantId = tenantId,

                PartInventoryId = stock.Id,

                PartId = part.Id,

                BatchReference = batchRef,

                SupplierId = line.SupplierId,

                ReceivedDate = line.ReceivedDate ?? DateTime.UtcNow,

                ExpiryDate = line.ExpiryDate,

                ExpectedLifeValue = line.ExpectedLifeValue ?? part.ExpectedLifeValue,

                ExpectedLifeUnit = line.ExpectedLifeUnit ?? part.ExpectedLifeUnit,

                Notes = line.Notes ?? string.Empty,

                IsActive = true

            };

            _context.PartInventoryBatches.Add(batch);

            await _context.SaveChangesAsync();

            return batch;

        }



        private async Task<PartInventory> GetOrCreateStockAsync(int partId, int locationId, int? tenantId)

        {

            var stock = await _context.PartInventories.FirstOrDefaultAsync(x =>

                x.PartId == partId && x.LocationId == locationId && x.IsActive && !x.IsDeleted);

            if (stock != null) return stock;



            stock = new PartInventory

            {

                TenantId = tenantId,

                PartId = partId,

                LocationId = locationId,

                IsActive = true

            };

            _context.PartInventories.Add(stock);

            await _context.SaveChangesAsync();

            return stock;

        }



        private async Task<PartInventoryBatch> FindOrCreateMatchingBatchAsync(PartInventory destStock, PartInventoryBatch source, int? tenantId)

        {

            var match = await _context.PartInventoryBatches.FirstOrDefaultAsync(x =>

                x.PartInventoryId == destStock.Id &&

                x.SupplierId == source.SupplierId &&

                x.ExpiryDate == source.ExpiryDate &&

                x.ReceivedDate == source.ReceivedDate &&

                x.IsActive);

            if (match != null) return match;



            var batchRef = await PartInventoryBatchReferenceGenerator.GetNextReferenceAsync(_context, tenantId);

            match = new PartInventoryBatch

            {

                TenantId = tenantId,

                PartInventoryId = destStock.Id,

                PartId = source.PartId,

                BatchReference = batchRef,

                SupplierId = source.SupplierId,

                ReceivedDate = source.ReceivedDate,

                ExpiryDate = source.ExpiryDate,

                ExpectedLifeValue = source.ExpectedLifeValue,

                ExpectedLifeUnit = source.ExpectedLifeUnit,

                Notes = source.Notes ?? string.Empty,

                IsActive = true

            };

            _context.PartInventoryBatches.Add(match);

            await _context.SaveChangesAsync();

            return match;

        }



        private async Task<PartInventoryBatch?> LoadBatchAsync(int batchId) =>

            await _context.PartInventoryBatches

                .Include(x => x.PartInventory)

                .Include(x => x.Part)

                .FirstOrDefaultAsync(x => x.Id == batchId);



        private async Task<(bool Ok, string Error, List<PartSerialNumber> Serials)> ResolveSerialsAsync(

            PartInventoryBatch batch, List<int> serialIds, decimal quantity, int? requiredStatus, int? requiredLocationId, bool isSerialized, int[]? allowedStatuses = null)

        {

            if (!isSerialized)

                return (true, null, new List<PartSerialNumber>());



            if (serialIds == null || serialIds.Count == 0)

                return (false, "Select serial number(s) for this operation.", new List<PartSerialNumber>());



            if (quantity > 0 && serialIds.Count != (int)quantity)

                return (false, "Serial count must match quantity.", new List<PartSerialNumber>());



            var serials = await _context.PartSerialNumbers

                .Where(x => serialIds.Contains(x.Id) && x.PartInventoryBatchId == batch.Id && x.IsActive)

                .ToListAsync();



            if (serials.Count != serialIds.Count)

                return (false, "One or more serials are invalid for this batch.", new List<PartSerialNumber>());



            foreach (var s in serials)

            {

                if (requiredStatus.HasValue && s.Status != requiredStatus.Value)

                    return (false, $"Serial {s.SerialNumber} is not in the required status.", new List<PartSerialNumber>());

                if (allowedStatuses != null && !allowedStatuses.Contains(s.Status))

                    return (false, $"Serial {s.SerialNumber} cannot be used for this operation.", new List<PartSerialNumber>());

                if (requiredLocationId.HasValue && s.LocationId != requiredLocationId)

                    return (false, $"Serial {s.SerialNumber} is not at the expected location.", new List<PartSerialNumber>());

            }



            return (true, null, serials);

        }



        private class TransactionCreateParams

        {

            public int PartId { get; set; }

            public int? PartInventoryId { get; set; }

            public int? PartInventoryBatchId { get; set; }

            public int Type { get; set; }

            public decimal Quantity { get; set; }

            public int? FromLocationId { get; set; }

            public int? ToLocationId { get; set; }

            public int? TenantId { get; set; }

            public int? SupplierId { get; set; }

            public int? IssuedToUserId { get; set; }

            public int? ReturnedFromUserId { get; set; }

            public string Reason { get; set; }

            public string Remarks { get; set; }

            public DateTime? TransactionDate { get; set; }

            public List<int> SerialIds { get; set; }

        }



        private async Task<int> CreateTransactionAsync(TransactionCreateParams p)

        {

            var entity = new PartTransaction

            {

                TenantId = p.TenantId,

                PartId = p.PartId,

                PartInventoryId = p.PartInventoryId,

                PartInventoryBatchId = p.PartInventoryBatchId,

                TransactionType = p.Type,

                Quantity = p.Quantity,

                FromLocationId = p.FromLocationId,

                ToLocationId = p.ToLocationId,

                TransactionDate = p.TransactionDate ?? DateTime.UtcNow,

                PerformedByUserId = UserHelper.GetCurrentUserId(_httpContextAccessor),

                IssuedToUserId = p.IssuedToUserId,

                ReturnedFromUserId = p.ReturnedFromUserId,

                Reason = p.Reason ?? string.Empty,

                Remarks = p.Remarks ?? string.Empty,

                SupplierId = p.SupplierId,

                IsActive = true

            };

            _context.PartTransactions.Add(entity);

            await _context.SaveChangesAsync();



            if (p.SerialIds != null)

            {

                foreach (var sid in p.SerialIds.Distinct())

                {

                    _context.PartTransactionSerials.Add(new PartTransactionSerial

                    {

                        PartTransactionId = entity.Id,

                        PartSerialNumberId = sid,

                        Quantity = 1,

                        IsActive = true

                    });

                }

                await _context.SaveChangesAsync();

            }



            return entity.Id;

        }



        private IQueryable<PartInventory> BuildStockQuery() =>

            _context.PartInventories

                .Include(x => x.Part)

                .Include(x => x.Location)

                .Include(x => x.Tenant)

                .Include(x => x.Batches);



        private async Task<PartInventoryDto> MapStockDtoAsync(PartInventory inv)

        {

            var earliest = inv.Batches?

                .Where(b => b.IsActive && !b.IsDeleted && b.AvailableQuantity > 0 && b.ExpiryDate.HasValue)

                .Select(b => b.ExpiryDate)

                .OrderBy(d => d)

                .FirstOrDefault();



            if (earliest == null)

            {

                earliest = await _context.PartInventoryBatches

                    .Where(b => b.PartInventoryId == inv.Id && b.IsActive && !b.IsDeleted && b.AvailableQuantity > 0 && b.ExpiryDate.HasValue)

                    .OrderBy(b => b.ExpiryDate)

                    .Select(b => b.ExpiryDate)

                    .FirstOrDefaultAsync();

            }



            var part = inv.Part ?? await _context.Parts.FindAsync(inv.PartId);

            return new PartInventoryDto

            {

                Id = inv.Id,

                TenantId = inv.TenantId,

                TenantName = inv.Tenant?.CompanyName,

                PartId = inv.PartId,

                PartNumber = part?.PartNumber,

                PartName = part?.PartName,

                PartIsSerialized = part?.IsSerialized ?? false,

                LocationId = inv.LocationId,

                LocationName = inv.Location?.Name,

                TotalQuantity = inv.TotalQuantity,

                AvailableQuantity = inv.AvailableQuantity,

                FaultyQuantity = inv.FaultyQuantity,

                QuarantineQuantity = inv.QuarantineQuantity,

                IssuedQuantity = inv.IssuedQuantity,

                EarliestExpiryDate = earliest,

                IsLowStock = part != null && inv.AvailableQuantity < part.MinStockLevel,

                IsActive = inv.IsActive

            };

        }



        private async Task<PartInventoryDetailDto?> MapDetailAsync(int id)

        {

            var inv = await BuildStockQuery().FirstOrDefaultAsync(x => x.Id == id);

            if (inv == null) return null;



            var baseDto = await MapStockDtoAsync(inv);

            var detail = new PartInventoryDetailDto

            {

                Id = baseDto.Id,

                TenantId = baseDto.TenantId,

                TenantName = baseDto.TenantName,

                PartId = baseDto.PartId,

                PartNumber = baseDto.PartNumber,

                PartName = baseDto.PartName,

                PartIsSerialized = baseDto.PartIsSerialized,

                LocationId = baseDto.LocationId,

                LocationName = baseDto.LocationName,

                TotalQuantity = baseDto.TotalQuantity,

                AvailableQuantity = baseDto.AvailableQuantity,

                FaultyQuantity = baseDto.FaultyQuantity,

                QuarantineQuantity = baseDto.QuarantineQuantity,

                IssuedQuantity = baseDto.IssuedQuantity,

                EarliestExpiryDate = baseDto.EarliestExpiryDate,

                IsLowStock = baseDto.IsLowStock,

                IsActive = baseDto.IsActive,

                Batches = await _context.PartInventoryBatches

                    .Include(b => b.Supplier)

                    .Where(b => b.PartInventoryId == id && b.IsActive && !b.IsDeleted)

                    .OrderBy(b => b.ExpiryDate)

                    .Select(b => new PartInventoryBatchDto

                    {

                        Id = b.Id,

                        PartInventoryId = b.PartInventoryId,

                        PartId = b.PartId,

                        BatchReference = b.BatchReference,

                        SupplierId = b.SupplierId,

                        SupplierName = b.Supplier != null ? b.Supplier.Name : null,

                        ReceivedDate = b.ReceivedDate,

                        ExpiryDate = b.ExpiryDate,

                        ExpectedLifeValue = b.ExpectedLifeValue,

                        ExpectedLifeUnit = b.ExpectedLifeUnit,

                        TotalQuantity = b.TotalQuantity,

                        AvailableQuantity = b.AvailableQuantity,

                        FaultyQuantity = b.FaultyQuantity,

                        QuarantineQuantity = b.QuarantineQuantity,

                        IssuedQuantity = b.IssuedQuantity,

                        Notes = b.Notes,

                        IsActive = b.IsActive

                    }).ToListAsync()

            };

            return detail;

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



        #endregion

    }

}


