using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Menu;

public class MenuAddonGroup : BaseEntity
{
    public int MenuAddonGroupId { get; set; }
    public int RestaurantId { get; set; } = 1;
    public string GroupName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<MenuItemAddonGroup> MenuItems { get; set; } = new List<MenuItemAddonGroup>();
    public ICollection<AddonGroupOption> Options { get; set; } = new List<AddonGroupOption>();
}
