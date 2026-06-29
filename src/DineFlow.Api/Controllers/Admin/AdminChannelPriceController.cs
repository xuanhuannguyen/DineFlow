using DineFlow.Api.Security;
using DineFlow.BusinessObjects.Common;
using DineFlow.Services.Menu;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/channel-prices")]
[RoleAuthorize(UserRole.Admin)]
public class AdminChannelPriceController : ControllerBase
{
    private readonly IChannelPricingService _pricing;

    public AdminChannelPriceController(IChannelPricingService pricing)
    {
        _pricing = pricing;
    }

    [HttpPut("menu-items/{menuItemId:int}/channels/{salesChannelId:int}")]
    public IActionResult SetMenuItemPrice(int menuItemId, int salesChannelId, [FromBody] ChannelPriceRequest request) =>
        Execute(() => _pricing.SetMenuItemExtraPrice(menuItemId, salesChannelId, request.ChannelExtraPrice, UserRole.Admin));

    [HttpPut("choice-items/{choiceItemId:int}/channels/{salesChannelId:int}")]
    public IActionResult SetChoiceItemPrice(int choiceItemId, int salesChannelId, [FromBody] ChannelPriceRequest request) =>
        Execute(() => _pricing.SetChoiceItemExtraPrice(choiceItemId, salesChannelId, request.ChannelExtraPrice, UserRole.Admin));

    private IActionResult Execute(Action action)
    {
        try { action(); return NoContent(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    public sealed record ChannelPriceRequest(decimal ChannelExtraPrice);
}
