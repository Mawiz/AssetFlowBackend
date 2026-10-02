using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.WorkOrder;
using Microsoft.AspNetCore.Http;

namespace AssetFlow.Services.Contracts
{
    public interface IWorkOrderService
    {
        Task<ResponseDto<WorkOrderDto>> CreateFromIssueAsync(CreateWorkOrderFromIssueDto dto);
        Task<ResponseDto<WorkOrderDto>> CreateFromOccurrenceAsync(CreateWorkOrderFromOccurrenceDto dto);
        Task<ResponseDto<WorkOrderDto>> UpdateAsync(UpdateWorkOrderDto dto);
        Task<ResponseDto<WorkOrderDto>> GetByIdAsync(int id);
        Task<ResponseDto<object>> FilterAsync(WorkOrderFilterDto model);
        Task<ResponseDto<bool>> DeleteAsync(int id);

        Task<ResponseDto<WorkOrderDto>> AssignAsync(WorkOrderAssignDto dto);
        Task<ResponseDto<WorkOrderDto>> ReassignAsync(WorkOrderAssignDto dto);
        Task<ResponseDto<WorkOrderDto>> AcceptAsync(WorkOrderActionRemarksDto dto);
        Task<ResponseDto<WorkOrderDto>> EngineerArrivedAsync(WorkOrderEngineerArrivedDto dto);
        Task<ResponseDto<WorkOrderDto>> StartAsync(WorkOrderActionRemarksDto dto);
        Task<ResponseDto<WorkOrderDto>> PauseAsync(WorkOrderActionRemarksDto dto);
        Task<ResponseDto<WorkOrderDto>> WaitingForPartsAsync(WorkOrderActionRemarksDto dto);
        Task<ResponseDto<WorkOrderDto>> ResumeAsync(WorkOrderActionRemarksDto dto);
        Task<ResponseDto<WorkOrderDto>> UpsertDiagnosisAsync(WorkOrderDiagnosisUpsertDto dto);
        Task<ResponseDto<WorkOrderDto>> UpdateWorkPerformedAsync(WorkOrderWorkPerformedDto dto);
        Task<ResponseDto<WorkOrderDto>> CompleteAsync(WorkOrderCompleteDto dto);
        Task<ResponseDto<WorkOrderDto>> ApproveAsync(WorkOrderApproveDto dto);
        Task<ResponseDto<WorkOrderDto>> RejectAsync(WorkOrderRejectDto dto);
        Task<ResponseDto<WorkOrderDto>> ReopenAsync(WorkOrderReopenDto dto);
        Task<ResponseDto<WorkOrderDto>> CancelAsync(WorkOrderCancelDto dto);

        Task<ResponseDto<WorkOrderAttachmentDto>> AddAttachmentAsync(int workOrderId, IFormFile file);
        Task<ResponseDto<bool>> DeleteAttachmentAsync(int attachmentId);
        Task<(Stream Stream, string ContentType, string FileName)?> GetAttachmentStreamAsync(int attachmentId);
    }
}
