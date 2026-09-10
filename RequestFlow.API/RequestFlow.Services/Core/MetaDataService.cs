using RequestFlow.Common;
using RequestFlow.Common.Enum;
using RequestFlow.Data.Data;
using RequestFlow.Data.Entities;
using RequestFlow.Data.Entities.Tenant;
using RequestFlow.Data.Identity;
using RequestFlow.Services.Contracts;
using RequestFlow.Services.Dto;
using RequestFlow.Services.Dto.MetaData;
using System.Collections;

namespace RequestFlow.Services.Core
{
    public class MetaDataService : IMetaDataService
    {
        private readonly ApplicationDbContext appDbContext;
        public MetaDataService(ApplicationDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
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
                            var result = new MetaDataResponseDto
                            {
                                Key = nameof(ApplicationRole),
                                Data = appDbContext.Roles
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
                                           r.Order
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

        //TODO Remove Passing Enums to F.E 
        public ResponseDto<object> GetAllEnums()
        {
            var response = new ResponseDto<object>();

            var enumObject = new Dictionary<string, List<object>>();
            var types = typeof(Enums).GetNestedTypes();
            foreach (Type type in types)
            {
                var enumVals = new List<object>();
                foreach (var item in Enum.GetValues(type))
                {
                    enumVals.Add(new
                    {
                        text = item.ToString(),
                        value = (int)item
                    });
                }

                enumObject[GetEnumName(type.ToString())] = enumVals;
            }

            response.Result = enumObject;
            return response;
        }

        public ResponseDto<List<MetaDataViewDto>> MetaDataKeys()
        {
            var response = new ResponseDto<List<MetaDataViewDto>>();

            var queryable = appDbContext.MetaDataKeysEnums
                            .Select(m => new MetaDataViewDto()
                            {
                                Id = m.Id,
                                Name = m.Name,
                                DisplayName = m.DisplayName,
                                ModifiedOn = m.ModifiedOn,
                            }).ToList();

            response.Result = queryable;
            return response;
        }

        #region Private

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
