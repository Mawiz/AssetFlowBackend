using Microsoft.EntityFrameworkCore;
using RequestFlow.Data.Data;
using RequestFlow.Data.Entities.Tenant;
using RequestFlow.Services.Contracts;
using RequestFlow.Services.Dto;
using RequestFlow.Services.Dto.Tenant;
using RequestFlow.Services.Dto;
using System.Linq.Dynamic.Core;
using X.PagedList;
using System.Net;

namespace RequestFlow.Services.Core
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResponseDto<SubscriptionTypeDto>> CreateAsync(CreateSubscriptionTypeDto dto)
        {
            var response = new ResponseDto<SubscriptionTypeDto>();

            if (await _context.SubscriptionTypes.AnyAsync(x => x.Name == dto.Name))
            {
                response.AddError("Subscription Type name already exists.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            var entity = new SubscriptionType
            {
                Name = dto.Name,
                DisplayName = dto.DisplayName,
                Order = dto.Order,
                Description = dto.Description,
                IsActive = true
            };

            _context.SubscriptionTypes.Add(entity);
            await _context.SaveChangesAsync();

            response.Result = MapToDto(entity);
            return response;
        }

        public async Task<ResponseDto<SubscriptionTypeDto>> UpdateAsync(UpdateSubscriptionTypeDto dto)
        {
            var response = new ResponseDto<SubscriptionTypeDto>();

            var entity = await _context.SubscriptionTypes.FindAsync(dto.Id);
            if (entity == null)
            {
                response.AddError("Subscription Type not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            entity.Name = dto.Name;
            entity.DisplayName = dto.DisplayName;
            entity.Order = dto.Order;
            entity.Description = dto.Description;

            _context.SubscriptionTypes.Update(entity);
            await _context.SaveChangesAsync();

            response.Result = MapToDto(entity);
            return response;
        }

        public async Task<ResponseDto<SubscriptionTypeDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<SubscriptionTypeDto>();

            var entity = await _context.SubscriptionTypes.FindAsync(id);
            if (entity == null)
            {
                response.AddError("Subscription Type not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            response.Result = MapToDto(entity);
            return response;
        }

        public async Task<ResponseDto<List<SubscriptionTypeDto>>> GetAllAsync()
        {
            var response = new ResponseDto<List<SubscriptionTypeDto>>();

            var result = await _context.SubscriptionTypes
                .OrderBy(x => x.Order)
                .Select(x => MapToDto(x))
                .ToListAsync();

            response.Result = result;
            return response;
        }

        // 4C3 Filter + Pagination
        public async Task<ResponseDto<List<SubscriptionTypeDto>>> FilterAsync(SearchViewDto model)
        {
            var response = new ResponseDto<List<SubscriptionTypeDto>>();

            var query = _context.SubscriptionTypes
                .Where(x => (string.IsNullOrEmpty(model.SearchText) || x.Name.Contains(model.SearchText) || x.DisplayName.Contains(model.SearchText)) && 
                (!model.IsActive.HasValue || x.IsActive == model.IsActive) &&
                (!model.StartDate.HasValue || x.CreatedOn >= model.StartDate.Value.Date) &&
                (!model.EndDate.HasValue || x.CreatedOn >= model.EndDate.Value.Date))
                .Select(entity => new SubscriptionTypeDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    DisplayName = entity.DisplayName,
                    Order = entity.Order,
                    Description = entity.Description,
                    IsActive = entity.IsActive
                });

            model.OrderByProp ??= nameof(SubscriptionType.Id);

            var ordered = model.SortDirection == (int)RequestFlow.Common.Enum.Enums.OrderBy.Ascending
                ? query.OrderBy(model.OrderByProp)
                : query.OrderBy($"{model.OrderByProp} descending");

            var paged = await ordered.ToPagedList(pageNumber: model.PageNumber, pageSize: model.PageSize).ToListAsync();

            response.Result = paged;
            return response;
        }

        public async Task<ResponseDto<bool>> ToggleStatusAsync(int id)
        {
            var response = new ResponseDto<bool>();

            var entity = await _context.SubscriptionTypes.FindAsync(id);
            if (entity == null)
            {
                response.AddError("Subscription Type not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            entity.IsActive = !entity.IsActive;
            await _context.SaveChangesAsync();

            response.Result = entity.IsActive;
            return response;
        }

        private static SubscriptionTypeDto MapToDto(SubscriptionType entity)
        {
            return new SubscriptionTypeDto
            {
                Id = entity.Id,
                Name = entity.Name,
                DisplayName = entity.DisplayName,
                Order = entity.Order,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }

        public async Task<ResponseDto<bool>> DeleteAsync(int id)
        {
            var response = new ResponseDto<bool>();

            var entity = await _context.SubscriptionTypes.FindAsync(id);
            if (entity == null)
            {
                response.AddError("Subscription Type not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            entity.IsDeleted = true;
            await _context.SaveChangesAsync();

            response.Result = true;
            return response;
        }

    }
}
