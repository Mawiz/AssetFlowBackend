using RequestFlow.Services.Dto;
using RequestFlow.Services.Dto.Role.RoleResource;

namespace RequestFlow.Services.Contracts
{
    public interface IResourceService
    {
        Task<ResponseDto<ResourceDto>> CreateAsync(CreateResourceDto dto);
        Task<ResponseDto<ResourceDto>> UpdateAsync(UpdateResourceDto dto);
        Task<ResponseDto<List<ResourceDto>>> GetAllAsync();
        Task<ResponseDto<List<ResourceDto>>> FilterAsync(RequestFlow.Services.Dto.SearchViewDto model);
    }
}
