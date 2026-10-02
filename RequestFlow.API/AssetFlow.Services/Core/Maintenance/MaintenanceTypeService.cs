using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.Maintenance;
using AssetFlow.Data.Provider;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Maintenance;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Net;
using X.PagedList;

namespace AssetFlow.Services.Core.Maintenance
{
    public class MaintenanceTypeService : IMaintenanceTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public MaintenanceTypeService(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<ResponseDto<MaintenanceTypeDto>> CreateAsync(CreateMaintenanceTypeDto dto)
        {
            var response = new ResponseDto<MaintenanceTypeDto>();
            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Code))
            { response.AddError("Name and Code are required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var tenantId = tenantResult.TenantId;
            var code = dto.Code.Trim();
            if (await _context.MaintenanceTypes.AnyAsync(x => x.TenantId == tenantId && x.Code == code))
            { response.AddError("Maintenance type code already exists."); response.StatusCode = HttpStatusCode.Conflict; return response; }

            var entity = new MaintenanceType
            {
                TenantId = tenantId,
                Name = dto.Name.Trim(),
                Code = code,
                Description = dto.Description,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive
            };
            _context.MaintenanceTypes.Add(entity);
            await _context.SaveChangesAsync();
            response.Result = await MapAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<MaintenanceTypeDto>> UpdateAsync(UpdateMaintenanceTypeDto dto)
        {
            var response = new ResponseDto<MaintenanceTypeDto>();
            var entity = await _context.MaintenanceTypes.FindAsync(dto.Id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entity.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            var code = dto.Code.Trim();
            if (await _context.MaintenanceTypes.AnyAsync(x => x.TenantId == tenantResult.TenantId && x.Code == code && x.Id != dto.Id))
            { response.AddError("Maintenance type code already exists."); response.StatusCode = HttpStatusCode.Conflict; return response; }

            entity.Name = dto.Name.Trim();
            entity.Code = code;
            entity.Description = dto.Description;
            entity.SortOrder = dto.SortOrder;
            entity.IsActive = dto.IsActive;
            entity.TenantId = tenantResult.TenantId;
            await _context.SaveChangesAsync();
            response.Result = await MapAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<MaintenanceTypeDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<MaintenanceTypeDto>();
            var dto = await MapAsync(id);
            if (dto == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, dto.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<List<MaintenanceTypeDto>>> GetAllAsync(int? tenantId)
        {
            var response = new ResponseDto<List<MaintenanceTypeDto>>();
            var query = TenantScopeHelper.ApplyTenantScope(_context.MaintenanceTypes.AsQueryable(), _tenantProvider, tenantId);
            response.Result = await query.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(x => new MaintenanceTypeDto
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    Name = x.Name,
                    Code = x.Code,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive
                }).ToListAsync();
            return response;
        }

        public async Task<ResponseDto<object>> FilterAsync(MaintenanceTypeFilterDto model)
        {
            var response = new ResponseDto<object>();
            var query = TenantScopeHelper.ApplyTenantScope(_context.MaintenanceTypes.AsQueryable(), _tenantProvider, model.TenantId);
            if (model.IsActive.HasValue) query = query.Where(x => x.IsActive == model.IsActive);
            if (!string.IsNullOrWhiteSpace(model.SearchText))
            {
                var s = model.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(s) || x.Code.Contains(s) || x.Description.Contains(s));
            }
            var order = string.IsNullOrWhiteSpace(model.OrderByProp) ? "SortOrder" : model.OrderByProp;
            if (model.SortDirection == (int)AssetFlow.Common.Enum.Enums.OrderBy.Descending) order += " descending";
            query = query.OrderBy(order);
            var page = await query.Select(x => new MaintenanceTypeDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                Name = x.Name,
                Code = x.Code,
                Description = x.Description,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            }).ToPagedListAsync(model.PageNumber, model.PageSize);
            response.Result = page;
            return response;
        }

        public async Task<ResponseDto<bool>> DeleteAsync(int id)
        {
            var response = new ResponseDto<bool>();
            var entity = await _context.MaintenanceTypes.FindAsync(id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entity.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            entity.IsDeleted = true;
            entity.IsActive = false;
            await _context.SaveChangesAsync();
            response.Result = true;
            return response;
        }

        private async Task<MaintenanceTypeDto?> MapAsync(int id) =>
            await _context.MaintenanceTypes.Where(x => x.Id == id).Select(x => new MaintenanceTypeDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                Name = x.Name,
                Code = x.Code,
                Description = x.Description,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            }).FirstOrDefaultAsync();
    }
}
