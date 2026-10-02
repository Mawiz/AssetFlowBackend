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
    public class MaintenanceChecklistService : IMaintenanceChecklistService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public MaintenanceChecklistService(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<ResponseDto<MaintenanceChecklistDto>> CreateAsync(SaveMaintenanceChecklistDto dto)
        {
            var response = new ResponseDto<MaintenanceChecklistDto>();
            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Code))
            { response.AddError("Name and Code are required."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var tenantId = tenantResult.TenantId;
            var relatedError = await ValidateMaintenanceTypeTenantAsync(tenantId, dto.MaintenanceTypeId);
            if (relatedError != null) { response.AddError(relatedError); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            var code = dto.Code.Trim();
            if (await _context.MaintenanceChecklists.AnyAsync(x => x.TenantId == tenantId && x.Code == code))
            { response.AddError("Checklist code already exists."); response.StatusCode = HttpStatusCode.Conflict; return response; }

            var entity = new MaintenanceChecklist
            {
                TenantId = tenantId,
                Name = dto.Name.Trim(),
                Code = code,
                Description = dto.Description,
                MaintenanceTypeId = dto.MaintenanceTypeId,
                Version = 1,
                IsActive = dto.IsActive
            };
            _context.MaintenanceChecklists.Add(entity);
            await _context.SaveChangesAsync();
            await UpsertItemsAsync(entity.Id, dto.Items ?? new List<MaintenanceChecklistItemDto>());
            response.Result = await MapAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<MaintenanceChecklistDto>> UpdateAsync(UpdateMaintenanceChecklistDto dto)
        {
            var response = new ResponseDto<MaintenanceChecklistDto>();
            var entity = await _context.MaintenanceChecklists.Include(x => x.Items).ThenInclude(i => i.Options).FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entity.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            var relatedError = await ValidateMaintenanceTypeTenantAsync(tenantResult.TenantId, dto.MaintenanceTypeId);
            if (relatedError != null) { response.AddError(relatedError); response.StatusCode = HttpStatusCode.BadRequest; return response; }
            var code = dto.Code.Trim();
            if (await _context.MaintenanceChecklists.AnyAsync(x => x.TenantId == tenantResult.TenantId && x.Code == code && x.Id != dto.Id))
            { response.AddError("Checklist code already exists."); response.StatusCode = HttpStatusCode.Conflict; return response; }

            entity.Name = dto.Name.Trim();
            entity.Code = code;
            entity.Description = dto.Description;
            entity.MaintenanceTypeId = dto.MaintenanceTypeId;
            entity.IsActive = dto.IsActive;
            entity.TenantId = tenantResult.TenantId;
            entity.Version += 1;
            await UpsertItemsAsync(entity.Id, dto.Items ?? new List<MaintenanceChecklistItemDto>());
            await _context.SaveChangesAsync();
            response.Result = await MapAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<MaintenanceChecklistDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<MaintenanceChecklistDto>();
            var dto = await MapAsync(id);
            if (dto == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, dto.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<List<MaintenanceChecklistDto>>> GetAllAsync(int? tenantId, int? maintenanceTypeId)
        {
            var response = new ResponseDto<List<MaintenanceChecklistDto>>();
            var query = TenantScopeHelper.ApplyTenantScope(_context.MaintenanceChecklists.AsQueryable(), _tenantProvider, tenantId);
            if (maintenanceTypeId.HasValue) query = query.Where(x => x.MaintenanceTypeId == maintenanceTypeId);
            var ids = await query.Where(x => x.IsActive).OrderBy(x => x.Name).Select(x => x.Id).ToListAsync();
            var list = new List<MaintenanceChecklistDto>();
            foreach (var id in ids) { var m = await MapAsync(id); if (m != null) list.Add(m); }
            response.Result = list;
            return response;
        }

        public async Task<ResponseDto<object>> FilterAsync(MaintenanceChecklistFilterDto model)
        {
            var response = new ResponseDto<object>();
            var query = TenantScopeHelper.ApplyTenantScope(_context.MaintenanceChecklists.AsQueryable(), _tenantProvider, model.TenantId);
            if (model.IsActive.HasValue) query = query.Where(x => x.IsActive == model.IsActive);
            if (model.MaintenanceTypeId.HasValue) query = query.Where(x => x.MaintenanceTypeId == model.MaintenanceTypeId);
            if (!string.IsNullOrWhiteSpace(model.SearchText))
            {
                var s = model.SearchText.Trim();
                query = query.Where(x => x.Name.Contains(s) || x.Code.Contains(s));
            }
            var order = string.IsNullOrWhiteSpace(model.OrderByProp) ? "Name" : model.OrderByProp;
            if (model.SortDirection == (int)AssetFlow.Common.Enum.Enums.OrderBy.Descending) order += " descending";
            var page = await query.OrderBy(order).Select(x => new MaintenanceChecklistDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                Name = x.Name,
                Code = x.Code,
                Description = x.Description,
                MaintenanceTypeId = x.MaintenanceTypeId,
                MaintenanceTypeName = x.MaintenanceType != null ? x.MaintenanceType.Name : null,
                Version = x.Version,
                IsActive = x.IsActive
            }).ToPagedListAsync(model.PageNumber, model.PageSize);
            response.Result = page;
            return response;
        }

        public async Task<ResponseDto<bool>> DeleteAsync(int id)
        {
            var response = new ResponseDto<bool>();
            var entity = await _context.MaintenanceChecklists.FindAsync(id);
            if (entity == null) { response.AddError("Not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            var access = TenantScopeHelper.EnsureEntityTenantAccess(_tenantProvider, entity.TenantId);
            if (!access.Ok) { response.AddError(access.Error); response.StatusCode = HttpStatusCode.Forbidden; return response; }
            entity.IsDeleted = true;
            entity.IsActive = false;
            await _context.SaveChangesAsync();
            response.Result = true;
            return response;
        }

        private async Task<string?> ValidateMaintenanceTypeTenantAsync(int? tenantId, int? maintenanceTypeId)
        {
            if (!maintenanceTypeId.HasValue) return null;
            var mt = await _context.MaintenanceTypes.FindAsync(maintenanceTypeId.Value);
            if (mt == null || !mt.IsActive) return "Maintenance type not found or inactive.";
            if (mt.TenantId != tenantId) return "Maintenance type does not belong to the selected tenant.";
            return null;
        }

        private async Task UpsertItemsAsync(int checklistId, List<MaintenanceChecklistItemDto> items)
        {
            var existing = await _context.MaintenanceChecklistItems.Include(x => x.Options)
                .Where(x => x.MaintenanceChecklistId == checklistId).ToListAsync();
            var incomingIds = items.Where(x => x.Id > 0).Select(x => x.Id).ToHashSet();
            foreach (var ex in existing.Where(x => !incomingIds.Contains(x.Id)))
            {
                ex.IsDeleted = true;
                ex.IsActive = false;
            }
            foreach (var dto in items.OrderBy(x => x.SortOrder))
            {
                MaintenanceChecklistItem item;
                if (dto.Id > 0)
                {
                    item = existing.FirstOrDefault(x => x.Id == dto.Id);
                    if (item == null) continue;
                }
                else
                {
                    item = new MaintenanceChecklistItem { MaintenanceChecklistId = checklistId };
                    _context.MaintenanceChecklistItems.Add(item);
                }
                item.ItemText = dto.ItemText?.Trim();
                item.Description = dto.Description;
                item.ResponseType = dto.ResponseType;
                item.IsRequired = dto.IsRequired;
                item.SortOrder = dto.SortOrder;
                item.IsActive = dto.IsActive;
                item.IsDeleted = false;

                var optExisting = item.Options?.ToList() ?? new List<MaintenanceChecklistItemOption>();
                var optIncomingIds = (dto.Options ?? new()).Where(o => o.Id > 0).Select(o => o.Id).ToHashSet();
                foreach (var o in optExisting.Where(o => !optIncomingIds.Contains(o.Id)))
                {
                    o.IsDeleted = true;
                    o.IsActive = false;
                }
                foreach (var od in dto.Options ?? new List<MaintenanceChecklistItemOptionDto>())
                {
                    MaintenanceChecklistItemOption opt;
                    if (od.Id > 0) opt = optExisting.FirstOrDefault(x => x.Id == od.Id) ?? new MaintenanceChecklistItemOption { MaintenanceChecklistItemId = item.Id };
                    else
                    {
                        opt = new MaintenanceChecklistItemOption { MaintenanceChecklistItem = item };
                        _context.MaintenanceChecklistItemOptions.Add(opt);
                    }
                    opt.OptionText = od.OptionText?.Trim();
                    opt.SortOrder = od.SortOrder;
                    opt.IsActive = od.IsActive;
                    opt.IsDeleted = false;
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task<MaintenanceChecklistDto?> MapAsync(int id) =>
            await _context.MaintenanceChecklists
                .Include(x => x.MaintenanceType)
                .Include(x => x.Items)
                .ThenInclude(i => i.Options)
                .Where(x => x.Id == id)
                .Select(x => new MaintenanceChecklistDto
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    Name = x.Name,
                    Code = x.Code,
                    Description = x.Description,
                    MaintenanceTypeId = x.MaintenanceTypeId,
                    MaintenanceTypeName = x.MaintenanceType != null ? x.MaintenanceType.Name : null,
                    Version = x.Version,
                    IsActive = x.IsActive,
                    Items = x.Items.Where(i => !i.IsDeleted && i.IsActive).OrderBy(i => i.SortOrder).Select(i => new MaintenanceChecklistItemDto
                    {
                        Id = i.Id,
                        ItemText = i.ItemText,
                        Description = i.Description,
                        ResponseType = i.ResponseType,
                        IsRequired = i.IsRequired,
                        SortOrder = i.SortOrder,
                        IsActive = i.IsActive,
                        Options = i.Options.Where(o => !o.IsDeleted && o.IsActive).OrderBy(o => o.SortOrder).Select(o => new MaintenanceChecklistItemOptionDto
                        {
                            Id = o.Id,
                            OptionText = o.OptionText,
                            SortOrder = o.SortOrder,
                            IsActive = o.IsActive
                        }).ToList()
                    }).ToList()
                }).FirstOrDefaultAsync();
    }
}
