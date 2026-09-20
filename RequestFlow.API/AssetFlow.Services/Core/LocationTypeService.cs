using Microsoft.EntityFrameworkCore;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.Location;
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

        public LocationTypeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResponseDto<LocationTypeDto>> CreateAsync(CreateLocationTypeDto dto)
        {
            var response = new ResponseDto<LocationTypeDto>();

            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Code))
            {
                response.AddError("Name and Code are required.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var code = dto.Code.Trim();
            if (await _context.LocationTypes.AnyAsync(x => x.Code == code))
            {
                response.AddError("Location Type code already exists.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            if (dto.ParentLocationTypeId.HasValue)
            {
                var parentExists = await _context.LocationTypes.AnyAsync(x => x.Id == dto.ParentLocationTypeId.Value);
                if (!parentExists)
                {
                    response.AddError("Parent Location Type not found.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }
            }

            var entity = new LocationType
            {
                Name = dto.Name.Trim(),
                Code = code,
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

            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Code))
            {
                response.AddError("Name and Code are required.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var code = dto.Code.Trim();
            if (await _context.LocationTypes.AnyAsync(x => x.Code == code && x.Id != dto.Id))
            {
                response.AddError("Location Type code already exists.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            if (dto.ParentLocationTypeId == dto.Id)
            {
                response.AddError("A Location Type cannot be its own parent.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            if (dto.ParentLocationTypeId.HasValue)
            {
                var parentExists = await _context.LocationTypes.AnyAsync(x => x.Id == dto.ParentLocationTypeId.Value);
                if (!parentExists)
                {
                    response.AddError("Parent Location Type not found.");
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

            entity.Name = dto.Name.Trim();
            entity.Code = code;
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

        public async Task<ResponseDto<List<LocationTypeDto>>> GetAllAsync()
        {
            var response = new ResponseDto<List<LocationTypeDto>>();

            var result = await _context.LocationTypes
                .Include(x => x.ParentLocationType)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => new LocationTypeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    ParentLocationTypeId = x.ParentLocationTypeId,
                    ParentLocationTypeName = x.ParentLocationType != null ? x.ParentLocationType.Name : null,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            response.Result = result;
            return response;
        }

        public async Task<ResponseDto<List<LocationTypeDto>>> FilterAsync(SearchViewDto model)
        {
            var response = new ResponseDto<List<LocationTypeDto>>();

            var query = _context.LocationTypes
                .Include(x => x.ParentLocationType)
                .Where(x =>
                    (string.IsNullOrEmpty(model.SearchText) ||
                     x.Name.Contains(model.SearchText) ||
                     x.Code.Contains(model.SearchText) ||
                     x.Description.Contains(model.SearchText)) &&
                    (!model.IsActive.HasValue || x.IsActive == model.IsActive) &&
                    (!model.StartDate.HasValue || x.CreatedOn >= model.StartDate.Value.Date) &&
                    (!model.EndDate.HasValue || x.CreatedOn <= model.EndDate.Value.Date))
                .Select(x => new LocationTypeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    ParentLocationTypeId = x.ParentLocationTypeId,
                    ParentLocationTypeName = x.ParentLocationType != null ? x.ParentLocationType.Name : null,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive
                });

            model.OrderByProp ??= nameof(LocationType.SortOrder);

            var ordered = model.SortDirection == (int)AssetFlow.Common.Enum.Enums.OrderBy.Ascending
                ? query.OrderBy(model.OrderByProp)
                : query.OrderBy($"{model.OrderByProp} descending");

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

        private async Task<LocationTypeDto> MapToDtoAsync(int id)
        {
            return await _context.LocationTypes
                .Include(x => x.ParentLocationType)
                .Where(x => x.Id == id)
                .Select(x => new LocationTypeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    ParentLocationTypeId = x.ParentLocationTypeId,
                    ParentLocationTypeName = x.ParentLocationType != null ? x.ParentLocationType.Name : null,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync();
        }
    }
}
