using DineFlow.BusinessObjects.Common;
using DineFlow.Api.Security;
using DineFlow.BusinessObjects.Menu;
using DineFlow.Services.Menu;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/categories")]
[RoleAuthorize(UserRole.Admin)]
public class AdminCategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public AdminCategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_categoryService.GetAll());
    }

    [HttpPost]
    public IActionResult Create([FromBody] Category category)
    {
        try
        {
            return Ok(_categoryService.Create(category, UserRole.Admin));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Category category)
    {
        try
        {
            category.CategoryId = id;
            _categoryService.Update(category, UserRole.Admin);
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
            _categoryService.SoftDelete(id, UserRole.Admin);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
