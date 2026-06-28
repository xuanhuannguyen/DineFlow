using DineFlow.Services.Tables;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/tables")]
    public class CustomerTableController : ControllerBase
    {
        private readonly ITableQrService _qrService;

        public CustomerTableController(ITableQrService qrService)
        {
            _qrService = qrService;
        }

        /// <summary>
        /// GET /api/customer/tables/by-token/{token}
        /// Customer Web gọi sau khi quét QR để xác thực và lấy thông tin bàn.
        /// Không yêu cầu đăng nhập. Không tạo session tại đây.
        /// </summary>
        [HttpGet("by-token/{token}")]
        public IActionResult GetByToken(string token)
        {
            try
            {
                var result = _qrService.ValidateQrToken(token);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
