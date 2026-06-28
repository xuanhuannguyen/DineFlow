using DineFlow.Services.Tables;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Internal
{
    public class UpdateTableStatusRequest
    {
        public string Status { get; set; } = null!;
        public int TableSessionId { get; set; }
        public string? Reason { get; set; }
    }

    [ApiController]
    [Route("api/internal/tables")]
    // [Authorize] // Internal endpoint, nên có auth hoặc key riêng
    public class InternalTableController : ControllerBase
    {
        private readonly ITableService _tableService;
        private readonly ITableStatusPort _tableStatusPort;

        public InternalTableController(ITableService tableService, ITableStatusPort tableStatusPort)
        {
            _tableService = tableService;
            _tableStatusPort = tableStatusPort;
        }

        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] UpdateTableStatusRequest request)
        {
            try
            {
                // Gọi thẳng ITableService để kiểm tra / update status nếu là luồng chung
                // Hoặc dùng TableStatusPort (đã được bọc logic riêng)
                _tableService.UpdateTableStatus(id, request.Status, request.Reason ?? string.Empty);
                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("TABLE_NOT_FOUND"))
                    return NotFound(new { message = ex.Message });
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
