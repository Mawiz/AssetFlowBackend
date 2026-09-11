using Microsoft.EntityFrameworkCore;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.ACL;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Role.RoleResource;
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

        public async Task<ResponseDto<ResourceDto>> CreateAsync(CreateResourceDto dto)
        {
            var response = new ResponseDto<ResourceDto>();
            var featureName = dto.ResourceName?.Trim();
            var subNames = (dto.SubResources ?? new List<CreateSubResourceDto>())
                .Select(s => s.ResourceName?.Trim())
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList();

            var validationError = ValidateNames(featureName, subNames);
            if (validationError != null)
            {
                response.AddError(validationError);
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            if (await FeatureNameExistsAsync(featureName, null))
            {
                response.AddError("Feature already exists with this name.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            var feature = new Resource
            {
                ResourceName = featureName,
                Verb = string.Empty
            };

            foreach (var name in subNames)
            {
                feature.SubResources.Add(new Resource
                {
                    ResourceName = name,
                    Verb = string.Empty
                });
            }

            _context.Resources.Add(feature);
            await _context.SaveChangesAsync();

            response.Result = MapToDto(feature);
            return response;
        }

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

            var featureName = dto.ResourceName?.Trim();
            var incoming = dto.SubResources ?? new List<UpdateSubResourceDto>();
            var subNames = incoming
                .Select(s => s.ResourceName?.Trim())
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList();

            var validationError = ValidateNames(featureName, subNames);
            if (validationError != null)
            {
                response.AddError(validationError);
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            if (await FeatureNameExistsAsync(featureName, dto.Id))
            {
                response.AddError("Feature already exists with this name.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            feature.ResourceName = featureName;

            var incomingIds = new HashSet<int>(incoming.Where(s => s.Id > 0).Select(s => s.Id));

            var toRemove = feature.SubResources
                .Where(sr => !incomingIds.Contains(sr.Id))
                .ToList();

            foreach (var rem in toRemove)
            {
                feature.SubResources.Remove(rem);
                _context.Resources.Remove(rem);
            }

            foreach (var subDto in incoming)
            {
                var name = subDto.ResourceName?.Trim();
                if (string.IsNullOrEmpty(name))
                    continue;

                if (subDto.Id > 0)
                {
                    var existing = feature.SubResources.FirstOrDefault(x => x.Id == subDto.Id);
                    if (existing != null)
                        existing.ResourceName = name;
                }
                else
                {
                    feature.SubResources.Add(new Resource
                    {
                        ResourceName = name,
                        Verb = string.Empty
                    });
                }
            }

            await _context.SaveChangesAsync();
            response.Result = MapToDto(feature);
            return response;
        }

        public async Task<ResponseDto<List<ResourceDto>>> GetAllAsync()
        {
            var response = new ResponseDto<List<ResourceDto>>();

            var list = await _context.Resources
                .Where(x => x.FeatureId == null)
                .Include(x => x.SubResources)
                .ToListAsync();

            response.Result = list.Select(MapToDto).ToList();
            return response;
        }

        public async Task<ResponseDto<List<ResourceDto>>> FilterAsync(SearchViewDto model)
        {
            var response = new ResponseDto<List<ResourceDto>>();

            var query = _context.Resources
                .Where(x => (string.IsNullOrEmpty(model.SearchText) || x.ResourceName.Contains(model.SearchText)) &&
                    x.FeatureId == null)
                .Include(x => x.SubResources)
                .Select(entity => new ResourceDto
                {
                    Id = entity.Id,
                    ResourceName = entity.ResourceName,
                    SubResources = entity.SubResources.Select(sr => new SubResourceDto
                    {
                        Id = sr.Id,
                        ResourceName = sr.ResourceName
                    }).ToList()
                });

            model.OrderByProp ??= nameof(Resource.Id);

            var ordered = model.SortDirection == (int)AssetFlow.Common.Enum.Enums.OrderBy.Ascending
                ? query.OrderBy(model.OrderByProp)
                : query.OrderBy($"{model.OrderByProp} descending");

            var paged = await ordered.ToPagedList(pageNumber: model.PageNumber, pageSize: model.PageSize).ToListAsync();

            response.Result = paged;
            return response;
        }

        private async Task<bool> FeatureNameExistsAsync(string featureName, int? excludeId)
        {
            var name = featureName.ToLower();
            return await _context.Resources.AnyAsync(x =>
                x.FeatureId == null &&
                (!excludeId.HasValue || x.Id != excludeId.Value) &&
                x.ResourceName.ToLower() == name);
        }

        private static string ValidateNames(string featureName, List<string> subNames)
        {
            if (string.IsNullOrWhiteSpace(featureName))
                return "Feature name is required.";

            if (subNames.GroupBy(n => n, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1))
                return "A feature cannot have two permissions with the same name.";

            if (subNames.Any(n => n.Equals(featureName, StringComparison.OrdinalIgnoreCase)))
                return "A permission cannot have the same name as its feature.";

            return null;
        }

        private static ResourceDto MapToDto(Resource entity)
        {
            return new ResourceDto
            {
                Id = entity.Id,
                ResourceName = entity.ResourceName,
                SubResources = entity.SubResources?.Select(sr => new SubResourceDto
                {
                    Id = sr.Id,
                    ResourceName = sr.ResourceName
                }).ToList()
            };
        }
    }
}
