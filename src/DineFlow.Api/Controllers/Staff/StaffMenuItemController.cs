using DineFlow.BusinessObjects.Common;
using DineFlow.Api.Security;
using DineFlow.Services.Menu;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/menu-items")]
[RoleAuthorize(UserRole.Staff, UserRole.Admin)]
public class StaffMenuItemController : ControllerBase
{
    private readonly IMenuItemService _menuItemService;
    private readonly IStockService _stockService;

    public StaffMenuItemController(IMenuItemService menuItemService, IStockService stockService)
    {
        _menuItemService = menuItemService;
        _stockService = stockService;
    }

    [HttpPut("{id:int}/stock")]
    public IActionResult UpdateStock(int id, [FromBody] StockUpdateRequest request)
    {
        try
        {
            _stockService.UpdateStock(id, request.AvailableQuantity, request.StaffNote, UserRole.Staff);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}/stock")]
    public IActionResult GetStockStatus(int id)
    {
        try
        {
            return Ok(_stockService.GetStockStatus(id));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/availability")]
    public IActionResult SetAvailability(int id, [FromBody] AvailabilityUpdateRequest request)
    {
        try
        {
            _menuItemService.SetAvailability(id, request.IsAvailable, request.SoldOutReason, request.StaffNote, UserRole.Staff);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/sold-out")]
    public IActionResult MarkSoldOut(int id, [FromBody] AvailabilityUpdateRequest? request = null)
    {
        try
        {
            _menuItemService.SetAvailability(
                id,
                false,
                request?.SoldOutReason,
                request?.StaffNote,
                UserRole.Staff);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/reopen")]
    public IActionResult Reopen(int id)
    {
        try
        {
            _menuItemService.SetAvailability(id, true, UserRole.Staff);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class StockUpdateRequest
{
    public int? AvailableQuantity { get; set; }
    public string? StaffNote { get; set; }
}

public class AvailabilityUpdateRequest
{
    public bool IsAvailable { get; set; }
    public string? SoldOutReason { get; set; }
    public string? StaffNote { get; set; }
}
