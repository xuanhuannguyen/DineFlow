using DineFlow.BusinessObjects.Tables;
using DineFlow.Services.Tables;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/areas")]
    // [Authorize(Roles = AppEnums.UserRole.Admin)] // Bỏ comment khi tích hợp Auth
    public class AdminAreaController : ControllerBase
    {
        private readonly IAreaService _areaService;

        public AdminAreaController(IAreaService areaService)
        {
            _areaService = areaService;
        }

        [HttpGet]
        public IActionResult GetAllAreas()
        {
            try
            {
                var areas = _areaService.GetAllAreas();
                return Ok(areas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetAreaById(int id)
        {
            try
            {
                var area = _areaService.GetAreaById(id);
                return Ok(area);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("AREA_NOT_FOUND"))
                    return NotFound(new { message = ex.Message });
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CreateArea([FromBody] CreateAreaRequest request)
        {
            try
            {
                var area = _areaService.CreateArea(request);
                return CreatedAtAction(nameof(GetAreaById), new { id = area.AreaId }, area);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateArea(int id, [FromBody] UpdateAreaRequest request)
        {
            try
            {
                var area = _areaService.UpdateArea(id, request);
                return Ok(area);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("AREA_NOT_FOUND"))
                    return NotFound(new { message = ex.Message });
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/deactivate")]
        public IActionResult DeactivateArea(int id)
        {
            try
            {
                _areaService.DeactivateArea(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("AREA_NOT_FOUND"))
                    return NotFound(new { message = ex.Message });
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/reactivate")]
        public IActionResult ReactivateArea(int id)
        {
            try
            {
                _areaService.ReactivateArea(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("AREA_NOT_FOUND"))
                    return NotFound(new { message = ex.Message });
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
