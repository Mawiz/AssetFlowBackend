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

    public class PartTransactionService : IPartTransactionService

    {

        private readonly ApplicationDbContext _context;

        private readonly ITenantProvider _tenantProvider;

        private readonly IHttpContextAccessor _httpContextAccessor;



        private static readonly int[] OperationalTypes =

        {

            (int)Enums.PartTransactionType.Receipt,

            (int)Enums.PartTransactionType.Adjustment,

            (int)Enums.PartTransactionType.Transfer,

            (int)Enums.PartTransactionType.Issue,

            (int)Enums.PartTransactionType.Return,

            (int)Enums.PartTransactionType.MarkFaulty,

            (int)Enums.PartTransactionType.Quarantine,

            (int)Enums.PartTransactionType.ReleaseFromQuarantine,

            (int)Enums.PartTransactionType.ReturnToSupplier,

            (int)Enums.PartTransactionType.Scrap

        };



        public PartTransactionService(ApplicationDbContext context, ITenantProvider tenantProvider, IHttpContextAccessor httpContextAccessor)

        {

            _context = context;

            _tenantProvider = tenantProvider;

            _httpContextAccessor = httpContextAccessor;

        }



        public async Task<ResponseDto<PartTransactionDto>> CreateAsync(CreatePartTransactionDto dto)

        {

            var response = new ResponseDto<PartTransactionDto>();

            if (OperationalTypes.Contains(dto.TransactionType))

            {

                response.AddError("Use the dedicated inventory operation endpoints for this transaction type.");

                response.StatusCode = HttpStatusCode.BadRequest;

                return response;

            }



            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);

            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }



            var part = await _context.Parts.FindAsync(dto.PartId);

            if (part == null || part.TenantId != tenantResult.TenantId)

            { response.AddError("Part not found."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            if (dto.Quantity <= 0) { response.AddError("Quantity must be greater than zero."); response.StatusCode = HttpStatusCode.BadRequest; return response; }



            var entity = new PartTransaction

            {

                TenantId = tenantResult.TenantId,

                PartId = dto.PartId,

                TransactionType = dto.TransactionType,

                Quantity = dto.Quantity,

                FromLocationId = dto.FromLocationId,

                ToLocationId = dto.ToLocationId,

                TransactionDate = dto.TransactionDate,

                PerformedByUserId = UserHelper.GetCurrentUserId(_httpContextAccessor),

                Reason = dto.Reason,

                Remarks = dto.Remarks,

                IsActive = true

            };

            _context.PartTransactions.Add(entity);

            await _context.SaveChangesAsync();

            response.Result = await MapToDtoAsync(entity.Id);

            return response;

        }



        public async Task<ResponseDto<List<PartTransactionDto>>> FilterAsync(PartTransactionFilterDto model)

        {

            var response = new ResponseDto<List<PartTransactionDto>>();

            IQueryable<PartTransaction> query = BuildQuery();

            query = TenantScopeHelper.ApplyTenantScope(query, _tenantProvider, model.TenantId);



            query = query.Where(x =>

                (!model.PartId.HasValue || x.PartId == model.PartId) &&

                (!model.PartInventoryId.HasValue || x.PartInventoryId == model.PartInventoryId) &&

                (!model.TransactionType.HasValue || x.TransactionType == model.TransactionType) &&

                (!model.IsActive.HasValue || x.IsActive == model.IsActive));



            var paged = await query

                .OrderByDescending(x => x.TransactionDate)

                .ToPagedListAsync(model.PageNumber, model.PageSize);



            var list = new List<PartTransactionDto>();

            foreach (var item in paged)

                list.Add(await MapEntityToDtoAsync(item));



            response.Result = list;

            return response;

        }



        public async Task<ResponseDto<bool>> DeleteAsync(int id)

        {

            var response = new ResponseDto<bool>();

            var entity = await _context.PartTransactions.FindAsync(id);

            if (entity == null) { response.AddError("Transaction not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            entity.IsDeleted = true;

            entity.IsActive = false;

            await _context.SaveChangesAsync();

            response.Result = true;

            return response;

        }



        private IQueryable<PartTransaction> BuildQuery() =>

            _context.PartTransactions

                .Include(x => x.Part)

                .Include(x => x.PartInventoryBatch)

                .Include(x => x.FromLocation)

                .Include(x => x.ToLocation)

                .Include(x => x.PerformedByUser)

                .Include(x => x.IssuedToUser)

                .Include(x => x.ReturnedFromUser)

                .Include(x => x.Tenant)

                .Include(x => x.Supplier)

                .Include(x => x.TransactionSerials).ThenInclude(ts => ts.PartSerialNumber);



        private async Task<PartTransactionDto?> MapToDtoAsync(int id)

        {

            var entity = await BuildQuery().FirstOrDefaultAsync(x => x.Id == id);

            return entity == null ? null : await MapEntityToDtoAsync(entity);

        }



        private static Task<PartTransactionDto> MapEntityToDtoAsync(PartTransaction x)

        {

            var dto = new PartTransactionDto

            {

                Id = x.Id,

                TenantId = x.TenantId,

                TenantName = x.Tenant?.CompanyName,

                PartId = x.PartId,

                PartNumber = x.Part?.PartNumber,

                PartName = x.Part?.PartName,

                PartInventoryId = x.PartInventoryId,

                PartInventoryBatchId = x.PartInventoryBatchId,

                BatchReference = x.PartInventoryBatch?.BatchReference,

                TransactionType = x.TransactionType,

                Quantity = x.Quantity,

                FromLocationId = x.FromLocationId,

                FromLocationName = x.FromLocation?.Name,

                ToLocationId = x.ToLocationId,

                ToLocationName = x.ToLocation?.Name,

                TransactionDate = x.TransactionDate,

                PerformedByUserId = x.PerformedByUserId,

                PerformedByUserName = x.PerformedByUser != null ? (x.PerformedByUser.FullName ?? x.PerformedByUser.UserName) : null,

                IssuedToUserId = x.IssuedToUserId,

                IssuedToUserName = x.IssuedToUser != null ? (x.IssuedToUser.FullName ?? x.IssuedToUser.UserName) : null,

                ReturnedFromUserId = x.ReturnedFromUserId,

                ReturnedFromUserName = x.ReturnedFromUser != null ? (x.ReturnedFromUser.FullName ?? x.ReturnedFromUser.UserName) : null,

                Reason = x.Reason,

                Remarks = x.Remarks,

                SupplierId = x.SupplierId,

                SupplierName = x.Supplier?.Name,

                IsActive = x.IsActive,

                SerialNumbers = x.TransactionSerials?

                    .Where(ts => ts.PartSerialNumber != null)

                    .Select(ts => ts.PartSerialNumber.SerialNumber)

                    .ToList() ?? new List<string>()

            };

            return Task.FromResult(dto);

        }

    }

}


