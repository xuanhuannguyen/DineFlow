using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Menu;

public class MenuItemAddonGroup : BaseEntity
{
    public int MenuItemAddonGroupId { get; set; }
    public int MenuItemId { get; set; }
    public int MenuAddonGroupId { get; set; }
    public bool IsRequired { get; set; }
    public int MinSelect { get; set; }
    public int MaxSelect { get; set; } = 1;
    public bool AllowDuplicateOption { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public MenuItem? MenuItem { get; set; }
    public MenuAddonGroup? MenuAddonGroup { get; set; }
}
