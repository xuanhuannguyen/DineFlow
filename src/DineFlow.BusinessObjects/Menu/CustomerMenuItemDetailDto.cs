namespace DineFlow.BusinessObjects.Menu;

public class CustomerMenuItemDetailDto
{
    public MenuItem Item { get; set; } = new();
    public List<MenuItemAddonGroup> AddonGroups { get; set; } = new();
}
