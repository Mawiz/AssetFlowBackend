using Microsoft.EntityFrameworkCore;
using AssetFlow.Common.Enum;
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
    public class PartService : IPartService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public PartService(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<ResponseDto<PartDto>> CreateAsync(CreatePartDto dto)
        {
            var response = new ResponseDto<PartDto>();
            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var validation = await ValidatePartAsync(dto, tenantResult.TenantId, null);
            if (validation != null) { response.AddError(validation); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var partNumber = dto.PartNumber.Trim();
            if (await _context.Parts.AnyAsync(x => x.TenantId == tenantResult.TenantId && x.PartNumber == partNumber))
            {
                response.AddError("Part number already exists."); response.StatusCode = HttpStatusCode.Conflict; return response;
            }

            var entity = MapToEntity(new Part(), dto, tenantResult.TenantId);
            _context.Parts.Add(entity);
            await _context.SaveChangesAsync();
            response.Result = await MapToDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<PartDto>> UpdateAsync(UpdatePartDto dto)
        {
            var response = new ResponseDto<PartDto>();
            var entity = await _context.Parts.FindAsync(dto.Id);
            if (entity == null) { response.AddError("Part not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var validation = await ValidatePartAsync(dto, tenantResult.TenantId, dto.Id);
            if (validation != null) { response.AddError(validation); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var partNumber = dto.PartNumber.Trim();
            if (await _context.Parts.AnyAsync(x => x.TenantId == tenantResult.TenantId && x.PartNumber == partNumber && x.Id != dto.Id))
            {
                response.AddError("Part number already exists."); response.StatusCode = HttpStatusCode.Conflict; return response;
            }

            MapToEntity(entity, dto, tenantResult.TenantId);
            _context.Parts.Update(entity);
            await _context.SaveChangesAsync();
            response.Result = await MapToDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<PartDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<PartDto>();
            var dto = await MapToDtoAsync(id);
            if (dto == null) { response.AddError("Part not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<List<PartDto>>> FilterAsync(PartFilterDto model)
        {
            var response = new ResponseDto<List<PartDto>>();
            IQueryable<Part> query = BuildQuery();
            query = TenantScopeHelper.ApplyTenantScope(query, _tenantProvider, model.TenantId);

            if (!string.IsNullOrWhiteSpace(model.SearchText))
            {
                var t = model.SearchText;
                query = query.Where(x => x.PartNumber.Contains(t) || x.PartName.Contains(t) ||
                    x.Manufacturer.Contains(t) || x.SupplierName.Contains(t));
            }

            query = query.Where(x =>
                (!model.IsActive.HasValue || x.IsActive == model.IsActive) &&
                (!model.PartCategoryId.HasValue || x.PartCategoryId == model.PartCategoryId) &&
                (!model.IsSerialized.HasValue || x.IsSerialized == model.IsSerialized) &&
                (!model.StartDate.HasValue || x.CreatedOn >= model.StartDate.Value.Date) &&
                (!model.EndDate.HasValue || x.CreatedOn <= model.EndDate.Value.Date));

            if (model.LowStockOnly == true)
            {
                query = query.Where(p =>
                    (_context.PartInventories.Where(i => i.PartId == p.Id && i.IsActive)
                        .Sum(i => (decimal?)i.QuantityAvailable) ?? 0) < p.MinStockLevel);
            }

            var projected = query.Select(ProjectToDto());
            model.OrderByProp ??= nameof(Part.PartName);
            var ordered = model.SortDirection == (int)Enums.OrderBy.Ascending
                ? projected.OrderBy(model.OrderByProp)
                : projected.OrderBy($"{model.OrderByProp} descending");

            var paged = await ordered.ToPagedListAsync(model.PageNumber, model.PageSize);
            var list = paged.ToList();
            foreach (var item in list)
                await EnrichStockAsync(item);
            response.Result = list;
            return response;
        }

        public async Task<ResponseDto<bool>> DeleteAsync(int id)
        {
            var response = new ResponseDto<bool>();
            var entity = await _context.Parts.FindAsync(id);
            if (entity == null) { response.AddError("Part not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            entity.IsDeleted = true;
            entity.IsActive = false;
            await _context.SaveChangesAsync();
            response.Result = true;
            return response;
        }

        private async Task<string?> ValidatePartAsync(CreatePartDto dto, int? tenantId, int? partId)
        {
            if (string.IsNullOrWhiteSpace(dto.PartNumber) || string.IsNullOrWhiteSpace(dto.PartName))
                return "Part number and name are required.";
            if (dto.MinStockLevel < 0) return "Minimum stock cannot be negative.";
            if (dto.ExpectedLifeValue.HasValue && dto.ExpectedLifeValue < 0) return "Expected life cannot be negative.";
            if (dto.MaxStockLevel.HasValue && dto.MaxStockLevel < dto.MinStockLevel)
                return "Maximum stock cannot be less than minimum stock.";

            var category = await _context.PartCategories.FindAsync(dto.PartCategoryId);
            if (category == null || !category.IsActive) return "Part category not found or inactive.";
            if (category.TenantId != tenantId) return "Part category must belong to the same tenant.";
            return null;
        }

        private static Part MapToEntity(Part entity, CreatePartDto dto, int? tenantId)
        {
            entity.TenantId = tenantId;
            entity.PartNumber = dto.PartNumber.Trim();
            entity.PartName = dto.PartName.Trim();
            entity.PartCategoryId = dto.PartCategoryId;
            entity.Description = dto.Description;
            entity.Manufacturer = dto.Manufacturer?.Trim();
            entity.SupplierName = dto.SupplierName?.Trim();
            entity.UnitOfMeasure = dto.UnitOfMeasure?.Trim();
            entity.ExpectedLifeValue = dto.ExpectedLifeValue;
            entity.ExpectedLifeUnit = dto.ExpectedLifeUnit;
            entity.MinStockLevel = dto.MinStockLevel;
            entity.MaxStockLevel = dto.MaxStockLevel;
            entity.IsSerialized = true;
            entity.IsActive = dto.IsActive;
            return entity;
        }

        private IQueryable<Part> BuildQuery() =>
            _context.Parts.Include(x => x.PartCategory).Include(x => x.Tenant);

        private static System.Linq.Expressions.Expression<Func<Part, PartDto>> ProjectToDto() =>
            x => new PartDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,
                PartNumber = x.PartNumber,
                PartName = x.PartName,
                PartCategoryId = x.PartCategoryId,
                PartCategoryName = x.PartCategory.Name,
                Description = x.Description,
                Manufacturer = x.Manufacturer,
                SupplierName = x.SupplierName,
                UnitOfMeasure = x.UnitOfMeasure,
                ExpectedLifeValue = x.ExpectedLifeValue,
                ExpectedLifeUnit = x.ExpectedLifeUnit,
                MinStockLevel = x.MinStockLevel,
                MaxStockLevel = x.MaxStockLevel,
                IsSerialized = x.IsSerialized,
                IsActive = x.IsActive
            };

        private async Task<PartDto?> MapToDtoAsync(int id)
        {
            var dto = await BuildQuery().Where(x => x.Id == id).Select(ProjectToDto()).FirstOrDefaultAsync();
            if (dto != null) await EnrichStockAsync(dto);
            return dto;
        }

        private async Task EnrichStockAsync(PartDto dto)
        {
            dto.CurrentStock = await PartStockHelper.GetAvailableStockAsync(_context, dto.Id);
            dto.IsLowStock = dto.CurrentStock < dto.MinStockLevel;
        }
    }
}
