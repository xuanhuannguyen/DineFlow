using DineFlow.Api.Security;
using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;
using DineFlow.Services.Menu;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/choices")]
[RoleAuthorize(UserRole.Admin)]
public class AdminChoiceController : ControllerBase
{
    private readonly IChoiceService _choices;

    public AdminChoiceController(IChoiceService choices)
    {
        _choices = choices;
    }

    [HttpGet("groups")]
    public IActionResult GetGroups() => Ok(_choices.GetGroups());

    [HttpGet("menu-items/{menuItemId:int}/groups")]
    public IActionResult GetMappings(int menuItemId) => Ok(_choices.GetMappings(menuItemId));

    [HttpPost("groups")]
    public IActionResult CreateGroup([FromBody] ChoiceGroup group) =>
        Execute(() => _choices.CreateGroup(group, UserRole.Admin));

    [HttpPut("groups/{id:int}")]
    public IActionResult UpdateGroup(int id, [FromBody] ChoiceGroup group) =>
        ExecuteNoContent(() =>
        {
            group.ChoiceGroupId = id;
            _choices.UpdateGroup(group, UserRole.Admin);
        });

    [HttpPost("items")]
    public IActionResult CreateChoiceItem([FromBody] ChoiceItem item) =>
        Execute(() => _choices.CreateChoiceItem(item, UserRole.Admin));

    [HttpPut("items/{id:int}")]
    public IActionResult UpdateChoiceItem(int id, [FromBody] ChoiceItem item) =>
        ExecuteNoContent(() =>
        {
            item.ChoiceItemId = id;
            _choices.UpdateChoiceItem(item, UserRole.Admin);
        });

    [HttpDelete("items/{id:int}")]
    public IActionResult DeleteChoiceItem(int id) =>
        ExecuteNoContent(() => _choices.DeleteChoiceItem(id, UserRole.Admin));

    [HttpPut("menu-items/{menuItemId:int}/groups/{choiceGroupId:int}")]
    public IActionResult AssignGroup(
        int menuItemId,
        int choiceGroupId,
        [FromBody] MenuItemChoiceGroup mapping) =>
        Execute(() =>
        {
            mapping.MenuItemId = menuItemId;
            mapping.ChoiceGroupId = choiceGroupId;
            return _choices.AssignGroup(mapping, UserRole.Admin);
        });

    [HttpDelete("menu-items/{menuItemId:int}/groups/{choiceGroupId:int}")]
    public IActionResult RemoveGroup(int menuItemId, int choiceGroupId) =>
        ExecuteNoContent(() => _choices.RemoveGroup(menuItemId, choiceGroupId, UserRole.Admin));

    private IActionResult Execute<T>(Func<T> action)
    {
        try { return Ok(action()); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    private IActionResult ExecuteNoContent(Action action)
    {
        try { action(); return NoContent(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}
