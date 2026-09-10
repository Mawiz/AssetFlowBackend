using RequestFlow.Services.Dto;
using RequestFlow.Services.Dto.Tenant;

namespace RequestFlow.Services.Contracts
{
    public interface ITenantService
    {
        Task<ResponseDto<TenantDto>> CreateAsync(CreateTenantDto dto);
        Task<ResponseDto<TenantDto>> UpdateAsync(UpdateTenantDto dto);
        Task<ResponseDto<TenantDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<TenantDto>>> GetAllAsync();
        Task<ResponseDto<List<TenantDto>>> FilterAsync(RequestFlow.Services.Dto.SearchViewDto model);
        Task<ResponseDto<bool>> ToggleStatusAsync(int id);
    }
}
