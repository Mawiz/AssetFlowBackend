
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RequestFlow.Common.Settings;
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
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ApplicationDbContext applicationDbContext;
        private readonly SystemSettings systemSettings;
        public TenantService(ApplicationDbContext applicationDbContext, IHttpContextAccessor httpContextAccessor,
            IOptions<SystemSettings> systemSettings)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.applicationDbContext = applicationDbContext;
            this.systemSettings = systemSettings.Value;
        }

        public async Task<ResponseDto<TenantDto>> CreateAsync(CreateTenantDto dto)
        {
            var response = new ResponseDto<TenantDto>();

            if (await applicationDbContext.Tenants.AnyAsync(x => x.CompanyName == dto.CompanyName))
            {
                response.AddError("Tenant with the same company name already exists.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            var entity = new Tenant
            {
                CompanyName = dto.CompanyName,
                SubscriptionTypeId = dto.SubscriptionTypeId,
                IsActive = true
            };

            foreach (var langId in dto.LanguageIds)
            {
                entity.TenantLanguages.Add(new TenantLanguage
                {
                    LanguageId = langId,
                    IsActive = true,
                    IsDeleted = false
                });
            }

            applicationDbContext.Tenants.Add(entity);
            await applicationDbContext.SaveChangesAsync();

            response.Result = MapToDto(entity);
            return response;
        }

        public async Task<ResponseDto<TenantDto>> UpdateAsync(UpdateTenantDto dto)
        {
            var response = new ResponseDto<TenantDto>();

            var entity = await applicationDbContext.Tenants
                .Include(t => t.TenantLanguages)
                .Include(t => t.SubscriptionType)
                .FirstOrDefaultAsync(t => t.Id == dto.Id);

            if (entity == null)
            {
                response.AddError("Tenant not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            entity.CompanyName = dto.CompanyName;
            entity.SubscriptionTypeId = dto.SubscriptionTypeId;

            // Reset languages
            entity.TenantLanguages.Clear();
            foreach (var langId in dto.LanguageIds)
            {
                entity.TenantLanguages.Add(new TenantLanguage
                {
                    LanguageId = langId,
                    IsActive = true,
                    IsDeleted = false
                });
            }

            applicationDbContext.Tenants.Update(entity);
            await applicationDbContext.SaveChangesAsync();

            response.Result = MapToDto(entity);
            return response;
        }

        public async Task<ResponseDto<TenantDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<TenantDto>();

            var entity = await applicationDbContext.Tenants
                .Include(t => t.TenantLanguages)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
            {
                response.AddError("Tenant not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            response.Result = MapToDto(entity);
            return response;
        }

        public async Task<ResponseDto<List<TenantDto>>> GetAllAsync()
        {
            var response = new ResponseDto<List<TenantDto>>();

            var result = await applicationDbContext.Tenants
                .Include(t => t.TenantLanguages)
                .Include(t => t.SubscriptionType)
                .Select(t => MapToDto(t))
                .ToListAsync();

            response.Result = result;
            return response;
        }

        // 4C3 Filter + Pagination for Tenants
        public async Task<ResponseDto<List<TenantDto>>> FilterAsync(SearchViewDto model)
        {
            var response = new ResponseDto<List<TenantDto>>();

            var query = applicationDbContext.Tenants
                .Where(x => (string.IsNullOrEmpty(model.SearchText) || x.CompanyName.Contains(model.SearchText)) &&
                (!model.IsActive.HasValue || x.IsActive == model.IsActive) &&
                (!model.StartDate.HasValue || x.CreatedOn >= model.StartDate.Value.Date) &&
                (!model.EndDate.HasValue || x.CreatedOn >= model.EndDate.Value.Date))
                .Include(t => t.TenantLanguages)
                .Include(t => t.SubscriptionType)
                .Where(t => string.IsNullOrEmpty(model.SearchText) || t.CompanyName.Contains(model.SearchText))
                .Select(entity => new TenantDto
                {
                    Id = entity.Id,
                    CompanyName = entity.CompanyName,
                    SubscriptionTypeId = entity.SubscriptionTypeId,
                    SubscriptionName = entity.SubscriptionType.DisplayName,
                    LanguageIds = entity.TenantLanguages.Select(tl => tl.LanguageId).ToList() ?? new(),
                    IsActive = entity.IsActive
                });

            model.OrderByProp ??= nameof(Tenant.Id);

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

            var entity = await applicationDbContext.Tenants.FindAsync(id);
            if (entity == null)
            {
                response.AddError("Tenant not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            entity.IsActive = !entity.IsActive;
            await applicationDbContext.SaveChangesAsync();

            response.Result = entity.IsActive;
            return response;
        }

        private static TenantDto MapToDto(Tenant entity)
        {
            return new TenantDto
            {
                Id = entity.Id,
                CompanyName = entity.CompanyName,
                SubscriptionTypeId = entity.SubscriptionTypeId,
                SubscriptionName = entity.SubscriptionType?.DisplayName,
                LanguageIds = entity.TenantLanguages?.Select(tl => tl.LanguageId).ToList() ?? new(),
                IsActive = entity.IsActive
            };
        }
    }
}
