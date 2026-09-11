using RequestFlow.Services.Dto;
using RequestFlow.Services.Dto.MetaData;

namespace RequestFlow.Services.Contracts
{
    public interface IMetaDataService
    {
        ResponseDto<ListMetaDataResponseDto> GetMetaDataValues(MetaDataRequestDto model);
        ResponseDto<object> GetAllEnums();
        ResponseDto<List<MetaDataViewDto>> MetaDataKeys();
    }
}
