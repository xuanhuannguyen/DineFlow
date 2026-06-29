using DineFlow.Api.Security;
using DineFlow.BusinessObjects.Menu;
using DineFlow.Services.Menu;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Internal;

[ApiController]
[InternalApiKey]
[Route("api/internal/menu")]
public class InternalMenuController : ControllerBase
{
    private readonly IMenuItemService _menuItemService;
    private readonly IStockService _stockService;
    private readonly IMenuAddonService _menuAddonService;

    public InternalMenuController(
        IMenuItemService menuItemService,
        IStockService stockService,
        IMenuAddonService menuAddonService)
    {
        _menuItemService = menuItemService;
        _stockService = stockService;
        _menuAddonService = menuAddonService;
    }

    [HttpPost("validate-orderable-items")]
    public async Task<IActionResult> ValidateOrderableItems([FromBody] List<OrderItemRequestDto> items)
    {
        try
        {
            var isValid = await _stockService.ValidateOrderableItemsAsync(items);
            return Ok(new { isValid });
        }
        catch (Exception ex)
        {
            return BadRequest(new { isValid = false, message = ex.Message });
        }
    }

    [HttpPost("validate-addons")]
    public async Task<IActionResult> ValidateAddons([FromBody] List<OrderItemRequestDto> items)
    {
        try
        {
            var isValid = await _menuAddonService.ValidateAddonsForOrderAsync(items);
            return Ok(new { isValid });
        }
        catch (Exception ex)
        {
            return BadRequest(new { isValid = false, message = ex.Message });
        }
    }

    [HttpPost("addon-snapshots")]
    public async Task<IActionResult> GetAddonSnapshots([FromBody] List<OrderItemRequestDto> items)
    {
        try
        {
            return Ok(await _menuAddonService.GetAddonSnapshotsForOrderAsync(items));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("reserve-stock")]
    public async Task<IActionResult> ReserveStock([FromBody] List<OrderItemRequestDto> items)
    {
        try
        {
            await _stockService.ReserveStockForOrderAsync(items);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("rollback-stock")]
    public async Task<IActionResult> RollbackStock([FromBody] List<OrderItemRequestDto> items)
    {
        try
        {
            await _stockService.RollbackStockForCancelledOrderAsync(items);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
