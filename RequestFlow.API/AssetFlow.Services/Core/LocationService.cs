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

            var validation = await ValidateLocationAsync(dto.LocationTypeId, dto.ParentLocationId, null);
            if (!validation.IsValid)
            {
                response.AddError(validation.Error);
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
            var tenantId = (int?)_tenantProvider.GetTenantId();
            if (tenantId == 0) tenantId = null;
            if (await _context.Locations.AnyAsync(x => x.TenantId == tenantId && x.Code == code))
            {
                response.AddError("Location code already exists for this tenant.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            var entity = new Location
            {
                LocationTypeId = dto.LocationTypeId,
                ParentLocationId = dto.ParentLocationId,
                Name = dto.Name.Trim(),
                Code = code,
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

            var validation = await ValidateLocationAsync(dto.LocationTypeId, dto.ParentLocationId, dto.Id);
            if (!validation.IsValid)
            {
                response.AddError(validation.Error);
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
            if (await _context.Locations.AnyAsync(x => x.TenantId == entity.TenantId && x.Code == code && x.Id != dto.Id))
            {
                response.AddError("Location code already exists for this tenant.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            entity.LocationTypeId = dto.LocationTypeId;
            entity.ParentLocationId = dto.ParentLocationId;
            entity.Name = dto.Name.Trim();
            entity.Code = code;
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
            response.Result = await BuildLocationQuery().ToListAsync();
            return response;
        }

        public async Task<ResponseDto<List<LocationDto>>> FilterAsync(LocationFilterDto model)
        {
            var response = new ResponseDto<List<LocationDto>>();

            var query = _context.Locations
                .Include(x => x.LocationType)
                .Include(x => x.ParentLocation)
                .Where(x =>
                    (string.IsNullOrEmpty(model.SearchText) ||
                     x.Name.Contains(model.SearchText) ||
                     x.Code.Contains(model.SearchText) ||
                     x.Description.Contains(model.SearchText)) &&
                    (!model.IsActive.HasValue || x.IsActive == model.IsActive) &&
                    (!model.StartDate.HasValue || x.CreatedOn >= model.StartDate.Value.Date) &&
                    (!model.EndDate.HasValue || x.CreatedOn <= model.EndDate.Value.Date) &&
                    (!model.LocationTypeId.HasValue || x.LocationTypeId == model.LocationTypeId))
                .Select(x => new LocationDto
                {
                    Id = x.Id,
                    LocationTypeId = x.LocationTypeId,
                    LocationTypeName = x.LocationType.Name,
                    ParentLocationId = x.ParentLocationId,
                    ParentLocationName = x.ParentLocation != null ? x.ParentLocation.Name : null,
                    Name = x.Name,
                    Code = x.Code,
                    Description = x.Description,
                    IsActive = x.IsActive
                });

            model.OrderByProp ??= nameof(Location.Name);

            var ordered = model.SortDirection == (int)AssetFlow.Common.Enum.Enums.OrderBy.Ascending
                ? query.OrderBy(model.OrderByProp)
                : query.OrderBy($"{model.OrderByProp} descending");

            var paged = await ordered.ToPagedListAsync(model.PageNumber, model.PageSize);
            response.Result = paged.ToList();
            return response;
        }

        public async Task<ResponseDto<List<LocationDto>>> GetByLocationTypeIdAsync(int locationTypeId)
        {
            var response = new ResponseDto<List<LocationDto>>();

            if (!await _context.LocationTypes.AnyAsync(x => x.Id == locationTypeId))
            {
                response.AddError("Location Type not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            response.Result = await BuildLocationQuery()
                .Where(x => x.LocationTypeId == locationTypeId && x.IsActive)
                .OrderBy(x => x.Name)
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

        private IQueryable<LocationDto> BuildLocationQuery()
        {
            return _context.Locations
                .Include(x => x.LocationType)
                .Include(x => x.ParentLocation)
                .Select(x => new LocationDto
                {
                    Id = x.Id,
                    LocationTypeId = x.LocationTypeId,
                    LocationTypeName = x.LocationType.Name,
                    ParentLocationId = x.ParentLocationId,
                    ParentLocationName = x.ParentLocation != null ? x.ParentLocation.Name : null,
                    Name = x.Name,
                    Code = x.Code,
                    Description = x.Description,
                    IsActive = x.IsActive
                });
        }

        private async Task<LocationDto> MapToDtoAsync(int id)
        {
            return await BuildLocationQuery().FirstOrDefaultAsync(x => x.Id == id);
        }

        private async Task<(bool IsValid, string Error)> ValidateLocationAsync(
            int locationTypeId,
            int? parentLocationId,
            int? locationId)
        {
            var locationType = await _context.LocationTypes.FindAsync(locationTypeId);
            if (locationType == null)
                return (false, "Location Type not found.");

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

                var tenantId = (int?)_tenantProvider.GetTenantId();
                if (tenantId == 0) tenantId = null;
                if (tenantId.HasValue && parentLocation.TenantId != tenantId)
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
