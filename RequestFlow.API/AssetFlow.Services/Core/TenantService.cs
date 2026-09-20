
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AssetFlow.Common.Settings;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.Tenant;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Tenant;
using AssetFlow.Services.Dto;
using System.Linq.Dynamic.Core;
using X.PagedList;
using System.Net;

namespace AssetFlow.Services.Core
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

            var resourceError = await ValidateTenantResourceIdsAsync(dto.ResourceIds);
            if (resourceError != null)
            {
                response.AddError(resourceError);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            applicationDbContext.Tenants.Add(entity);
            await applicationDbContext.SaveChangesAsync();

            await SyncTenantResourcesAsync(entity.Id, dto.ResourceIds);
            await applicationDbContext.SaveChangesAsync();

            response.Result = await LoadTenantDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<TenantDto>> UpdateAsync(UpdateTenantDto dto)
        {
            var response = new ResponseDto<TenantDto>();

            var entity = await applicationDbContext.Tenants
                .Include(t => t.TenantLanguages)
                .Include(t => t.TenantResources)
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

            var resourceError = await ValidateTenantResourceIdsAsync(dto.ResourceIds);
            if (resourceError != null)
            {
                response.AddError(resourceError);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            await SyncTenantResourcesAsync(entity.Id, dto.ResourceIds);
            await PruneTenantRoleResourcesAsync(entity.Id, dto.ResourceIds);

            applicationDbContext.Tenants.Update(entity);
            await applicationDbContext.SaveChangesAsync();

            response.Result = await LoadTenantDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<TenantDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<TenantDto>();

            var entity = await applicationDbContext.Tenants
                .Include(t => t.TenantLanguages)
                .Include(t => t.TenantResources)
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

            var ordered = model.SortDirection == (int)AssetFlow.Common.Enum.Enums.OrderBy.Ascending
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
                ResourceIds = entity.TenantResources?.Select(tr => tr.ResourceId).ToList() ?? new(),
                IsActive = entity.IsActive
            };
        }

        private async Task<TenantDto> LoadTenantDtoAsync(int tenantId)
        {
            var entity = await applicationDbContext.Tenants
                .Include(t => t.TenantLanguages)
                .Include(t => t.TenantResources)
                .Include(t => t.SubscriptionType)
                .FirstAsync(t => t.Id == tenantId);
            return MapToDto(entity);
        }

        private async Task<string?> ValidateTenantResourceIdsAsync(List<int>? resourceIds)
        {
            var ids = (resourceIds ?? new List<int>()).Distinct().ToList();
            if (!ids.Any())
                return null;

            var leafCount = await applicationDbContext.Resources
                .CountAsync(r => ids.Contains(r.Id) && r.FeatureId != null);

            return leafCount == ids.Count
                ? null
                : "Tenant permissions must be valid permission entries (not feature rows).";
        }

        private async Task SyncTenantResourcesAsync(int tenantId, List<int>? resourceIds)
        {
            var ids = (resourceIds ?? new List<int>()).Distinct().ToList();
            var existing = await applicationDbContext.TenantResources
                .Where(tr => tr.TenantId == tenantId)
                .ToListAsync();

            var toRemove = existing.Where(tr => !ids.Contains(tr.ResourceId)).ToList();
            if (toRemove.Any())
                applicationDbContext.TenantResources.RemoveRange(toRemove);

            var existingIds = existing.Select(tr => tr.ResourceId).ToHashSet();
            foreach (var resourceId in ids.Where(id => !existingIds.Contains(id)))
            {
                applicationDbContext.TenantResources.Add(new TenantResource
                {
                    TenantId = tenantId,
                    ResourceId = resourceId
                });
            }
        }

        private async Task PruneTenantRoleResourcesAsync(int tenantId, List<int>? resourceIds)
        {
            var allowed = (resourceIds ?? new List<int>()).Distinct().ToHashSet();
            var roleIds = await applicationDbContext.Roles
                .Where(r => r.TenantId == tenantId)
                .Select(r => r.Id)
                .ToListAsync();

            if (!roleIds.Any())
                return;

            var toRemove = await applicationDbContext.RoleResources
                .Where(rr => roleIds.Contains(rr.ApplicationRoleId) && !allowed.Contains(rr.ResourceId))
                .ToListAsync();

            if (toRemove.Any())
                applicationDbContext.RoleResources.RemoveRange(toRemove);
        }
    }
}
