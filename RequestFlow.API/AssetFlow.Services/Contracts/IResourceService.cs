using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Role.RoleResource;

namespace AssetFlow.Services.Contracts
{
    public interface IResourceService
    {
        Task<ResponseDto<ResourceDto>> CreateAsync(CreateResourceDto dto);
        Task<ResponseDto<ResourceDto>> UpdateAsync(UpdateResourceDto dto);
        Task<ResponseDto<List<ResourceDto>>> GetAllAsync();
        Task<ResponseDto<List<ResourceDto>>> FilterAsync(AssetFlow.Services.Dto.SearchViewDto model);
    }
}
