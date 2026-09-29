using Microsoft.EntityFrameworkCore;
using AssetFlow.Common.Enum;
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
    public class PartSerialNumberService : IPartSerialNumberService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public PartSerialNumberService(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<ResponseDto<PartSerialNumberDto>> UpdateAsync(UpdatePartSerialNumberDto dto)
        {
            var response = new ResponseDto<PartSerialNumberDto>();
            var entity = await _context.PartSerialNumbers.FindAsync(dto.Id);
            if (entity == null) { response.AddError("Serial number record not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }

            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, dto.TenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var err = await ValidateAsync(dto, tenantResult.TenantId, dto.Id);
            if (err != null) { response.AddError(err); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            entity.Status = dto.Status;
            entity.ReceivedDate = dto.ReceivedDate;
            entity.LocationId = dto.LocationId;
            entity.WarrantyStartDate = dto.WarrantyStartDate;
            entity.WarrantyEndDate = dto.WarrantyEndDate;
            entity.IsActive = dto.IsActive;
            _context.PartSerialNumbers.Update(entity);
            await _context.SaveChangesAsync();
            response.Result = await MapToDtoAsync(entity.Id);
            return response;
        }

        public async Task<ResponseDto<PartSerialNumberDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<PartSerialNumberDto>();
            var dto = await MapToDtoAsync(id);
            if (dto == null) { response.AddError("Serial number record not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            response.Result = dto;
            return response;
        }

        public async Task<ResponseDto<List<PartSerialNumberDto>>> FilterAsync(PartSerialNumberFilterDto model)
        {
            var response = new ResponseDto<List<PartSerialNumberDto>>();
            IQueryable<PartSerialNumber> query = BuildQuery();
            query = TenantScopeHelper.ApplyTenantScope(query, _tenantProvider, model.TenantId);

            if (!model.PartId.HasValue)
            {
                response.AddError("Part is required."); response.StatusCode = HttpStatusCode.BadRequest; return response;
            }

            query = query.Where(x => x.PartId == model.PartId);
            if (!string.IsNullOrWhiteSpace(model.SearchText))
            {
                var t = model.SearchText;
                query = query.Where(x => x.SerialNumber.Contains(t));
            }

            query = query.Where(x => !model.IsActive.HasValue || x.IsActive == model.IsActive);
            var projected = query.Select(ProjectToDto());
            model.OrderByProp ??= nameof(PartSerialNumber.SerialNumber);
            var ordered = model.SortDirection == (int)Enums.OrderBy.Ascending
                ? projected.OrderBy(model.OrderByProp)
                : projected.OrderBy($"{model.OrderByProp} descending");

            var paged = await ordered.ToPagedListAsync(model.PageNumber, model.PageSize);
            response.Result = paged.ToList();
            return response;
        }

        public async Task<ResponseDto<bool>> DeleteAsync(int id)
        {
            var response = new ResponseDto<bool>();
            var entity = await _context.PartSerialNumbers.FindAsync(id);
            if (entity == null) { response.AddError("Serial number record not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            entity.IsDeleted = true;
            entity.IsActive = false;
            await _context.SaveChangesAsync();
            response.Result = true;
            return response;
        }

        public async Task<ResponseDto<NextPartSerialDto>> GetNextSerialAsync(int partId, int? tenantId)
        {
            var response = new ResponseDto<NextPartSerialDto>();
            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, tenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var part = await _context.Parts.FindAsync(partId);
            if (part == null) { response.AddError("Part not found."); response.StatusCode = HttpStatusCode.NotFound; return response; }
            if (part.TenantId != tenantResult.TenantId) { response.AddError("Part tenant mismatch."); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            var next = await PartSerialNumberGenerator.GetNextSerialAsync(_context, partId, tenantResult.TenantId);
            response.Result = new NextPartSerialDto { SerialNumber = next };
            return response;
        }

        public async Task<ResponseDto<bool>> SerialExistsAsync(string serial, int? tenantId)
        {
            var response = new ResponseDto<bool>();
            var tenantResult = TenantScopeHelper.ResolveWriteTenantId(_tenantProvider, tenantId);
            if (!tenantResult.Ok) { response.AddError(tenantResult.Error); response.StatusCode = HttpStatusCode.BadRequest; return response; }

            if (string.IsNullOrWhiteSpace(serial))
            {
                response.AddError("Serial number is required.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            response.Result = await PartSerialNumberGenerator.SerialExistsAsync(_context, tenantResult.TenantId, serial);
            return response;
        }

        private async Task<string?> ValidateAsync(CreatePartSerialNumberDto dto, int? tenantId, int? id)
        {
            if (dto.WarrantyStartDate.HasValue && dto.WarrantyEndDate.HasValue && dto.WarrantyEndDate < dto.WarrantyStartDate)
                return "Warranty end date must be on or after start date.";
            if (dto.LocationId.HasValue)
            {
                var loc = await _context.Locations.FindAsync(dto.LocationId.Value);
                if (loc == null || loc.TenantId != tenantId) return "Invalid location.";
            }
            return null;
        }

        private IQueryable<PartSerialNumber> BuildQuery() =>
            _context.PartSerialNumbers.Include(x => x.Part).Include(x => x.Location).Include(x => x.Tenant);

        private static System.Linq.Expressions.Expression<Func<PartSerialNumber, PartSerialNumberDto>> ProjectToDto() =>
            x => new PartSerialNumberDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                TenantName = x.Tenant != null ? x.Tenant.CompanyName : null,
                PartId = x.PartId,
                PartNumber = x.Part.PartNumber,
                PartName = x.Part.PartName,
                SerialNumber = x.SerialNumber,
                Status = x.Status,
                ReceivedDate = x.ReceivedDate,
                LocationId = x.LocationId,
                LocationName = x.Location != null ? x.Location.Name : null,
                WarrantyStartDate = x.WarrantyStartDate,
                WarrantyEndDate = x.WarrantyEndDate,
                IsActive = x.IsActive
            };

        private async Task<PartSerialNumberDto?> MapToDtoAsync(int id) =>
            await BuildQuery().Where(x => x.Id == id).Select(ProjectToDto()).FirstOrDefaultAsync();
    }
}
