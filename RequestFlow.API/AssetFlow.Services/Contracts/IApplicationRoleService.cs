using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Role;

namespace AssetFlow.Services.Contracts
{
    public interface IApplicationRoleService
    {
        Task<ResponseDto<RoleDto>> CreateAsync(CreateRoleDto dto);
        Task<ResponseDto<RoleDto>> UpdateAsync(UpdateRoleDto dto);
        Task<ResponseDto<List<RoleDto>>> GetAllAsync();
        Task<ResponseDto<List<RoleDto>>> FilterAsync(AssetFlow.Services.Dto.SearchViewDto model);
        Task<ResponseDto<List<RoleWithResourcesDto>>> GetRolesByTenantAsync(int? tenantId);
    }
}
