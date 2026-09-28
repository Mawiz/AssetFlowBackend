using Microsoft.EntityFrameworkCore;
using AssetFlow.Common.Enum;
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
    public class AssetService : IAssetService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public AssetService(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<ResponseDto<AssetDto>> CreateAsync(CreateAssetDto dto)
        {
            var response = new ResponseDto<AssetDto>();

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok)
            {
                response.AddError(tenantResult.Error);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var tenantId = tenantResult.TenantId;
            var validationError = await ValidateAssetFieldsAsync(dto, tenantId, null);
            if (validationError != null)
            {
                response.AddError(validationError);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var assetCode = dto.AssetCode.Trim();
            if (await _context.Assets.AnyAsync(x => x.TenantId == tenantId && x.AssetCode == assetCode))
            {
                response.AddError("Asset code already exists.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            var entity = MapToEntity(new Asset(), dto, tenantId);
            _context.Assets.Add(entity);
            await _context.SaveChangesAsync();

            response.Result = await MapToDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<AssetDto>> UpdateAsync(UpdateAssetDto dto)
        {
            var response = new ResponseDto<AssetDto>();

            var entity = await _context.Assets.FindAsync(dto.Id);
            if (entity == null)
            {
                response.AddError("Asset not found.");
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
            var validationError = await ValidateAssetFieldsAsync(dto, tenantId, dto.Id);
            if (validationError != null)
            {
                response.AddError(validationError);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var assetCode = dto.AssetCode.Trim();
            if (await _context.Assets.AnyAsync(x =>
                    x.TenantId == tenantId && x.AssetCode == assetCode && x.Id != dto.Id))
            {
                response.AddError("Asset code already exists.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            MapToEntity(entity, dto, tenantId);
            _context.Assets.Update(entity);
            await _context.SaveChangesAsync();
            response.Result = await MapToDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<AssetDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<AssetDto>();
            var dto = await MapToDtoAsync(id);
            if (dto == null)
            {
                response.AddError("Asset not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<List<AssetDto>>> GetAllAsync()
        {
            var response = new ResponseDto<List<AssetDto>>();
            IQueryable<Asset> query = BuildQuery();
            query = TenantScopeHelper.ApplyTenantScope(query, _tenantProvider);
            response.Result = await query.Select(ProjectToDto()).ToListAsync();
            return response;
        }

        public async Task<ResponseDto<List<AssetDto>>> FilterAsync(AssetFilterDto model)
        {
            var response = new ResponseDto<List<AssetDto>>();

            IQueryable<Asset> query = BuildQuery();
            query = TenantScopeHelper.ApplyTenantScope(query, _tenantProvider, model.TenantId);

            if (!string.IsNullOrWhiteSpace(model.SearchText))
            {
                var text = model.SearchText;
                query = query.Where(x =>
                    x.AssetCode.Contains(text) ||
                    x.Name.Contains(text) ||
                    x.SerialNumber.Contains(text) ||
                    x.Model.Contains(text) ||
                    x.Manufacturer.Contains(text));
            }

            query = query.Where(x =>
                (!model.IsActive.HasValue || x.IsActive == model.IsActive) &&
                (!model.StartDate.HasValue || x.CreatedOn >= model.StartDate.Value.Date) &&
                (!model.EndDate.HasValue || x.CreatedOn <= model.EndDate.Value.Date) &&
                (!model.AssetCategoryId.HasValue || x.AssetCategoryId == model.AssetCategoryId) &&
                (!model.AssetTypeId.HasValue || x.AssetTypeId == model.AssetTypeId) &&
                (!model.LocationId.HasValue || x.LocationId == model.LocationId) &&
                (!model.Status.HasValue || x.Status == model.Status) &&
                (!model.Criticality.HasValue || x.Criticality == model.Criticality) &&
                (!model.ResponsibleUserId.HasValue || x.ResponsibleUserId == model.ResponsibleUserId));

            var projected = query.Select(ProjectToDto());
            model.OrderByProp ??= nameof(Asset.Name);

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
            var entity = await _context.Assets.FindAsync(id);
            if (entity == null)
            {
                response.AddError("Asset not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            entity.IsDeleted = true;
            entity.IsActive = false;
            await _context.SaveChangesAsync();
            response.Result = true;
            return response;
        }

        private static Asset MapToEntity(Asset entity, CreateAssetDto dto, int? tenantId)
        {
            entity.TenantId = tenantId;
            entity.AssetCode = dto.AssetCode.Trim();
            entity.Name = dto.Name.Trim();
            entity.AssetCategoryId = dto.AssetCategoryId;
            entity.AssetTypeId = dto.AssetTypeId;
            entity.Manufacturer = dto.Manufacturer?.Trim();
            entity.Model = dto.Model?.Trim();
            entity.SerialNumber = dto.SerialNumber?.Trim();
            entity.InstallationDate = dto.InstallationDate;
            entity.LocationId = dto.LocationId;
            entity.ResponsibleUserId = dto.ResponsibleUserId;
            entity.OtherLocationInformation = dto.OtherLocationInformation;
            entity.Status = dto.Status;
            entity.Criticality = dto.Criticality;
            entity.WarrantyStartDate = dto.WarrantyStartDate;
            entity.WarrantyEndDate = dto.WarrantyEndDate;
            entity.PurchaseDate = dto.PurchaseDate;
            entity.PurchaseCost = dto.PurchaseCost;
            entity.SupplierName = dto.SupplierName?.Trim();
            entity.ExpectedLifeValue = dto.ExpectedLifeValue;
            entity.ExpectedLifeUnit = dto.ExpectedLifeUnit;
            entity.Notes = dto.Notes;
            entity.IsActive = dto.IsActive;
            return entity;
        }

        private async Task<string?> ValidateAssetFieldsAsync(CreateAssetDto dto, int? tenantId, int? assetId)
        {
            if (string.IsNullOrWhiteSpace(dto.AssetCode) || string.IsNullOrWhiteSpace(dto.Name))
                return "Asset code and name are required.";

            if (dto.LocationId <= 0)
                return "Location is required.";

            if (dto.WarrantyStartDate.HasValue && dto.WarrantyEndDate.HasValue &&
                dto.WarrantyEndDate < dto.WarrantyStartDate)
                return "Warranty end date must be on or after warranty start date.";

            var categoryError = await ValidateCategoryAsync(dto.AssetCategoryId, tenantId);
            if (categoryError != null) return categoryError;

            var type = await _context.AssetTypes.FindAsync(dto.AssetTypeId);
            if (type == null) return "Asset type not found.";
            if (!type.IsActive) return "Asset type is not active.";
            if (type.TenantId != tenantId) return "Asset type must belong to the same tenant.";
            if (type.AssetCategoryId != dto.AssetCategoryId)
                return "Asset type must belong to the selected asset category.";

            var location = await _context.Locations.FindAsync(dto.LocationId);
            if (location == null) return "Location not found.";
            if (!location.IsActive) return "Location is not active.";
            if (location.TenantId != tenantId) return "Location must belong to the same tenant.";

            if (dto.ResponsibleUserId.HasValue)
            {
                var user = await _context.ApplicationUsers.FindAsync(dto.ResponsibleUserId.Value);
                if (user == null) return "Responsible user not found.";
                if (user.TenantId != tenantId) return "Responsible user must belong to the same tenant.";
            }

            return null;
        }

        private async Task<string?> ValidateCategoryAsync(int assetCategoryId, int? tenantId)
        {
            var category = await _context.AssetCategories.FindAsync(assetCategoryId);
            if (category == null) return "Asset category not found.";
            if (!category.IsActive) return "Asset category is not active.";
            if (category.TenantId != tenantId) return "Asset category must belong to the same tenant.";
            return null;
        }

        private IQueryable<Asset> BuildQuery()
        {
            return _context.Assets
                .Include(x => x.AssetCategory)
                .Include(x => x.AssetType)
                .Include(x => x.Location)
                .Include(x => x.ResponsibleUser)
                .Include(x => x.Tenant);
        }

        private static System.Linq.Expressions.Expression<Func<Asset, AssetDto>> ProjectToDto()
        {
            return x => new AssetDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,
                AssetCode = x.AssetCode,
                Name = x.Name,
                AssetCategoryId = x.AssetCategoryId,
                AssetCategoryName = x.AssetCategory.Name,
                AssetTypeId = x.AssetTypeId,
                AssetTypeName = x.AssetType.Name,
                Manufacturer = x.Manufacturer,
                Model = x.Model,
                SerialNumber = x.SerialNumber,
                InstallationDate = x.InstallationDate,
                LocationId = x.LocationId,
                LocationName = x.Location.Name,
                ResponsibleUserId = x.ResponsibleUserId,
                ResponsibleUserName = x.ResponsibleUser != null
                    ? (x.ResponsibleUser.FullName ?? x.ResponsibleUser.UserName)
                    : null,
                OtherLocationInformation = x.OtherLocationInformation,
                Status = x.Status,
                Criticality = x.Criticality,
                WarrantyStartDate = x.WarrantyStartDate,
                WarrantyEndDate = x.WarrantyEndDate,
                PurchaseDate = x.PurchaseDate,
                PurchaseCost = x.PurchaseCost,
                SupplierName = x.SupplierName,
                ExpectedLifeValue = x.ExpectedLifeValue,
                ExpectedLifeUnit = x.ExpectedLifeUnit,
                Notes = x.Notes,
                IsActive = x.IsActive
            };
        }

        private async Task<AssetDto?> MapToDtoAsync(int id)
        {
            var dto = await BuildQuery()
                .Where(x => x.Id == id)
                .Select(ProjectToDto())
                .FirstOrDefaultAsync();

            return dto;
        }
    }
}
