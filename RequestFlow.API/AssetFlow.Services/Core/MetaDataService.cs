using AssetFlow.Common;

using AssetFlow.Common.Enum;

using AssetFlow.Data.Data;

using AssetFlow.Data.Entities;

using AssetFlow.Data.Entities.Asset;

using AssetFlow.Data.Entities.SparePart;

using AssetFlow.Data.Entities.Tenant;

using AssetFlow.Data.Identity;

using AssetFlow.Data.Provider;

using AssetFlow.Services.Contracts;

using AssetFlow.Services.Dto;

using AssetFlow.Services.Dto.MetaData;

using Microsoft.EntityFrameworkCore;

using System.Collections;

using System.Net;

using LocationEntity = AssetFlow.Data.Entities.Location.Location;

using LocationTypeEntity = AssetFlow.Data.Entities.Location.LocationType;



namespace AssetFlow.Services.Core

{

    public class MetaDataService : IMetaDataService

    {

        private readonly ApplicationDbContext appDbContext;

        private readonly ITenantProvider tenantProvider;



        public MetaDataService(ApplicationDbContext appDbContext, ITenantProvider tenantProvider)

        {

            this.appDbContext = appDbContext;

            this.tenantProvider = tenantProvider;

        }



        public ResponseDto<ListMetaDataResponseDto> GetMetaDataValues(MetaDataRequestDto model)

        {

            var response = new ResponseDto<ListMetaDataResponseDto>()

            { Result = new ListMetaDataResponseDto() };



            var modifiedOn = new List<DateTime>();





            foreach (var key in model.SecretKeys)

            {

                switch (key)

                {

                    #region UserManagement 



                    case nameof(ApplicationRole):

                        {

                            var roleQuery = appDbContext.Roles.AsQueryable();



                            if (model.TenantId.HasValue)

                            {

                                roleQuery = model.TenantId == 0

                                    ? roleQuery.Where(r => r.TenantId == null)

                                    : roleQuery.Where(r => r.TenantId == model.TenantId);

                            }



                            var result = new MetaDataResponseDto

                            {

                                Key = nameof(ApplicationRole),

                                Data = roleQuery

                                       .Where(r => !model.LatestByDate.HasValue ||

                                              r.ModifiedOn > model.LatestByDate.Value)

                                       .OrderByDescending(r => r.ModifiedOn)

                                       .Select(r => new

                                       {

                                           r.Id,

                                           r.Name,

                                           r.DisplayName,

                                           r.Description,

                                           r.ModifiedOn,

                                           r.Order,

                                           r.TenantId

                                       })

                                       .ToList()

                            };



                            GetLatestModifiedOn(result.Data, modifiedOn);



                            response.Result.MetaResult.Add(result);

                            break;

                        }



                    #endregion UserManagement



                    case nameof(Language):

                        {

                            var result = new MetaDataResponseDto

                            {

                                Key = nameof(Language),

                                Data = appDbContext.Languages

                                       .Where(r => !model.LatestByDate.HasValue ||

                                              r.ModifiedOn > model.LatestByDate.Value)

                                       .OrderByDescending(r => r.ModifiedOn)

                                       .Select(r => new

                                       {

                                           r.Id,

                                           r.Name,

                                           r.DisplayName,

                                           r.Code,

                                           r.ModifiedOn,

                                           r.Order

                                       })

                                       .ToList()

                            };



                            GetLatestModifiedOn(result.Data, modifiedOn);



                            response.Result.MetaResult.Add(result);

                            break;

                        }



                    case nameof(SubscriptionType):

                        {

                            var result = new MetaDataResponseDto

                            {

                                Key = nameof(SubscriptionType),

                                Data = appDbContext.SubscriptionTypes

                                       .Where(r => !model.LatestByDate.HasValue ||

                                              r.ModifiedOn > model.LatestByDate.Value)

                                       .OrderByDescending(r => r.ModifiedOn)

                                       .Select(r => new

                                       {

                                           r.Id,

                                           r.Name,

                                           r.DisplayName,

                                           r.ModifiedOn,

                                           r.Order

                                       })

                                       .ToList()

                            };



                            GetLatestModifiedOn(result.Data, modifiedOn);



                            response.Result.MetaResult.Add(result);

                            break;

                        }



                    case nameof(Tenant):

                        {

                            var result = new MetaDataResponseDto

                            {

                                Key = nameof(Tenant),

                                Data = appDbContext.Tenants

                                       .Where(r => !model.LatestByDate.HasValue ||

                                              r.ModifiedOn > model.LatestByDate.Value)

                                       .OrderByDescending(r => r.ModifiedOn)

                                       .Select(r => new

                                       {

                                           r.Id,

                                           Name = r.CompanyName,

                                           DisplayName = r.CompanyName,

                                           r.ModifiedOn,

                                       })

                                       .ToList()

                            };



                            GetLatestModifiedOn(result.Data, modifiedOn);



                            response.Result.MetaResult.Add(result);

                            break;

                        }



                    case nameof(ApplicationUser):

                        {

                            var userQuery = appDbContext.ApplicationUsers.AsQueryable();



                            if (model.TenantId.HasValue)

                            {

                                userQuery = model.TenantId == 0

                                    ? userQuery.Where(u => u.TenantId == null)

                                    : userQuery.Where(u => u.TenantId == model.TenantId);

                            }



                            var result = new MetaDataResponseDto

                            {

                                Key = nameof(ApplicationUser),

                                Data = userQuery

                                    .Where(u => u.IsActive &&

                                        (!model.LatestByDate.HasValue ||

                                         (u.ModifiedOn ?? u.CreatedOn) > model.LatestByDate.Value))

                                    .OrderByDescending(u => u.ModifiedOn ?? u.CreatedOn)

                                    .Select(u => new

                                    {

                                        u.Id,

                                        Name = u.UserName,

                                        DisplayName = u.FullName ?? u.UserName,

                                        ModifiedOn = u.ModifiedOn ?? u.CreatedOn,

                                        u.TenantId

                                    })

                                    .ToList()

                            };



                            GetLatestModifiedOn(result.Data, modifiedOn);

                            response.Result.MetaResult.Add(result);

                            break;

                        }



                    default:

                        {

                            response.AddError(string.Format(AppResource.NotFound, key));

                            break;

                        }

                }

            }



            if (modifiedOn.Any())

            {

                response.Result.ModifiedOn = modifiedOn.OrderByDescending(mo => mo).FirstOrDefault();

            }



            return response;

        }



        public async Task<ResponseDto<List<MetaDataByTypeItemDto>>> GetMetaDataByTypeAsync(MetaDataByTypeRequestDto model)

        {

            var response = new ResponseDto<List<MetaDataByTypeItemDto>>();



            if (string.IsNullOrWhiteSpace(model.Type))

            {

                response.AddError("Type is required.");

                response.StatusCode = HttpStatusCode.BadRequest;

                return response;

            }



            var type = model.Type.Trim();

            var tenantId = ResolveTenantId(model.TenantId);



            switch (type)

            {

                case "Location":

                    response.Result = await GetLocationsByTypeAsync(tenantId, model.ParentId, model.LocationTypeId);

                    break;



                case "LocationType":

                    response.Result = await GetLocationTypesByTypeAsync(tenantId, model.ParentId);

                    break;



                case "AssetCategory":

                    response.Result = await GetAssetCategoriesAsync(tenantId);

                    break;



                case "AssetType":

                    if (!model.ParentId.HasValue)

                    {

                        response.AddError("ParentId (asset category) is required for AssetType.");

                        response.StatusCode = HttpStatusCode.BadRequest;

                        return response;

                    }

                    response.Result = await GetAssetTypesAsync(tenantId, model.ParentId.Value);

                    break;



                case "ApplicationUser":

                    response.Result = await GetUsersAsync(tenantId);

                    break;



                case "Department":

                    // Reserved for future department master; ParentId = location when configured.

                    response.Result = new List<MetaDataByTypeItemDto>();

                    break;

                case "AssetComponent":

                    if (!model.ParentId.HasValue)

                    {

                        response.AddError("ParentId (asset) is required for AssetComponent.");

                        response.StatusCode = HttpStatusCode.BadRequest;

                        return response;

                    }

                    response.Result = await GetAssetComponentsForMetadataAsync(tenantId, model.ParentId.Value);

                    break;

                case "Part":

                    response.Result = await GetPartsForMetadataAsync(tenantId, model.SearchText);

                    break;

                case "PartInventoryBatch":

                    if (!model.ParentId.HasValue)

                    {

                        response.AddError("ParentId (part) is required for PartInventoryBatch.");

                        response.StatusCode = HttpStatusCode.BadRequest;

                        return response;

                    }

                    response.Result = await GetPartInventoryBatchesForMetadataAsync(tenantId, model.ParentId.Value);

                    break;

                default:

                    response.AddError(string.Format(AppResource.NotFound, type));

                    response.StatusCode = HttpStatusCode.NotFound;

                    break;

            }



            return response;

        }



        public ResponseDto<object> GetAllEnums()

        {

            var response = new ResponseDto<object>();



            var enumObject = new Dictionary<string, List<object>>();

            var types = typeof(Enums).GetNestedTypes();

            foreach (Type enumType in types)

            {

                var enumVals = new List<object>();

                foreach (var item in Enum.GetValues(enumType))

                {

                    enumVals.Add(new

                    {

                        text = item.ToString(),

                        value = (int)item

                    });

                }



                enumObject[GetEnumName(enumType.ToString())] = enumVals;

            }



            response.Result = enumObject;

            return response;

        }



        public ResponseDto<List<MetaDataViewDto>> MetaDataKeys()

        {

            var response = new ResponseDto<List<MetaDataViewDto>>();



            response.Result = appDbContext.MetaDataKeysEnums

                .Where(m => m.IsActive)

                .Select(m => new MetaDataViewDto

                {

                    Id = m.Id,

                    Name = m.Name,

                    DisplayName = m.DisplayName,

                    ModifiedOn = m.ModifiedOn,

                })

                .ToList();



            return response;

        }



        #region Private — by type



        private int? ResolveTenantId(int? dtoTenantId)

        {

            var contextTenant = TenantScopeHelper.GetContextTenantId(tenantProvider);

            if (contextTenant.HasValue)

                return contextTenant;



            if (dtoTenantId.HasValue && dtoTenantId != 0)

                return dtoTenantId;



            return null;

        }



        private async Task<List<MetaDataByTypeItemDto>> GetLocationsByTypeAsync(

            int? tenantId,

            int? parentId,

            int? locationTypeId)

        {

            IQueryable<LocationEntity> query = appDbContext.Locations.Where(l => l.IsActive);



            if (tenantId.HasValue)

                query = query.Where(l => l.TenantId == tenantId);



            if (parentId.HasValue)

                query = query.Where(l => l.ParentLocationId == parentId);

            else

                query = query.Where(l => l.ParentLocationId == null);



            if (locationTypeId.HasValue)

                query = query.Where(l => l.LocationTypeId == locationTypeId);



            var items = await query

                .OrderBy(l => l.Name)

                .Select(l => new MetaDataByTypeItemDto

                {

                    Id = l.Id,

                    Name = l.Name,

                    DisplayName = l.Name,

                    ParentId = l.ParentLocationId,

                    LocationTypeId = l.LocationTypeId,

                    TenantId = l.TenantId

                })

                .ToListAsync();



            await ApplyHasChildrenForLocationsAsync(items);

            return items;

        }



        private async Task ApplyHasChildrenForLocationsAsync(List<MetaDataByTypeItemDto> items)

        {

            if (items.Count == 0) return;



            var ids = items.Select(i => i.Id).ToList();

            var withChildren = await appDbContext.Locations

                .Where(c => c.IsActive && c.ParentLocationId.HasValue && ids.Contains(c.ParentLocationId.Value))

                .Select(c => c.ParentLocationId!.Value)

                .Distinct()

                .ToListAsync();



            foreach (var item in items)

                item.HasChildren = withChildren.Contains(item.Id);

        }



        private async Task<List<MetaDataByTypeItemDto>> GetLocationTypesByTypeAsync(int? tenantId, int? parentId)

        {

            IQueryable<LocationTypeEntity> query = appDbContext.LocationTypes.Where(lt => lt.IsActive);



            if (tenantId.HasValue)

                query = query.Where(lt => lt.TenantId == tenantId);



            if (parentId.HasValue)

                query = query.Where(lt => lt.ParentLocationTypeId == parentId);

            else

                query = query.Where(lt => lt.ParentLocationTypeId == null);



            var items = await query

                .OrderBy(lt => lt.SortOrder)

                .ThenBy(lt => lt.Name)

                .Select(lt => new MetaDataByTypeItemDto

                {

                    Id = lt.Id,

                    Name = lt.Name,

                    DisplayName = lt.Name,

                    ParentId = lt.ParentLocationTypeId,

                    TenantId = lt.TenantId

                })

                .ToListAsync();



            if (items.Count == 0) return items;



            var ids = items.Select(i => i.Id).ToList();

            var withChildren = await appDbContext.LocationTypes

                .Where(c => c.IsActive && c.ParentLocationTypeId.HasValue && ids.Contains(c.ParentLocationTypeId.Value))

                .Select(c => c.ParentLocationTypeId!.Value)

                .Distinct()

                .ToListAsync();



            foreach (var item in items)

                item.HasChildren = withChildren.Contains(item.Id);



            return items;

        }



        private async Task<List<MetaDataByTypeItemDto>> GetAssetCategoriesAsync(int? tenantId)

        {

            IQueryable<AssetCategory> query = appDbContext.AssetCategories.Where(c => c.IsActive);

            if (tenantId.HasValue)

                query = query.Where(c => c.TenantId == tenantId);



            return await query

                .OrderBy(c => c.Name)

                .Select(c => new MetaDataByTypeItemDto

                {

                    Id = c.Id,

                    Name = c.Name,

                    DisplayName = c.Name,

                    TenantId = c.TenantId,

                    HasChildren = appDbContext.AssetTypes.Any(t =>

                        t.IsActive && t.AssetCategoryId == c.Id && (!tenantId.HasValue || t.TenantId == tenantId))

                })

                .ToListAsync();

        }



        private async Task<List<MetaDataByTypeItemDto>> GetAssetTypesAsync(int? tenantId, int categoryId)

        {

            IQueryable<AssetType> query = appDbContext.AssetTypes

                .Where(t => t.IsActive && t.AssetCategoryId == categoryId);



            if (tenantId.HasValue)

                query = query.Where(t => t.TenantId == tenantId);



            return await query

                .OrderBy(t => t.Name)

                .Select(t => new MetaDataByTypeItemDto

                {

                    Id = t.Id,

                    Name = t.Name,

                    DisplayName = t.Name,

                    ParentId = t.AssetCategoryId,

                    TenantId = t.TenantId,

                    HasChildren = false

                })

                .ToListAsync();

        }



        private async Task<List<MetaDataByTypeItemDto>> GetAssetComponentsForMetadataAsync(int? tenantId, int assetId)

        {

            IQueryable<AssetComponent> query = appDbContext.AssetComponents

                .Where(c => c.IsActive && c.AssetId == assetId);

            if (tenantId.HasValue)

                query = query.Where(c => c.TenantId == tenantId);

            var rows = await query

                .OrderBy(c => c.ComponentName)

                .Select(c => new

                {

                    c.Id,

                    c.ComponentName,

                    c.PartNumber,

                    c.SerialNumber,

                    c.TenantId,

                    c.AssetId

                })

                .ToListAsync();

            return rows.Select(c =>

            {

                var partNum = c.PartNumber?.Trim() ?? string.Empty;

                var serial = c.SerialNumber?.Trim() ?? string.Empty;

                var pnDisplay = string.IsNullOrEmpty(partNum) ? "—" : partNum;

                var label = $"{c.ComponentName} ({pnDisplay})" + (string.IsNullOrEmpty(serial) ? "" : $" · {serial}");

                return new MetaDataByTypeItemDto

                {

                    Id = c.Id,

                    Name = c.ComponentName,

                    DisplayName = label,

                    Code = partNum,

                    ParentId = c.AssetId,

                    TenantId = c.TenantId,

                    HasChildren = false

                };

            }).ToList();

        }

        private async Task<List<MetaDataByTypeItemDto>> GetPartsForMetadataAsync(int? tenantId, string searchText)

        {

            if (string.IsNullOrWhiteSpace(searchText))

                return new List<MetaDataByTypeItemDto>();

            var pn = searchText.Trim();

            IQueryable<Part> query = appDbContext.Parts.Where(p => p.IsActive && p.PartNumber.ToLower() == pn.ToLower());

            if (tenantId.HasValue)

                query = query.Where(p => p.TenantId == tenantId);

            return await query

                .OrderBy(p => p.PartName)

                .Take(20)

                .Select(p => new MetaDataByTypeItemDto

                {

                    Id = p.Id,

                    Name = p.PartNumber,

                    DisplayName = p.PartName,

                    Code = p.PartNumber,

                    IsSerialized = p.IsSerialized,

                    TenantId = p.TenantId,

                    HasChildren = false

                })

                .ToListAsync();

        }

        private async Task<List<MetaDataByTypeItemDto>> GetPartInventoryBatchesForMetadataAsync(int? tenantId, int partId)

        {

            IQueryable<PartInventoryBatch> query = appDbContext.PartInventoryBatches

                .Where(b => b.IsActive && b.PartId == partId && b.AvailableQuantity > 0);

            if (tenantId.HasValue)

                query = query.Where(b => b.TenantId == tenantId);

            return await query

                .OrderByDescending(b => b.ReceivedDate)

                .Take(50)

                .Select(b => new MetaDataByTypeItemDto

                {

                    Id = b.Id,

                    Name = b.BatchReference ?? ("Batch-" + b.Id),

                    DisplayName = (b.BatchReference ?? ("Batch #" + b.Id)) + " — avail " + b.AvailableQuantity,

                    AvailableQuantity = b.AvailableQuantity,

                    ParentId = b.PartId,

                    TenantId = b.TenantId,

                    HasChildren = false

                })

                .ToListAsync();

        }

        private async Task<List<MetaDataByTypeItemDto>> GetUsersAsync(int? tenantId)

        {

            IQueryable<ApplicationUser> query = appDbContext.ApplicationUsers.Where(u => u.IsActive);



            if (tenantId.HasValue)

                query = query.Where(u => u.TenantId == tenantId);



            return await query

                .OrderBy(u => u.FullName ?? u.UserName)

                .Select(u => new MetaDataByTypeItemDto

                {

                    Id = u.Id,

                    Name = u.UserName,

                    DisplayName = u.FullName ?? u.UserName,

                    TenantId = u.TenantId,

                    HasChildren = false

                })

                .ToListAsync();

        }



        private string GetEnumName(string enumType)

        {

            var index = enumType.LastIndexOf("+");

            var name = enumType.Substring(index + 1);

            return name;

        }



        private static void GetLatestModifiedOn(object data, List<DateTime> modifiedOn)

        {

            var dataToList = ((IEnumerable)data).Cast<object>().ToList();



            if (dataToList.Any())

            {

                var latestmodifiedOn = DateTime.Parse(dataToList.FirstOrDefault().GetType().GetProperty(nameof(BaseModel.ModifiedOn)).GetValue(dataToList.FirstOrDefault(), null).ToString());



                modifiedOn.Add(latestmodifiedOn);

            }

        }



        #endregion Private

    }

}


