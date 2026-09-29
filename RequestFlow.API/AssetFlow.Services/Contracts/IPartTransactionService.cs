using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.SparePart;

namespace AssetFlow.Services.Contracts
{
    public interface IPartTransactionService
    {
        Task<ResponseDto<PartTransactionDto>> CreateAsync(CreatePartTransactionDto dto);
        Task<ResponseDto<List<PartTransactionDto>>> FilterAsync(PartTransactionFilterDto model);
        Task<ResponseDto<bool>> DeleteAsync(int id);
    }
}
