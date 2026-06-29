using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Menu;

public class MenuItemImage : BaseEntity
{
    public int MenuItemImageId { get; set; }
    public int MenuItemId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public MenuItem? MenuItem { get; set; }
}
