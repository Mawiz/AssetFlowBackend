using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Issue;

namespace AssetFlow.Services.Contracts
{
    public interface IIssueCategoryService
    {
        Task<ResponseDto<IssueCategoryDto>> CreateAsync(CreateIssueCategoryDto dto);
        Task<ResponseDto<IssueCategoryDto>> UpdateAsync(UpdateIssueCategoryDto dto);
        Task<ResponseDto<IssueCategoryDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<IssueCategoryDto>>> GetAllAsync(int? tenantId);
        Task<ResponseDto<object>> FilterAsync(IssueCategoryFilterDto model);
        Task<ResponseDto<bool>> DeleteAsync(int id);
    }
}
