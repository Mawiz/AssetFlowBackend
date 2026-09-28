using Microsoft.EntityFrameworkCore;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.Asset;
using AssetFlow.Data.Provider;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Asset;
using System.Linq.Dynamic.Core;
using System.Net;
using X.PagedList;

namespace AssetFlow.Services.Core
{
    public class AssetTypeService : IAssetTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public AssetTypeService(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<ResponseDto<AssetTypeDto>> CreateAsync(CreateAssetTypeDto dto)
        {
            var response = new ResponseDto<AssetTypeDto>();

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok)
            {
                response.AddError(tenantResult.Error);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var tenantId = tenantResult.TenantId;

            var categoryError = await ValidateAssetCategoryAsync(dto.AssetCategoryId, tenantId);
            if (categoryError != null)
            {
                response.AddError(categoryError);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Code))
            {
                response.AddError("Name and Code are required.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var code = dto.Code.Trim();
            if (await _context.AssetTypes.AnyAsync(x =>
                    x.TenantId == tenantId &&
                    x.AssetCategoryId == dto.AssetCategoryId &&
                    x.Code == code))
            {
                response.AddError("Asset Type code already exists for this category.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            var entity = new AssetType
            {
                TenantId = tenantId,
                AssetCategoryId = dto.AssetCategoryId,
                Name = dto.Name.Trim(),
                Code = code,
                Description = dto.Description,
                IsActive = dto.IsActive
            };

            _context.AssetTypes.Add(entity);
            await _context.SaveChangesAsync();

            response.Result = await MapToDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<AssetTypeDto>> UpdateAsync(UpdateAssetTypeDto dto)
        {
            var response = new ResponseDto<AssetTypeDto>();

            var entity = await _context.AssetTypes.FindAsync(dto.Id);
            if (entity == null)
            {
                response.AddError("Asset Type not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok)
            {
                response.AddError(tenantResult.Error);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var tenantId = tenantResult.TenantId;

            var categoryError = await ValidateAssetCategoryAsync(dto.AssetCategoryId, tenantId);
            if (categoryError != null)
            {
                response.AddError(categoryError);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Code))
            {
                response.AddError("Name and Code are required.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var code = dto.Code.Trim();
            if (await _context.AssetTypes.AnyAsync(x =>
                    x.TenantId == tenantId &&
                    x.AssetCategoryId == dto.AssetCategoryId &&
                    x.Code == code &&
                    x.Id != dto.Id))
            {
                response.AddError("Asset Type code already exists for this category.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            entity.TenantId = tenantId;
            entity.AssetCategoryId = dto.AssetCategoryId;
            entity.Name = dto.Name.Trim();
            entity.Code = code;
            entity.Description = dto.Description;
            entity.IsActive = dto.IsActive;

            _context.AssetTypes.Update(entity);
            await _context.SaveChangesAsync();

            response.Result = await MapToDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<AssetTypeDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<AssetTypeDto>();
            var dto = await MapToDtoAsync(id);
            if (dto == null)
            {
                response.AddError("Asset Type not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<List<AssetTypeDto>>> GetAllAsync()
        {
            var response = new ResponseDto<List<AssetTypeDto>>();
            IQueryable<AssetType> query = BuildQuery();
            query = TenantScopeHelper.ApplyTenantScope(query, _tenantProvider);
            response.Result = await query.Select(ProjectToDto()).ToListAsync();
            return response;
        }

        public async Task<ResponseDto<List<AssetTypeDto>>> FilterAsync(AssetTypeFilterDto model)
        {
            var response = new ResponseDto<List<AssetTypeDto>>();

            IQueryable<AssetType> query = BuildQuery();
            query = TenantScopeHelper.ApplyTenantScope(query, _tenantProvider, model.TenantId);

            query = query.Where(x =>
                (string.IsNullOrEmpty(model.SearchText) ||
                 x.Name.Contains(model.SearchText) ||
                 x.Code.Contains(model.SearchText) ||
                 x.Description.Contains(model.SearchText) ||
                 x.AssetCategory.Name.Contains(model.SearchText)) &&
                (!model.IsActive.HasValue || x.IsActive == model.IsActive) &&
                (!model.StartDate.HasValue || x.CreatedOn >= model.StartDate.Value.Date) &&
                (!model.EndDate.HasValue || x.CreatedOn <= model.EndDate.Value.Date) &&
                (!model.AssetCategoryId.HasValue || x.AssetCategoryId == model.AssetCategoryId));

            var projected = query.Select(ProjectToDto());

            model.OrderByProp ??= nameof(AssetType.Name);

            var ordered = model.SortDirection == (int)AssetFlow.Common.Enum.Enums.OrderBy.Ascending
                ? projected.OrderBy(model.OrderByProp)
                : projected.OrderBy($"{model.OrderByProp} descending");

            var paged = await ordered.ToPagedListAsync(model.PageNumber, model.PageSize);
            response.Result = paged.ToList();
            return response;
        }

        public async Task<ResponseDto<bool>> DeleteAsync(int id)
        {
            var response = new ResponseDto<bool>();

            var entity = await _context.AssetTypes.FindAsync(id);
            if (entity == null)
            {
                response.AddError("Asset Type not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            entity.IsDeleted = true;
            entity.IsActive = false;
            await _context.SaveChangesAsync();

            response.Result = true;
            return response;
        }

        private IQueryable<AssetType> BuildQuery()
        {
            return _context.AssetTypes
                .Include(x => x.AssetCategory)
                .Include(x => x.Tenant);
        }

        private static System.Linq.Expressions.Expression<Func<AssetType, AssetTypeDto>> ProjectToDto()
        {
            return x => new AssetTypeDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,
                AssetCategoryId = x.AssetCategoryId,
                AssetCategoryName = x.AssetCategory.Name,
                Name = x.Name,
                Code = x.Code,
                Description = x.Description,
                IsActive = x.IsActive
            };
        }

        private async Task<AssetTypeDto> MapToDtoAsync(int id)
        {
            return await BuildQuery()
                .Where(x => x.Id == id)
                .Select(ProjectToDto())
                .FirstOrDefaultAsync();
        }

        private async Task<string?> ValidateAssetCategoryAsync(int assetCategoryId, int? tenantId)
        {
            var category = await _context.AssetCategories.FindAsync(assetCategoryId);
            if (category == null)
                return "Asset Category not found.";

            if (!category.IsActive)
                return "Asset Category is not active.";

            if (category.TenantId != tenantId)
                return "Asset Category must belong to the same tenant.";

            return null;
        }
    }
}
