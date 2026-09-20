using Microsoft.EntityFrameworkCore;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.Location;
using AssetFlow.Data.Provider;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Location;
using System.Linq.Dynamic.Core;
using System.Net;
using X.PagedList;

namespace AssetFlow.Services.Core
{
    public class LocationService : ILocationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public LocationService(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<ResponseDto<LocationDto>> CreateAsync(CreateLocationDto dto)
        {
            var response = new ResponseDto<LocationDto>();

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok)
            {
                response.AddError(tenantResult.Error);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var tenantId = tenantResult.TenantId;

            var validation = await ValidateLocationAsync(
                dto.LocationTypeId, dto.ParentLocationId, null, tenantId);
            if (!validation.IsValid)
            {
                response.AddError(validation.Error);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                response.AddError("Name is required.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var entity = new Location
            {
                LocationTypeId = dto.LocationTypeId,
                ParentLocationId = dto.ParentLocationId,
                Name = dto.Name.Trim(),
                Description = dto.Description,
                IsActive = dto.IsActive,
                TenantId = tenantId
            };

            _context.Locations.Add(entity);
            await _context.SaveChangesAsync();

            response.Result = await MapToDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<LocationDto>> UpdateAsync(UpdateLocationDto dto)
        {
            var response = new ResponseDto<LocationDto>();

            var entity = await _context.Locations.FindAsync(dto.Id);
            if (entity == null)
            {
                response.AddError("Location not found.");
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

            var validation = await ValidateLocationAsync(
                dto.LocationTypeId, dto.ParentLocationId, dto.Id, tenantId);
            if (!validation.IsValid)
            {
                response.AddError(validation.Error);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                response.AddError("Name is required.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            entity.TenantId = tenantId;
            entity.LocationTypeId = dto.LocationTypeId;
            entity.ParentLocationId = dto.ParentLocationId;
            entity.Name = dto.Name.Trim();
            entity.Description = dto.Description;
            entity.IsActive = dto.IsActive;

            _context.Locations.Update(entity);
            await _context.SaveChangesAsync();

            response.Result = await MapToDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<LocationDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<LocationDto>();
            var dto = await MapToDtoAsync(id);
            if (dto == null)
            {
                response.AddError("Location not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<List<LocationDto>>> GetAllAsync()
        {
            var response = new ResponseDto<List<LocationDto>>();
            response.Result = await BuildLocationDtoQuery().ToListAsync();
            return response;
        }

        public async Task<ResponseDto<List<LocationDto>>> FilterAsync(LocationFilterDto model)
        {
            var response = new ResponseDto<List<LocationDto>>();

            IQueryable<Location> query = _context.Locations
                .Include(x => x.LocationType)
                .Include(x => x.ParentLocation)
                .Include(x => x.Tenant);

            query = TenantScopeHelper.ApplyAdminListTenantFilter(query, _tenantProvider, model.TenantId);

            query = query.Where(x =>
                (string.IsNullOrEmpty(model.SearchText) ||
                     x.Name.Contains(model.SearchText) ||
                     x.Description.Contains(model.SearchText)) &&
                (!model.IsActive.HasValue || x.IsActive == model.IsActive) &&
                (!model.StartDate.HasValue || x.CreatedOn >= model.StartDate.Value.Date) &&
                (!model.EndDate.HasValue || x.CreatedOn <= model.EndDate.Value.Date) &&
                (!model.LocationTypeId.HasValue || x.LocationTypeId == model.LocationTypeId));

            var projected = query.Select(ProjectToDto());

            model.OrderByProp ??= nameof(Location.Name);

            var ordered = model.SortDirection == (int)AssetFlow.Common.Enum.Enums.OrderBy.Ascending
                ? projected.OrderBy(model.OrderByProp)
                : projected.OrderBy($"{model.OrderByProp} descending");

            var paged = await ordered.ToPagedListAsync(model.PageNumber, model.PageSize);
            response.Result = paged.ToList();
            return response;
        }

        public async Task<ResponseDto<List<LocationDto>>> GetByLocationTypeIdAsync(int locationTypeId, int? tenantId = null)
        {
            var response = new ResponseDto<List<LocationDto>>();

            if (!await _context.LocationTypes.AnyAsync(x => x.Id == locationTypeId))
            {
                response.AddError("Location Type not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            var query = _context.Locations
                .Include(x => x.LocationType)
                .Include(x => x.ParentLocation)
                .Include(x => x.Tenant)
                .Where(x => x.LocationTypeId == locationTypeId && x.IsActive);

            var effectiveTenantId = TenantScopeHelper.GetContextTenantId(_tenantProvider);
            if (effectiveTenantId.HasValue)
            {
                query = query.Where(x => x.TenantId == effectiveTenantId);
            }
            else if (tenantId.HasValue)
            {
                query = query.Where(x => x.TenantId == tenantId);
            }

            response.Result = await query
                .OrderBy(x => x.Name)
                .Select(ProjectToDto())
                .ToListAsync();

            return response;
        }

        public async Task<ResponseDto<bool>> DeleteAsync(int id)
        {
            var response = new ResponseDto<bool>();

            var entity = await _context.Locations.FindAsync(id);
            if (entity == null)
            {
                response.AddError("Location not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            if (await _context.Locations.AnyAsync(x => x.ParentLocationId == id))
            {
                response.AddError("Cannot delete Location that has child locations.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            entity.IsDeleted = true;
            entity.IsActive = false;
            await _context.SaveChangesAsync();

            response.Result = true;
            return response;
        }

        private IQueryable<Location> BuildLocationQuery()
        {
            return _context.Locations
                .Include(x => x.LocationType)
                .Include(x => x.ParentLocation)
                .Include(x => x.Tenant);
        }

        private IQueryable<LocationDto> BuildLocationDtoQuery()
        {
            return BuildLocationQuery().Select(ProjectToDto());
        }

        private static System.Linq.Expressions.Expression<Func<Location, LocationDto>> ProjectToDto()
        {
            return x => new LocationDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,
                LocationTypeId = x.LocationTypeId,
                LocationTypeName = x.LocationType.Name,
                ParentLocationId = x.ParentLocationId,
                ParentLocationName = x.ParentLocation != null ? x.ParentLocation.Name : null,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            };
        }

        private async Task<LocationDto> MapToDtoAsync(int id)
        {
            return await BuildLocationDtoQuery().FirstOrDefaultAsync(x => x.Id == id);
        }

        private async Task<(bool IsValid, string Error)> ValidateLocationAsync(
            int locationTypeId,
            int? parentLocationId,
            int? locationId,
            int? tenantId)
        {
            var locationType = await _context.LocationTypes.FindAsync(locationTypeId);
            if (locationType == null)
                return (false, "Location Type not found.");

            if (locationType.TenantId != tenantId)
                return (false, "Location Type must belong to the same tenant.");

            if (!locationType.ParentLocationTypeId.HasValue)
            {
                if (parentLocationId.HasValue)
                    return (false, "Parent Location must be empty for this Location Type.");
            }
            else
            {
                if (!parentLocationId.HasValue)
                    return (false, "Parent Location is required for this Location Type.");

                var parentLocation = await _context.Locations
                    .FirstOrDefaultAsync(x => x.Id == parentLocationId.Value);

                if (parentLocation == null)
                    return (false, "Parent Location not found.");

                if (parentLocation.LocationTypeId != locationType.ParentLocationTypeId.Value)
                    return (false, "Parent Location must be of the required parent type.");

                if (parentLocation.TenantId != tenantId)
                    return (false, "Parent Location must belong to the same tenant.");
            }

            if (locationId.HasValue)
            {
                if (parentLocationId == locationId)
                    return (false, "A Location cannot be its own parent.");

                if (await LocationHierarchyValidator.WouldCreateLocationCycleAsync(
                        _context.Locations, locationId.Value, parentLocationId))
                    return (false, "Invalid parent Location: circular hierarchy.");
            }

            return (true, string.Empty);
        }
    }
}
