using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.SparePart;

namespace AssetFlow.Services.Contracts
{
    public interface IPartSerialNumberService
    {
        Task<ResponseDto<PartSerialNumberDto>> UpdateAsync(UpdatePartSerialNumberDto dto);
        Task<ResponseDto<PartSerialNumberDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<PartSerialNumberDto>>> FilterAsync(PartSerialNumberFilterDto model);
        Task<ResponseDto<bool>> DeleteAsync(int id);
        Task<ResponseDto<NextPartSerialDto>> GetNextSerialAsync(int partId, int? tenantId);
        Task<ResponseDto<bool>> SerialExistsAsync(string serial, int? tenantId);
    }
}
