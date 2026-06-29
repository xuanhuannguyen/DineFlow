using DineFlow.Services.Menu;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Customer;

[ApiController]
[Route("api/customer/menu")]
public class CustomerMenuController : ControllerBase
{
    private readonly IChannelPricingService _pricingService;

    public CustomerMenuController(IChannelPricingService pricingService)
    {
        _pricingService = pricingService;
    }

    [HttpGet]
    public IActionResult GetMenu([FromQuery] string channel = "CUSTOMER_WEB")
    {
        try
        {
            return Ok(_pricingService.GetMenu(channel));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{menuItemId:int}")]
    public IActionResult GetMenuItemDetail(int menuItemId, [FromQuery] string channel = "CUSTOMER_WEB")
    {
        try
        {
            return Ok(_pricingService.GetItemDetail(menuItemId, channel));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
