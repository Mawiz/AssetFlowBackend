using Microsoft.AspNetCore.Mvc;

using AssetFlow.API.Filter;

using AssetFlow.Common.Helper;

using AssetFlow.Services.Contracts;

using AssetFlow.Services.Dto.SparePart;



namespace AssetFlow.API.Controllers

{

    [Route("api/[controller]")]

    [ApiController]

    public class PartInventoryController : BaseAppController

    {

        private readonly IPartInventoryService _service;



        public PartInventoryController(IPartInventoryService service) => _service = service;



        [HttpGet("{id}")]

        [RequirePermission(Permissions.PartInventoryView)]

        public async Task<IActionResult> GetById(int id)

        {

            var response = await _service.GetByIdAsync(id);

            return StatusCode((int)response.StatusCode, response);

        }



        [HttpGet("Batch/{batchId}")]

        [RequirePermission(Permissions.PartInventoryView)]

        public async Task<IActionResult> GetBatch(int batchId)

        {

            var response = await _service.GetBatchByIdAsync(batchId);

            return StatusCode((int)response.StatusCode, response);

        }



        [HttpPost("Filter")]

        [RequirePermission(Permissions.PartInventoryView)]

        public async Task<IActionResult> Filter([FromBody] PartInventoryFilterDto model) => Ok(await _service.FilterAsync(model));



        [HttpPost("Receipt")]

        [RequirePermission(Permissions.PartInventoryCreate)]

        public async Task<IActionResult> Receipt([FromBody] PartReceiptDto dto)

        {

            var response = await _service.ReceiptAsync(dto);

            return StatusCode((int)response.StatusCode, response);

        }



        [HttpPost("BatchReceipt")]

        [RequirePermission(Permissions.PartInventoryCreate)]

        public async Task<IActionResult> BatchReceipt([FromBody] PartBatchReceiptDto dto)

        {

            var response = await _service.BatchReceiptAsync(dto);

            return StatusCode((int)response.StatusCode, response);

        }



        [HttpPost("ReceiveStock")]

        [RequirePermission(Permissions.PartInventoryCreate)]

        public async Task<IActionResult> ReceiveStock([FromBody] PartReceiveStockDto dto)

        {

            var response = await _service.ReceiveStockAsync(dto);

            return StatusCode((int)response.StatusCode, response);

        }



        [HttpPost("Transfer")]

        [RequirePermission(Permissions.PartInventoryUpdate)]

        public async Task<IActionResult> Transfer([FromBody] PartTransferDto dto)

        {

            var response = await _service.TransferAsync(dto);

            return StatusCode((int)response.StatusCode, response);

        }



        [HttpPost("Adjust")]

        [RequirePermission(Permissions.PartInventoryUpdate)]

        public async Task<IActionResult> Adjust([FromBody] PartAdjustmentDto dto)

        {

            var response = await _service.AdjustAsync(dto);

            return StatusCode((int)response.StatusCode, response);

        }



        [HttpPost("Issue")]

        [RequirePermission(Permissions.PartInventoryUpdate)]

        public async Task<IActionResult> Issue([FromBody] PartIssueDto dto)

        {

            var response = await _service.IssueAsync(dto);

            return StatusCode((int)response.StatusCode, response);

        }



        [HttpPost("Return")]

        [RequirePermission(Permissions.PartInventoryUpdate)]

        public async Task<IActionResult> Return([FromBody] PartReturnDto dto)

        {

            var response = await _service.ReturnAsync(dto);

            return StatusCode((int)response.StatusCode, response);

        }



        [HttpPost("MarkFaulty")]

        [RequirePermission(Permissions.PartInventoryUpdate)]

        public async Task<IActionResult> MarkFaulty([FromBody] PartInventoryStateChangeDto dto)

        {

            var response = await _service.MarkFaultyAsync(dto);

            return StatusCode((int)response.StatusCode, response);

        }



        [HttpPost("Quarantine")]

        [RequirePermission(Permissions.PartInventoryUpdate)]

        public async Task<IActionResult> Quarantine([FromBody] PartInventoryStateChangeDto dto)

        {

            var response = await _service.QuarantineAsync(dto);

            return StatusCode((int)response.StatusCode, response);

        }



        [HttpPost("ReleaseFromQuarantine")]

        [RequirePermission(Permissions.PartInventoryUpdate)]

        public async Task<IActionResult> ReleaseFromQuarantine([FromBody] PartInventoryStateChangeDto dto)

        {

            var response = await _service.ReleaseFromQuarantineAsync(dto);

            return StatusCode((int)response.StatusCode, response);

        }



        [HttpPost("ReturnToSupplier")]

        [RequirePermission(Permissions.PartInventoryUpdate)]

        public async Task<IActionResult> ReturnToSupplier([FromBody] PartReturnToSupplierDto dto)

        {

            var response = await _service.ReturnToSupplierAsync(dto);

            return StatusCode((int)response.StatusCode, response);

        }



        [HttpPost("Scrap")]

        [RequirePermission(Permissions.PartInventoryUpdate)]

        public async Task<IActionResult> Scrap([FromBody] PartInventoryStateChangeDto dto)

        {

            var response = await _service.ScrapAsync(dto);

            return StatusCode((int)response.StatusCode, response);

        }



        [HttpDelete("{id}")]

        [RequirePermission(Permissions.PartInventoryDelete)]

        public async Task<IActionResult> Delete(int id)

        {

            var response = await _service.DeleteAsync(id);

            return StatusCode((int)response.StatusCode, response);

        }

    }

}


