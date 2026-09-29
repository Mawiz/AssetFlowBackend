using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.SparePart;

namespace AssetFlow.Services.Contracts
{
    public interface IPartCategoryService
    {
        Task<ResponseDto<PartCategoryDto>> CreateAsync(CreatePartCategoryDto dto);
        Task<ResponseDto<PartCategoryDto>> UpdateAsync(UpdatePartCategoryDto dto);
        Task<ResponseDto<PartCategoryDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<PartCategoryDto>>> GetAllAsync(int? tenantId = null);
        Task<ResponseDto<List<PartCategoryDto>>> FilterAsync(SearchViewDto model);
        Task<ResponseDto<bool>> DeleteAsync(int id);
    }
}
