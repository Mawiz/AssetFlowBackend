using Microsoft.EntityFrameworkCore;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.ACL;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Role.RoleResource;
using AssetFlow.Services.Dto;
using System.Linq.Dynamic.Core;
using X.PagedList;
using System.Net;

namespace AssetFlow.Services.Core
{
    public class ResourceService : IResourceService
    {
        private readonly ApplicationDbContext _context;

        public ResourceService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Create Feature + SubResources
        public async Task<ResponseDto<ResourceDto>> CreateAsync(CreateResourceDto dto)
        {
            var response = new ResponseDto<ResourceDto>();

            if (await _context.Resources.AnyAsync(x => x.ResourceName == dto.ResourceName && x.FeatureId == null))
            {
                response.AddError("Feature already exists with this name.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            var feature = new Resource
            {
                ResourceName = dto.ResourceName,
                Verb = dto.Verb,
                IsBackEnd = dto.IsBackEnd
            };

            // Add SubResources
            foreach (var subDto in dto.SubResources ?? new List<CreateSubResourceDto>())
            {
                feature.SubResources.Add(new Resource
                {
                    ResourceName = subDto.ResourceName,
                    Verb = subDto.Verb,
                    IsBackEnd = subDto.IsBackEnd
                });
            }

            _context.Resources.Add(feature);
            await _context.SaveChangesAsync();

            response.Result = MapToDto(feature);
            return response;
        }

        // 🔹 Update Feature + SubResources
        public async Task<ResponseDto<ResourceDto>> UpdateAsync(UpdateResourceDto dto)
        {
            var response = new ResponseDto<ResourceDto>();

            var feature = await _context.Resources
                .Include(f => f.SubResources)
                .FirstOrDefaultAsync(f => f.Id == dto.Id && f.FeatureId == null);

            if (feature == null)
            {
                response.AddError("Feature not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            // Update main feature
            feature.ResourceName = dto.ResourceName;
            feature.Verb = dto.Verb;
            feature.IsBackEnd = dto.IsBackEnd;

            var incoming = dto.SubResources ?? new List<UpdateSubResourceDto>();

            // Build set of incoming existing Ids
            var incomingIds = new HashSet<int>(incoming.Where(s => s.Id > 0).Select(s => s.Id));

            // Remove SubResources that are not present in incoming DTO (deleted by client)
            var toRemove = feature.SubResources
                .Where(sr => !incomingIds.Contains(sr.Id))
                .ToList();

            foreach (var rem in toRemove)
            {
                // Remove from parent collection and mark for deletion in EF
                feature.SubResources.Remove(rem);
                _context.Resources.Remove(rem);
            }

            // Update or Add SubResources
            foreach (var subDto in incoming)
            {
                if (subDto.Id > 0)
                {
                    var existing = feature.SubResources.FirstOrDefault(x => x.Id == subDto.Id);
                    if (existing != null)
                    {
                        existing.ResourceName = subDto.ResourceName;
                        existing.Verb = subDto.Verb;
                        existing.IsBackEnd = subDto.IsBackEnd;
                    }
                }
                else
                {
                    // Add new SubResource
                    feature.SubResources.Add(new Resource
                    {
                        ResourceName = subDto.ResourceName,
                        Verb = subDto.Verb,
                        IsBackEnd = subDto.IsBackEnd
                    });
                }
            }

            await _context.SaveChangesAsync();
            response.Result = MapToDto(feature);
            return response;
        }

        // 🔹 Get All Features with their SubResources
        public async Task<ResponseDto<List<ResourceDto>>> GetAllAsync()
        {
            var response = new ResponseDto<List<ResourceDto>>();

            var list = await _context.Resources
                .Where(x => x.FeatureId == null)
                .Include(x => x.SubResources)
                .Select(x => MapToDto(x))
                .ToListAsync();

            response.Result = list;
            return response;
        }

        // 4C3 Filter + Pagination for Resources (top-level features)
        public async Task<ResponseDto<List<ResourceDto>>> FilterAsync(SearchViewDto model)
        {
            var response = new ResponseDto<List<ResourceDto>>();

            var query = _context.Resources
                .Where(x => (string.IsNullOrEmpty(model.SearchText) || x.ResourceName.Contains(model.SearchText)) &&
                    //(!model.StartDate.HasValue || x.CreatedOn >= model.StartDate.Value.Date) &&
                    //(!model.EndDate.HasValue || x.CreatedOn >= model.EndDate.Value.Date) &&
                    //(!model.IsActive.HasValue || x == model.IsActive) &&
                    x.FeatureId == null)
                .Include(x => x.SubResources)
                .Select(entity => new ResourceDto
                {
                    Id = entity.Id,
                    ResourceName = entity.ResourceName,
                    Verb = entity.Verb,
                    IsBackEnd = entity.IsBackEnd,
                    SubResources = entity.SubResources.Select(sr => new SubResourceDto
                    {
                        Id = sr.Id,
                        ResourceName = sr.ResourceName,
                        Verb = sr.Verb,
                        IsBackEnd = sr.IsBackEnd
                    }).ToList()
                });

            model.OrderByProp ??=nameof(Resource.Id);

            var ordered = model.SortDirection == (int)AssetFlow.Common.Enum.Enums.OrderBy.Ascending
                ? query.OrderBy(model.OrderByProp)
                : query.OrderBy($"{model.OrderByProp} descending");

            var paged = await ordered.ToPagedList(pageNumber: model.PageNumber, pageSize: model.PageSize).ToListAsync();

            response.Result = paged;
            return response;
        }

        private static ResourceDto MapToDto(Resource entity)
        {
            return new ResourceDto
            {
                Id = entity.Id,
                ResourceName = entity.ResourceName,
                Verb = entity.Verb,
                IsBackEnd = entity.IsBackEnd,
                SubResources = entity.SubResources?.Select(sr => new SubResourceDto
                {
                    Id = sr.Id,
                    ResourceName = sr.ResourceName,
                    Verb = sr.Verb,
                    IsBackEnd = sr.IsBackEnd
                }).ToList()
            };
        }
    }
}
