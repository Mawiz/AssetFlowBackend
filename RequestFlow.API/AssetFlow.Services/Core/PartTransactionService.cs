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

        public PartTransactionService(ApplicationDbContext context, ITenantProvider tenantProvider, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseDto<PartTransactionDto>> CreateAsync(CreatePartTransactionDto dto)
        {
            var response = new ResponseDto<PartTransactionDto>();
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
                PartSerialNumberId = null,
                TransactionType = dto.TransactionType,
                Quantity = dto.Quantity,
                FromLocationId = dto.FromLocationId,
                ToLocationId = dto.ToLocationId,
                TransactionDate = dto.TransactionDate,
                PerformedByUserId = UserHelper.GetCurrentUserId(_httpContextAccessor),
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
                (!model.TransactionType.HasValue || x.TransactionType == model.TransactionType) &&
                (!model.IsActive.HasValue || x.IsActive == model.IsActive));

            var projected = query.Select(ProjectToDto());
            model.OrderByProp ??= nameof(PartTransaction.TransactionDate);
            var ordered = model.SortDirection == (int)Enums.OrderBy.Ascending
                ? projected.OrderBy(model.OrderByProp)
                : projected.OrderBy($"{model.OrderByProp} descending");

            var paged = await ordered.ToPagedListAsync(model.PageNumber, model.PageSize);
            response.Result = paged.ToList();
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
                .Include(x => x.PartSerialNumber)
                .Include(x => x.FromLocation)
                .Include(x => x.ToLocation)
                .Include(x => x.PerformedByUser)
                .Include(x => x.Tenant)
                .Include(x => x.Supplier);

        private static System.Linq.Expressions.Expression<Func<PartTransaction, PartTransactionDto>> ProjectToDto() =>
            x => new PartTransactionDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,
                PartId = x.PartId,
                PartNumber = x.Part.PartNumber,
                PartName = x.Part.PartName,
                PartSerialNumberId = null,
                SerialNumber = null,
                TransactionType = x.TransactionType,
                Quantity = x.Quantity,
                FromLocationId = x.FromLocationId,
                FromLocationName = x.FromLocation != null ? x.FromLocation.Name : null,
                ToLocationId = x.ToLocationId,
                ToLocationName = x.ToLocation != null ? x.ToLocation.Name : null,
                TransactionDate = x.TransactionDate,
                PerformedByUserId = x.PerformedByUserId,
                PerformedByUserName = x.PerformedByUser != null ? (x.PerformedByUser.FullName ?? x.PerformedByUser.UserName) : null,
                Remarks = x.Remarks,
                SupplierId = x.SupplierId,
                SupplierName = x.Supplier != null ? x.Supplier.Name : null,
                IsActive = x.IsActive
            };

        private async Task<PartTransactionDto?> MapToDtoAsync(int id) =>
            await BuildQuery().Where(x => x.Id == id).Select(ProjectToDto()).FirstOrDefaultAsync();
    }
}
