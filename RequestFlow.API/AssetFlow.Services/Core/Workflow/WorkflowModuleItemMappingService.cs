using AssetFlow.Common.Enum;
using AssetFlow.Common.Helper;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.Workflow;
using AssetFlow.Data.Provider;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Workflow;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Net;
using X.PagedList;
using static AssetFlow.Common.Enum.Enums;

namespace AssetFlow.Services.Core.Workflow
{
    public class WorkflowModuleItemMappingService : IWorkflowModuleItemMappingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        private static readonly HashSet<string> AllowedItemTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            WorkflowReferenceItemTypes.Issue,
            WorkflowReferenceItemTypes.Maintenance
        };

        public WorkflowModuleItemMappingService(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<ResponseDto<WorkflowModuleItemMappingDto>> CreateAsync(CreateWorkflowModuleItemMappingDto dto)
        {
            var response = new ResponseDto<WorkflowModuleItemMappingDto>();
            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var validation = await ValidateMappingAsync(tenantResult.TenantId, dto.ReferenceItemType, dto.ModuleId, null);
            if (!validation.Ok) { response.AddError(validation.Error!); response.StatusCode = validation.StatusCode; return response; }

            var entity = new WorkflowModuleItemMapping
            {
                ModuleId = dto.ModuleId,
                ReferenceItemType = dto.ReferenceItemType.Trim(),
                TenantId = tenantResult.TenantId,
                IsActive = dto.IsActive
            };
            _context.WorkflowModuleItemMappings.Add(entity);
            await _context.SaveChangesAsync();
            response.Result = await MapAsync(entity.ModuleItemMappingId);
            return response;
        }

        public async Task<ResponseDto<WorkflowModuleItemMappingDto>> UpdateAsync(UpdateWorkflowModuleItemMappingDto dto)
        {
            var response = new ResponseDto<WorkflowModuleItemMappingDto>();
            var entity = await _context.WorkflowModuleItemMappings.FirstOrDefaultAsync(x => x.ModuleItemMappingId == dto.ModuleItemMappingId && !x.IsDeleted);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entity.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }

            var validation = await ValidateMappingAsync(entity.TenantId, dto.ReferenceItemType, dto.ModuleId, entity.ModuleItemMappingId);
            if (!validation.Ok) { response.AddError(validation.Error!); response.StatusCode = validation.StatusCode; return response; }

            entity.ModuleId = dto.ModuleId;
            entity.ReferenceItemType = dto.ReferenceItemType.Trim();
            entity.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();
            response.Result = await MapAsync(entity.ModuleItemMappingId);
            return response;
        }

        public async Task<ResponseDto<WorkflowModuleItemMappingDto>> GetByIdAsync(int moduleItemMappingId)
        {
            var response = new ResponseDto<WorkflowModuleItemMappingDto>();
            var dto = await MapAsync(moduleItemMappingId);
            if (dto == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<List<WorkflowModuleItemMappingDto>>> GetAllAsync(int? tenantId)
        {
            var response = new ResponseDto<List<WorkflowModuleItemMappingDto>>();
            var query = TenantScopeHelper.ApplyTenantScope(_context.WorkflowModuleItemMappings.AsQueryable(), _tenantProvider, tenantId)
                .Where(x => !x.IsDeleted);
            var ids = await query.Select(x => x.ModuleItemMappingId).ToListAsync();
            var list = new List<WorkflowModuleItemMappingDto>();
            foreach (var id in ids)
            {
                var m = await MapAsync(id);
                if (m != null) list.Add(m);
            }
            response.Result = list;
            return response;
        }

        public async Task<ResponseDto<object>> FilterAsync(WorkflowModuleItemMappingFilterDto model)
        {
            var response = new ResponseDto<object>();
            var query = TenantScopeHelper.ApplyTenantScope(_context.WorkflowModuleItemMappings.AsQueryable(), _tenantProvider, model.TenantId)
                .Where(x => !x.IsDeleted);
            if (model.IsActive.HasValue) query = query.Where(x => x.IsActive == model.IsActive);
            if (!string.IsNullOrWhiteSpace(model.SearchText))
            {
                var s = model.SearchText.Trim();
                query = query.Where(x => x.ReferenceItemType.Contains(s));
            }
            var order = string.IsNullOrWhiteSpace(model.OrderByProp) ? "ReferenceItemType" : model.OrderByProp;
            if (model.SortDirection == (int)OrderBy.Descending) order += " descending";
            var paged = await query.OrderBy(order).ToPagedListAsync(model.PageNumber, model.PageSize);
            var items = new List<WorkflowModuleItemMappingDto>();
            foreach (var row in paged)
            {
                var m = await MapAsync(row.ModuleItemMappingId);
                if (m != null) items.Add(m);
            }
            response.Result = new { items, totalCount = paged.TotalItemCount };
            return response;
        }

        public async Task<ResponseDto<bool>> DeleteAsync(int moduleItemMappingId)
        {
            var response = new ResponseDto<bool>();
            var entity = await _context.WorkflowModuleItemMappings.FirstOrDefaultAsync(x => x.ModuleItemMappingId == moduleItemMappingId && !x.IsDeleted);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entity.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            entity.IsDeleted = true;
            await _context.SaveChangesAsync();
            response.Result = true;
            return response;
        }

        private async Task<(bool Ok, string? Error, HttpStatusCode StatusCode)> ValidateMappingAsync(int? tenantId, string referenceItemType, int moduleId, int? excludeId)
        {
            if (string.IsNullOrWhiteSpace(referenceItemType) || !AllowedItemTypes.Contains(referenceItemType.Trim()))
                return (false, "Reference item type must be Issue or Maintenance.", HttpStatusCode.BadRequest);

            var type = AllowedItemTypes.First(t => t.Equals(referenceItemType.Trim(), StringComparison.OrdinalIgnoreCase));

            var duplicate = await _context.WorkflowModuleItemMappings.AnyAsync(x =>
                x.TenantId == tenantId &&
                x.ReferenceItemType == type &&
                !x.IsDeleted &&
                (!excludeId.HasValue || x.ModuleItemMappingId != excludeId));
            if (duplicate)
                return (false, "A mapping already exists for this item type in the tenant.", HttpStatusCode.BadRequest);

            var module = await _context.WorkflowModules.AsNoTracking()
                .FirstOrDefaultAsync(m => m.ModuleId == moduleId && !m.IsDeleted && m.IsActive);
            if (module == null)
                return (false, "Workflow module not found or inactive.", HttpStatusCode.BadRequest);
            if (module.TenantId != tenantId)
                return (false, "Workflow module must belong to the same tenant.", HttpStatusCode.BadRequest);

            return (true, null, HttpStatusCode.OK);
        }

        private async Task<WorkflowModuleItemMappingDto?> MapAsync(int id)
        {
            var entity = await _context.WorkflowModuleItemMappings.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ModuleItemMappingId == id && !x.IsDeleted);
            if (entity == null) return null;

            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entity.TenantId);
            if (!access.Ok) return null;

            var module = await _context.WorkflowModules.AsNoTracking().FirstOrDefaultAsync(m => m.ModuleId == entity.ModuleId);
            string? tenantName = null;
            if (entity.TenantId.HasValue)
                tenantName = await _context.Tenants.Where(t => t.Id == entity.TenantId).Select(t => t.CompanyName).FirstOrDefaultAsync();

            return new WorkflowModuleItemMappingDto
            {
                ModuleItemMappingId = entity.ModuleItemMappingId,
                ModuleId = entity.ModuleId,
                ModuleNameEn = module?.ModuleNameEn,
                ReferenceItemType = entity.ReferenceItemType,
                TenantId = entity.TenantId,
                TenantName = tenantName,
                IsActive = entity.IsActive
            };
        }
    }
}
