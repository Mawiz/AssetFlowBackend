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
    public class AssetComponentService : IAssetComponentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public AssetComponentService(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<ResponseDto<AssetComponentDto>> CreateAsync(CreateAssetComponentDto dto)
        {
            var response = new ResponseDto<AssetComponentDto>();

            var asset = await ValidateAssetAsync(dto.AssetId);
            if (asset.Error != null)
            {
                response.AddError(asset.Error);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId ?? asset.TenantId);
            if (!tenantResult.Ok)
            {
                response.AddError(tenantResult.Error);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var tenantId = tenantResult.TenantId;
            var fieldError = ValidateComponentFields(dto);
            if (fieldError != null)
            {
                response.AddError(fieldError);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var code = dto.ComponentCode.Trim();
            if (await _context.AssetComponents.AnyAsync(x => x.TenantId == tenantId && x.ComponentCode == code))
            {
                response.AddError("Component code already exists.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            var entity = MapToEntity(new AssetComponent(), dto, tenantId, dto.AssetId);
            _context.AssetComponents.Add(entity);
            await _context.SaveChangesAsync();

            response.Result = await MapToDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<AssetComponentDto>> UpdateAsync(UpdateAssetComponentDto dto)
        {
            var response = new ResponseDto<AssetComponentDto>();

            var entity = await _context.AssetComponents.FindAsync(dto.Id);
            if (entity == null)
            {
                response.AddError("Component not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            if (entity.AssetId != dto.AssetId)
            {
                response.AddError("Component asset cannot be changed.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var asset = await ValidateAssetAsync(dto.AssetId);
            if (asset.Error != null)
            {
                response.AddError(asset.Error);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId ?? asset.TenantId);
            if (!tenantResult.Ok)
            {
                response.AddError(tenantResult.Error);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var tenantId = tenantResult.TenantId;
            var fieldError = ValidateComponentFields(dto);
            if (fieldError != null)
            {
                response.AddError(fieldError);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var code = dto.ComponentCode.Trim();
            if (await _context.AssetComponents.AnyAsync(x =>
                    x.TenantId == tenantId && x.ComponentCode == code && x.Id != dto.Id))
            {
                response.AddError("Component code already exists.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            MapToEntity(entity, dto, tenantId, dto.AssetId);
            _context.AssetComponents.Update(entity);
            await _context.SaveChangesAsync();
            response.Result = await MapToDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<AssetComponentDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<AssetComponentDto>();
            var dto = await MapToDtoAsync(id);
            if (dto == null)
            {
                response.AddError("Component not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<List<AssetComponentDto>>> FilterAsync(AssetComponentFilterDto model)
        {
            var response = new ResponseDto<List<AssetComponentDto>>();

            IQueryable<AssetComponent> query = BuildQuery();
            query = TenantScopeHelper.ApplyTenantScope(query, _tenantProvider, model.TenantId);

            if (!model.AssetId.HasValue)
            {
                response.AddError("Asset is required to list components.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            query = query.Where(x => x.AssetId == model.AssetId);

            if (!string.IsNullOrWhiteSpace(model.SearchText))
            {
                var text = model.SearchText;
                query = query.Where(x =>
                    x.ComponentCode.Contains(text) ||
                    x.ComponentName.Contains(text) ||
                    x.PartNumber.Contains(text) ||
                    x.SerialNumber.Contains(text) ||
                    x.Manufacturer.Contains(text));
            }

            query = query.Where(x =>
                (!model.IsActive.HasValue || x.IsActive == model.IsActive) &&
                (!model.StartDate.HasValue || x.CreatedOn >= model.StartDate.Value.Date) &&
                (!model.EndDate.HasValue || x.CreatedOn <= model.EndDate.Value.Date));

            var projected = query.Select(ProjectToDto());
            model.OrderByProp ??= nameof(AssetComponent.ComponentName);

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
            var entity = await _context.AssetComponents.FindAsync(id);
            if (entity == null)
            {
                response.AddError("Component not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            entity.IsDeleted = true;
            entity.IsActive = false;
            await _context.SaveChangesAsync();
            response.Result = true;
            return response;
        }

        private async Task<(int? TenantId, string? Error)> ValidateAssetAsync(int assetId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null) return (null, "Asset not found.");
            if (!asset.IsActive) return (null, "Asset is not active.");
            return (asset.TenantId, null);
        }

        private static string? ValidateComponentFields(CreateAssetComponentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ComponentCode) || string.IsNullOrWhiteSpace(dto.ComponentName))
                return "Component code and name are required.";

            if (dto.WarrantyStartDate.HasValue && dto.WarrantyEndDate.HasValue &&
                dto.WarrantyEndDate < dto.WarrantyStartDate)
                return "Warranty end date must be on or after warranty start date.";

            if (dto.CurrentRunningHours.HasValue && dto.CurrentRunningHours < 0)
                return "Running hours cannot be negative.";

            return null;
        }

        private static AssetComponent MapToEntity(
            AssetComponent entity,
            CreateAssetComponentDto dto,
            int? tenantId,
            int assetId)
        {
            entity.TenantId = tenantId;
            entity.AssetId = assetId;
            entity.ComponentCode = dto.ComponentCode.Trim();
            entity.ComponentName = dto.ComponentName.Trim();
            entity.PartNumber = dto.PartNumber?.Trim();
            entity.SerialNumber = dto.SerialNumber?.Trim();
            entity.Manufacturer = dto.Manufacturer?.Trim();
            entity.InstallationDate = dto.InstallationDate;
            entity.ExpectedLifeValue = dto.ExpectedLifeValue;
            entity.ExpectedLifeUnit = dto.ExpectedLifeUnit;
            entity.CurrentStatus = dto.CurrentStatus;
            entity.SupplierName = dto.SupplierName?.Trim();
            entity.WarrantyStartDate = dto.WarrantyStartDate;
            entity.WarrantyEndDate = dto.WarrantyEndDate;
            entity.InstallationLocation = dto.InstallationLocation;
            entity.CurrentRunningHours = dto.CurrentRunningHours;
            entity.Notes = dto.Notes;
            entity.IsActive = dto.IsActive;
            return entity;
        }

        private IQueryable<AssetComponent> BuildQuery()
        {
            return _context.AssetComponents
                .Include(x => x.Asset)
                .Include(x => x.Tenant);
        }

        private static System.Linq.Expressions.Expression<Func<AssetComponent, AssetComponentDto>> ProjectToDto()
        {
            return x => new AssetComponentDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,
                AssetId = x.AssetId,
                AssetCode = x.Asset.AssetCode,
                AssetName = x.Asset.Name,
                ComponentCode = x.ComponentCode,
                ComponentName = x.ComponentName,
                PartNumber = x.PartNumber,
                SerialNumber = x.SerialNumber,
                Manufacturer = x.Manufacturer,
                InstallationDate = x.InstallationDate,
                ExpectedLifeValue = x.ExpectedLifeValue,
                ExpectedLifeUnit = x.ExpectedLifeUnit,
                CurrentStatus = x.CurrentStatus,
                SupplierName = x.SupplierName,
                WarrantyStartDate = x.WarrantyStartDate,
                WarrantyEndDate = x.WarrantyEndDate,
                InstallationLocation = x.InstallationLocation,
                CurrentRunningHours = x.CurrentRunningHours,
                Notes = x.Notes,
                IsActive = x.IsActive
            };
        }

        private async Task<AssetComponentDto?> MapToDtoAsync(int id)
        {
            var dto = await BuildQuery()
                .Where(x => x.Id == id)
                .Select(ProjectToDto())
                .FirstOrDefaultAsync();

            return dto;
        }
    }
}
