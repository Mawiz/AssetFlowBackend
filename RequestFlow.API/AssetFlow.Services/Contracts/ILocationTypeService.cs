using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Location;

namespace AssetFlow.Services.Contracts
{
    public interface ILocationTypeService
    {
        Task<ResponseDto<LocationTypeDto>> CreateAsync(CreateLocationTypeDto dto);
        Task<ResponseDto<LocationTypeDto>> UpdateAsync(UpdateLocationTypeDto dto);
        Task<ResponseDto<LocationTypeDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<LocationTypeDto>>> GetAllAsync();
        Task<ResponseDto<List<LocationTypeDto>>> FilterAsync(SearchViewDto model);
        Task<ResponseDto<bool>> DeleteAsync(int id);
    }
}
