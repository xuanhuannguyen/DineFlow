using DineFlow.BusinessObjects.Tables;
using DineFlow.Services.Tables;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/tables")]
    // [Authorize(Roles = AppEnums.UserRole.Admin)] // Bỏ comment khi tích hợp Auth
    public class AdminTableController : ControllerBase
    {
        private readonly ITableService _tableService;
        private readonly ITableQrService _qrService;

        public AdminTableController(ITableService tableService, ITableQrService qrService)
        {
            _tableService = tableService;
            _qrService = qrService;
        }

        [HttpGet]
        public IActionResult GetAllTables([FromQuery] string? keyword, [FromQuery] int? areaId,
                                          [FromQuery] string? status, [FromQuery] bool? isActive)
        {
            try
            {
                var tables = _tableService.GetAllTables(keyword, areaId, status, isActive);
                return Ok(tables);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetTableById(int id)
        {
            try
            {
                var table = _tableService.GetTableById(id);
                return Ok(table);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("TABLE_NOT_FOUND"))
                    return NotFound(new { message = ex.Message });
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CreateTable([FromBody] CreateTableRequest request)
        {
            try
            {
                var table = _tableService.CreateTable(request);
                return CreatedAtAction(nameof(GetTableById), new { id = table.TableId }, table);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTable(int id, [FromBody] UpdateTableRequest request)
        {
            try
            {
                var table = _tableService.UpdateTable(id, request);
                return Ok(table);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("TABLE_NOT_FOUND"))
                    return NotFound(new { message = ex.Message });
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/deactivate")]
        public IActionResult DeactivateTable(int id)
        {
            try
            {
                _tableService.DeactivateTable(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("TABLE_NOT_FOUND"))
                    return NotFound(new { message = ex.Message });
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/reactivate")]
        public IActionResult ReactivateTable(int id)
        {
            try
            {
                _tableService.ReactivateTable(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("TABLE_NOT_FOUND"))
                    return NotFound(new { message = ex.Message });
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/reset-qr")]
        public IActionResult ResetQrToken(int id)
        {
            try
            {
                var qrDto = _qrService.ResetQrToken(id);
                return Ok(qrDto);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("TABLE_NOT_FOUND"))
                    return NotFound(new { message = ex.Message });
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/qr")]
        public IActionResult GetQrByTableId(int id)
        {
            try
            {
                var qrDto = _qrService.GetQrByTableId(id);
                return Ok(qrDto);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("TABLE_NOT_FOUND"))
                    return NotFound(new { message = ex.Message });
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
