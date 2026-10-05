using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.SparePart;

namespace AssetFlow.Services.Contracts
{
    public interface IPartReplacementService
    {
        Task<ResponseDto<PartReplacementValidationResultDto>> ValidateAsync(ValidatePartReplacementDto dto);
        Task<ResponseDto<PartReplacementDto>> ReplaceAsync(ConfirmPartReplacementDto dto);
        Task<ResponseDto<PartReplacementDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<PartReplacementDto>>> GetByWorkOrderAsync(int workOrderId);
        Task<ResponseDto<PartReplacementValidationResultDto>> LookupSerialAsync(int workOrderId, string serialNumber);
    }
}
