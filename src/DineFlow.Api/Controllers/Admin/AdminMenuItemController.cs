using DineFlow.BusinessObjects.Common;
using DineFlow.Api.Security;
using DineFlow.BusinessObjects.Menu;
using DineFlow.Services.Menu;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/menu-items")]
[RoleAuthorize(UserRole.Admin)]
public class AdminMenuItemController : ControllerBase
{
    private readonly IMenuItemService _menuItemService;

    public AdminMenuItemController(IMenuItemService menuItemService)
    {
        _menuItemService = menuItemService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_menuItemService.GetAll());
    }

    [HttpPost]
    public IActionResult Create([FromBody] MenuItem item)
    {
        try
        {
            return Ok(_menuItemService.Create(item, UserRole.Admin));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] MenuItem item)
    {
        try
        {
            var existingItem = _menuItemService.GetById(id);
            if (existingItem is null)
            {
                return NotFound(new { message = "Menu item does not exist." });
            }

            item.MenuItemId = id;
            if (item.RowVersion.Length == 0)
            {
                item.RowVersion = existingItem.RowVersion;
            }

            _menuItemService.Update(item, UserRole.Admin);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/hide")]
    public IActionResult Hide(int id)
    {
        try
        {
            _menuItemService.SoftDelete(id, UserRole.Admin);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
