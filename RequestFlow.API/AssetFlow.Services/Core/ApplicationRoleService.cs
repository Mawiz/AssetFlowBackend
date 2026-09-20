using Microsoft.EntityFrameworkCore;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.ACL;
using AssetFlow.Data.Identity;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Role;
using AssetFlow.Services.Dto;
using System.Linq.Dynamic.Core;
using X.PagedList;
using AssetFlow.Services.Dto.Role.RoleResource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AssetFlow.Services.Core
{
    public class ApplicationRoleService : IApplicationRoleService
    {
        private readonly ApplicationDbContext _context;

        public ApplicationRoleService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Create Role with Resource Assignments
        public async Task<ResponseDto<RoleDto>> CreateAsync(CreateRoleDto dto)
        {
            var response = new ResponseDto<RoleDto>();

            if (await _context.Roles.AnyAsync(x => x.Name == dto.Name && x.TenantId == dto.TenantId))
            {
                response.AddError("Role already exists.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            var resourceError = await ValidateRoleResourcesForTenantAsync(dto.TenantId, dto.ResourceIds);
            if (resourceError != null)
            {
                response.AddError(resourceError);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var role = new ApplicationRole
            {
                Name = dto.Name,
                NormalizedName = dto.Name.ToUpperInvariant(),
                DisplayName = dto.DisplayName,
                Description = dto.Description,
                CreatedOn = DateTime.UtcNow,
                TenantId = dto.TenantId
            };

            // Assign selected resources
            foreach (var resourceId in dto.ResourceIds.Distinct())
            {
                role.RoleResources.Add(new RoleResource { ResourceId = resourceId });
            }

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            response.Result = MapToDto(role);
            return response;
        }

        // 🔹 Update Role with Resource Assignments
        public async Task<ResponseDto<RoleDto>> UpdateAsync(UpdateRoleDto dto)
        {
            var response = new ResponseDto<RoleDto>();

            var role = await _context.Roles
                .Include(r => r.RoleResources)
                .FirstOrDefaultAsync(r => r.Id == dto.Id);

            if (role == null)
            {
                response.AddError("Role not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            if (await _context.Roles.AnyAsync(x => x.Id != dto.Id && x.Name == dto.Name && x.TenantId == dto.TenantId))
            {
                response.AddError("Role already exists.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            var resourceError = await ValidateRoleResourcesForTenantAsync(dto.TenantId, dto.ResourceIds);
            if (resourceError != null)
            {
                response.AddError(resourceError);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            // Update basic info
            role.Name = dto.Name;
            role.NormalizedName = dto.Name.ToUpperInvariant();
            role.DisplayName = dto.DisplayName;
            role.Description = dto.Description;
            role.ModifiedOn = DateTime.UtcNow;
            role.TenantId = dto.TenantId;

            // Sync Resources
            var currentResourceIds = role.RoleResources.Select(rr => rr.ResourceId).ToList();
            var newResourceIds = dto.ResourceIds.Distinct().ToList();

            // Find resources to remove
            var toRemove = currentResourceIds.Except(newResourceIds).ToList();
            if (toRemove.Any())
            {
                var removeList = role.RoleResources.Where(rr => toRemove.Contains(rr.ResourceId)).ToList();
                _context.RoleResources.RemoveRange(removeList);
            }

            // Find resources to add
            var toAdd = newResourceIds.Except(currentResourceIds).ToList();
            foreach (var resId in toAdd)
            {
                role.RoleResources.Add(new RoleResource { ResourceId = resId });
            }

            await _context.SaveChangesAsync();

            response.Result = MapToDto(role);
            return response;
        }

        public async Task<ResponseDto<RoleDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<RoleDto>();

            var role = await _context.Roles
                .Include(r => r.RoleResources)
                .Include(r => r.Tenant)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                response.AddError("Role not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            response.Result = MapToDto(role);
            return response;
        }

        // 🔹 Get All Roles
        public async Task<ResponseDto<List<RoleDto>>> GetAllAsync()
        {
            var response = new ResponseDto<List<RoleDto>>();

            var list = await _context.ApplicationRoles
                .Include(r => r.RoleResources)
                .Include(r => r.Tenant)
                .OrderBy(r => r.DisplayName)
                .Select(r => MapToDto(r))
                .ToListAsync();

            response.Result = list;
            return response;
        }

        // 🔹 Filter + Pagination for Roles
        public async Task<ResponseDto<List<RoleDto>>> FilterAsync(SearchViewDto model)
        {
            var response = new ResponseDto<List<RoleDto>>();

            var queryable = _context.ApplicationRoles
                .Include(r => r.RoleResources)
                .Include(r => r.Tenant)
                .Where(x => (string.IsNullOrEmpty(model.SearchText) || x.Name.Contains(model.SearchText) || x.DisplayName.Contains(model.SearchText) || x.Description.Contains(model.SearchText)) &&
                //(!model.IsActive.HasValue || x == model.IsActive) &&
                (!model.StartDate.HasValue || x.CreatedOn >= model.StartDate.Value.Date) &&
                (!model.EndDate.HasValue || x.CreatedOn >= model.EndDate.Value.Date) &&
                (!model.TenantId.HasValue || x.TenantId == model.TenantId))
                .Where(r => string.IsNullOrEmpty(model.SearchText) || r.Name.Contains(model.SearchText) || r.DisplayName.Contains(model.SearchText) || r.Description.Contains(model.SearchText))
                .Select(entity => new RoleDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    DisplayName = entity.DisplayName,
                    Description = entity.Description,
                    ResourceIds = entity.RoleResources.Select(rr => rr.ResourceId).ToList(),
                    TenantId = entity.TenantId,
                    TenantName = entity.Tenant.CompanyName ?? "System"
                });

            model.OrderByProp ??= nameof(ApplicationRole.Id);

            var ordered = model.SortDirection == (int)AssetFlow.Common.Enum.Enums.OrderBy.Ascending
                ? queryable.OrderBy(model.OrderByProp)
                : queryable.OrderBy($"{model.OrderByProp} descending");

            var paged = await ordered.ToPagedList(pageNumber: model.PageNumber, pageSize: model.PageSize).ToListAsync();

            response.Result = paged;
            return response;
        }

        // 🔹 Helper: Map Role to DTO
        private static RoleDto MapToDto(ApplicationRole entity)
        {
            return new RoleDto
            {
                Id = entity.Id,
                Name = entity.Name,
                DisplayName = entity.DisplayName,
                Description = entity.Description,
                ResourceIds = entity.RoleResources.Select(rr => rr.ResourceId).ToList(),
                TenantId = entity.TenantId,
                TenantName = entity?.Tenant?.CompanyName ?? "System"
            };
        }

        public async Task<ResponseDto<List<RoleWithResourcesDto>>> GetRolesByTenantAsync(int? tenantId)
        {
            var response = new ResponseDto<List<RoleWithResourcesDto>>();

            var query = _context.Roles
                .Include(r => r.RoleResources)
                    .ThenInclude(rr => rr.Resource)
                .Include(r => r.Tenant)
                .AsQueryable();

            // 🔹 Filter by tenant
            if (tenantId.HasValue)
                query = query.Where(r => r.TenantId == tenantId.Value);
            else
                query = query.Where(r => r.TenantId == null);

            var roles = await query
                .OrderBy(r => r.Order)
                .Select(r => new RoleWithResourcesDto
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    DisplayName = r.DisplayName,
                    TenantId = r.TenantId,
                    TenantName = r.Tenant != null ? r.Tenant.CompanyName : "Owner",

                    // Map distinct top-level features (FeatureId == null)
                    Resources = r.RoleResources
                        .Where(rr => rr.Resource.FeatureId == null)
                        .Select(rr => new ResourceDto
                        {
                            Id = rr.Resource.Id,
                            ResourceName = rr.Resource.ResourceName,
                            SubResources = r.RoleResources
                                .Where(sub => sub.Resource.FeatureId == rr.Resource.Id)
                                .Select(sub => new SubResourceDto
                                {
                                    Id = sub.Resource.Id,
                                    ResourceName = sub.Resource.ResourceName
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToListAsync();

            response.Result = roles;
            return response;
        }

        private async Task<string?> ValidateRoleResourcesForTenantAsync(int? tenantId, List<int>? resourceIds)
        {
            if (!tenantId.HasValue)
                return null;

            var allowed = await _context.TenantResources
                .Where(tr => tr.TenantId == tenantId.Value)
                .Select(tr => tr.ResourceId)
                .ToHashSetAsync();

            if (!allowed.Any())
                return "This tenant has no permissions assigned. Update the tenant permission set first.";

            var ids = (resourceIds ?? new List<int>()).Distinct().ToList();
            if (!ids.Any())
                return null;

            var resources = await _context.Resources
                .Where(r => ids.Contains(r.Id))
                .Select(r => new { r.Id, r.FeatureId })
                .ToListAsync();

            foreach (var resource in resources.Where(r => r.FeatureId != null))
            {
                if (!allowed.Contains(resource.Id))
                    return "One or more permissions are not allowed for this tenant.";
            }

            return null;
        }
    }
}
