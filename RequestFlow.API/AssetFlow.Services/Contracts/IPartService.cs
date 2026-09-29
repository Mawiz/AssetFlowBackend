using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.SparePart;

namespace AssetFlow.Services.Contracts
{
    public interface IPartService
    {
        Task<ResponseDto<PartDto>> CreateAsync(CreatePartDto dto);
        Task<ResponseDto<PartDto>> UpdateAsync(UpdatePartDto dto);
        Task<ResponseDto<PartDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<PartDto>>> FilterAsync(PartFilterDto model);
        Task<ResponseDto<bool>> DeleteAsync(int id);
    }
}
