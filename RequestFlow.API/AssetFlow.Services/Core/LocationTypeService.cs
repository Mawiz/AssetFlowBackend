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
    public class LocationTypeService : ILocationTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public LocationTypeService(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<ResponseDto<LocationTypeDto>> CreateAsync(CreateLocationTypeDto dto)
        {
            var response = new ResponseDto<LocationTypeDto>();

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok)
            {
                response.AddError(tenantResult.Error);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                response.AddError("Name is required.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var tenantId = tenantResult.TenantId;

            if (dto.ParentLocationTypeId.HasValue)
            {
                var parentError = await ValidateParentLocationTypeAsync(dto.ParentLocationTypeId.Value, tenantId, null);
                if (parentError != null)
                {
                    response.AddError(parentError);
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }
            }

            var entity = new LocationType
            {
                TenantId = tenantId,
                Name = dto.Name.Trim(),
                ParentLocationTypeId = dto.ParentLocationTypeId,
                Description = dto.Description,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive
            };

            _context.LocationTypes.Add(entity);
            await _context.SaveChangesAsync();

            response.Result = await MapToDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<LocationTypeDto>> UpdateAsync(UpdateLocationTypeDto dto)
        {
            var response = new ResponseDto<LocationTypeDto>();

            var entity = await _context.LocationTypes.FindAsync(dto.Id);
            if (entity == null)
            {
                response.AddError("Location Type not found.");
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

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                response.AddError("Name is required.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var tenantId = tenantResult.TenantId;

            if (dto.ParentLocationTypeId == dto.Id)
            {
                response.AddError("A Location Type cannot be its own parent.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            if (dto.ParentLocationTypeId.HasValue)
            {
                var parentError = await ValidateParentLocationTypeAsync(dto.ParentLocationTypeId.Value, tenantId, dto.Id);
                if (parentError != null)
                {
                    response.AddError(parentError);
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                if (await LocationHierarchyValidator.WouldCreateLocationTypeCycleAsync(
                        _context.LocationTypes, dto.Id, dto.ParentLocationTypeId))
                {
                    response.AddError("Invalid parent Location Type: circular hierarchy.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }
            }

            entity.TenantId = tenantId;
            entity.Name = dto.Name.Trim();
            entity.ParentLocationTypeId = dto.ParentLocationTypeId;
            entity.Description = dto.Description;
            entity.SortOrder = dto.SortOrder;
            entity.IsActive = dto.IsActive;

            _context.LocationTypes.Update(entity);
            await _context.SaveChangesAsync();

            response.Result = await MapToDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<LocationTypeDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<LocationTypeDto>();
            var dto = await MapToDtoAsync(id);
            if (dto == null)
            {
                response.AddError("Location Type not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<List<LocationTypeDto>>> GetAllAsync(int? tenantId = null)
        {
            var response = new ResponseDto<List<LocationTypeDto>>();

            var query = BaseQuery();
            query = TenantScopeHelper.ApplyAdminListTenantFilter(query, _tenantProvider, tenantId);

            var result = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(ProjectToDto())
                .ToListAsync();

            response.Result = result;
            return response;
        }

        public async Task<ResponseDto<List<LocationTypeDto>>> FilterAsync(SearchViewDto model)
        {
            var response = new ResponseDto<List<LocationTypeDto>>();

            var query = BaseQuery();
            query = TenantScopeHelper.ApplyAdminListTenantFilter(query, _tenantProvider, model.TenantId);

            query = query.Where(x =>
                (string.IsNullOrEmpty(model.SearchText) ||
                     x.Name.Contains(model.SearchText) ||
                     x.Description.Contains(model.SearchText)) &&
                (!model.IsActive.HasValue || x.IsActive == model.IsActive) &&
                (!model.StartDate.HasValue || x.CreatedOn >= model.StartDate.Value.Date) &&
                (!model.EndDate.HasValue || x.CreatedOn <= model.EndDate.Value.Date));

            var projected = query.Select(ProjectToDto());

            model.OrderByProp ??= nameof(LocationType.SortOrder);

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

            var entity = await _context.LocationTypes.FindAsync(id);
            if (entity == null)
            {
                response.AddError("Location Type not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            if (await _context.LocationTypes.AnyAsync(x => x.ParentLocationTypeId == id))
            {
                response.AddError("Cannot delete Location Type that has child types.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            if (await _context.Locations.AnyAsync(x => x.LocationTypeId == id))
            {
                response.AddError("Cannot delete Location Type that has Locations.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            entity.IsDeleted = true;
            entity.IsActive = false;
            await _context.SaveChangesAsync();

            response.Result = true;
            return response;
        }

        private IQueryable<LocationType> BaseQuery()
        {
            return _context.LocationTypes
                .Include(x => x.ParentLocationType)
                .Include(x => x.Tenant);
        }

        private static System.Linq.Expressions.Expression<Func<LocationType, LocationTypeDto>> ProjectToDto()
        {
            return x => new LocationTypeDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,
                Name = x.Name,
                ParentLocationTypeId = x.ParentLocationTypeId,
                ParentLocationTypeName = x.ParentLocationType != null ? x.ParentLocationType.Name : null,
                Description = x.Description,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            };
        }

        private async Task<LocationTypeDto> MapToDtoAsync(int id)
        {
            return await BaseQuery()
                .Where(x => x.Id == id)
                .Select(ProjectToDto())
                .FirstOrDefaultAsync();
        }

        private async Task<string?> ValidateParentLocationTypeAsync(int parentId, int? tenantId, int? selfId)
        {
            var parent = await _context.LocationTypes.FindAsync(parentId);
            if (parent == null)
                return "Parent Location Type not found.";

            if (parent.TenantId != tenantId)
                return "Parent Location Type must belong to the same tenant.";

            return null;
        }
    }
}
