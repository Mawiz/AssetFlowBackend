using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.MetaData;

namespace AssetFlow.Services.Contracts
{
    public interface IMetaDataService
    {
        ResponseDto<ListMetaDataResponseDto> GetMetaDataValues(MetaDataRequestDto model);
        ResponseDto<object> GetAllEnums();
        ResponseDto<List<MetaDataViewDto>> MetaDataKeys();
    }
}
