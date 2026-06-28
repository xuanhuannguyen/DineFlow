using DineFlow.Services.Tables;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff
{
    [ApiController]
    [Route("api/staff")]
    // [Authorize] // Bỏ comment khi có Auth, áp dụng cho cả Staff và Admin
    public class StaffTableController : ControllerBase
    {
        private readonly ITableReadService _tableReadService;
        private readonly IAreaService _areaService;

        public StaffTableController(ITableReadService tableReadService, IAreaService areaService)
        {
            _tableReadService = tableReadService;
            _areaService = areaService;
        }

        [HttpGet("tables/status")]
        public IActionResult GetTableStatus([FromQuery] int? areaId, [FromQuery] string? status, [FromQuery] string? keyword)
        {
            try
            {
                var tables = _tableReadService.GetTableStatusOverview(areaId, status, keyword);
                return Ok(tables);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("areas")]
        public IActionResult GetActiveAreas()
        {
            try
            {
                var areas = _areaService.GetActiveAreas();
                return Ok(areas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
