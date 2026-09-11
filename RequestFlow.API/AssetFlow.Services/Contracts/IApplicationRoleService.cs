using RequestFlow.Services.Dto;
using RequestFlow.Services.Dto.Role;

namespace RequestFlow.Services.Contracts
{
    public interface IApplicationRoleService
    {
        Task<ResponseDto<RoleDto>> CreateAsync(CreateRoleDto dto);
        Task<ResponseDto<RoleDto>> UpdateAsync(UpdateRoleDto dto);
        Task<ResponseDto<List<RoleDto>>> GetAllAsync();
        Task<ResponseDto<List<RoleDto>>> FilterAsync(RequestFlow.Services.Dto.SearchViewDto model);
        Task<ResponseDto<List<RoleWithResourcesDto>>> GetRolesByTenantAsync(int? tenantId);
    }
}
