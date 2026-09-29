using Microsoft.EntityFrameworkCore;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.SparePart;
using AssetFlow.Data.Provider;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.SparePart;
using System.Linq.Dynamic.Core;
using System.Net;
using X.PagedList;

namespace AssetFlow.Services.Core
{
    public class PartCategoryService : IPartCategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public PartCategoryService(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<ResponseDto<PartCategoryDto>> CreateAsync(CreatePartCategoryDto dto)
        {
            var response = new ResponseDto<PartCategoryDto>();

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok)
            {
                response.AddError(tenantResult.Error);
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Code))
            {
                response.AddError("Name and Code are required.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var tenantId = tenantResult.TenantId;
            var code = dto.Code.Trim();
            if (await _context.PartCategories.AnyAsync(x => x.TenantId == tenantId && x.Code == code))
            {
                response.AddError("Part Category code already exists for this tenant.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            var entity = new PartCategory
            {
                TenantId = tenantId,
                Name = dto.Name.Trim(),
                Code = code,
                Description = dto.Description,
                IsActive = dto.IsActive
            };

            _context.PartCategories.Add(entity);
            await _context.SaveChangesAsync();

            response.Result = await MapToDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<PartCategoryDto>> UpdateAsync(UpdatePartCategoryDto dto)
        {
            var response = new ResponseDto<PartCategoryDto>();

            var entity = await _context.PartCategories.FindAsync(dto.Id);
            if (entity == null)
            {
                response.AddError("Part Category not found.");
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

            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Code))
            {
                response.AddError("Name and Code are required.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var tenantId = tenantResult.TenantId;
            var code = dto.Code.Trim();
            if (await _context.PartCategories.AnyAsync(x => x.TenantId == tenantId && x.Code == code && x.Id != dto.Id))
            {
                response.AddError("Part Category code already exists for this tenant.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            entity.TenantId = tenantId;
            entity.Name = dto.Name.Trim();
            entity.Code = code;
            entity.Description = dto.Description;
            entity.IsActive = dto.IsActive;

            _context.PartCategories.Update(entity);
            await _context.SaveChangesAsync();

            response.Result = await MapToDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<PartCategoryDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<PartCategoryDto>();
            var dto = await MapToDtoAsync(id);
            if (dto == null)
            {
                response.AddError("Part Category not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<List<PartCategoryDto>>> GetAllAsync(int? tenantId = null)
        {
            var response = new ResponseDto<List<PartCategoryDto>>();

            var query = BaseQuery();
            query = TenantScopeHelper.ApplyTenantScope(query, _tenantProvider, tenantId);
            query = query.Where(x => x.IsActive);

            var result = await query
                .OrderBy(x => x.Name)
                .Select(ProjectToDto())
                .ToListAsync();

            response.Result = result;
            return response;
        }

        public async Task<ResponseDto<List<PartCategoryDto>>> FilterAsync(SearchViewDto model)
        {
            var response = new ResponseDto<List<PartCategoryDto>>();

            var query = BaseQuery();
            query = TenantScopeHelper.ApplyTenantScope(query, _tenantProvider, model.TenantId);

            query = query.Where(x =>
                (string.IsNullOrEmpty(model.SearchText) ||
                 x.Name.Contains(model.SearchText) ||
                 x.Code.Contains(model.SearchText) ||
                 x.Description.Contains(model.SearchText)) &&
                (!model.IsActive.HasValue || x.IsActive == model.IsActive) &&
                (!model.StartDate.HasValue || x.CreatedOn >= model.StartDate.Value.Date) &&
                (!model.EndDate.HasValue || x.CreatedOn <= model.EndDate.Value.Date));

            var projected = query.Select(ProjectToDto());

            model.OrderByProp ??= nameof(PartCategory.Name);

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

            var entity = await _context.PartCategories.FindAsync(id);
            if (entity == null)
            {
                response.AddError("Part Category not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            if (await _context.Parts.AnyAsync(x => x.PartCategoryId == id))
            {
                response.AddError("Cannot delete Part Category that has Parts.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            entity.IsDeleted = true;
            entity.IsActive = false;
            await _context.SaveChangesAsync();

            response.Result = true;
            return response;
        }

        private IQueryable<PartCategory> BaseQuery()
        {
            return _context.PartCategories.Include(x => x.Tenant);
        }

        private static System.Linq.Expressions.Expression<Func<PartCategory, PartCategoryDto>> ProjectToDto()
        {
            return x => new PartCategoryDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,
                Name = x.Name,
                Code = x.Code,
                Description = x.Description,
                IsActive = x.IsActive
            };
        }

        private async Task<PartCategoryDto> MapToDtoAsync(int id)
        {
            return await BaseQuery()
                .Where(x => x.Id == id)
                .Select(ProjectToDto())
                .FirstOrDefaultAsync();
        }
    }
}
