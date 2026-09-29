using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.SparePart;

namespace AssetFlow.Services.Contracts
{
    public interface ISupplierService
    {
        Task<ResponseDto<SupplierDto>> CreateAsync(CreateSupplierDto dto);
        Task<ResponseDto<SupplierDto>> UpdateAsync(UpdateSupplierDto dto);
        Task<ResponseDto<SupplierDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<SupplierDto>>> GetAllAsync(int? tenantId = null);
        Task<ResponseDto<List<SupplierDto>>> FilterAsync(SearchViewDto model);
        Task<ResponseDto<bool>> DeleteAsync(int id);
    }
}
