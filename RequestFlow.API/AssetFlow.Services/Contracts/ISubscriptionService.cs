using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetFlow.Services.Contracts
{
    public interface ISubscriptionService
    {
        Task<ResponseDto<SubscriptionTypeDto>> CreateAsync(CreateSubscriptionTypeDto dto);
        Task<ResponseDto<SubscriptionTypeDto>> UpdateAsync(UpdateSubscriptionTypeDto dto);
        Task<ResponseDto<SubscriptionTypeDto>> GetByIdAsync(int id);
        Task<ResponseDto<List<SubscriptionTypeDto>>> GetAllAsync();
        Task<ResponseDto<List<SubscriptionTypeDto>>> FilterAsync(AssetFlow.Services.Dto.SearchViewDto model);
        Task<ResponseDto<bool>> ToggleStatusAsync(int id);
        Task<ResponseDto<bool>> DeleteAsync(int id);
    }
}
