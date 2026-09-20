using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Location;

namespace AssetFlow.Services.Contracts
{
    public interface ILocationService
    {
        Task<ResponseDto<LocationDto>> CreateAsync(CreateLocationDto dto);
        Task<ResponseDto<LocationDto>> UpdateAsync(UpdateLocationDto dto);
        Task<ResponseDto<LocationDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<LocationDto>>> GetAllAsync();
        Task<ResponseDto<List<LocationDto>>> FilterAsync(LocationFilterDto model);
        Task<ResponseDto<List<LocationDto>>> GetByLocationTypeIdAsync(int locationTypeId, int? tenantId = null);
        Task<ResponseDto<bool>> DeleteAsync(int id);
    }
}
