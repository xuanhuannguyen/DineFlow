using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Repositories.Menu;

public interface IMenuAddonRepository
{
    List<MenuAddonGroup> GetAllGroups();
    List<MenuAddonOption> GetAllOptions();
    List<MenuAddonGroup> GetGroupsByParentMenuItemId(int parentMenuItemId);
    List<MenuAddonGroup> GetActiveGroupsByParentMenuItemIds(IEnumerable<int> parentMenuItemIds);
    List<MenuItemAddonGroup> GetGroupMappingsByMenuItemId(int menuItemId);
    MenuAddonGroup? GetGroupById(int menuAddonGroupId);
    MenuAddonGroup AddGroup(MenuAddonGroup group);
    void UpdateGroup(MenuAddonGroup group);
    MenuAddonOption AddOption(MenuAddonOption option);
    MenuAddonOption? GetOptionById(int menuAddonOptionId);
    void UpdateOption(MenuAddonOption option);
    MenuItemAddonGroup AssignGroupToMenuItem(MenuItemAddonGroup mapping);
    MenuItemAddonGroup? GetMenuItemAddonGroup(int menuItemId, int menuAddonGroupId);
    void UpdateMenuItemAddonGroup(MenuItemAddonGroup mapping);
    AddonGroupOption AddOptionToGroup(AddonGroupOption mapping);
    AddonGroupOption? GetAddonGroupOptionById(int addonGroupOptionId);
    AddonGroupOption? GetAddonGroupOption(int menuAddonGroupId, int menuAddonOptionId);
    void UpdateAddonGroupOption(AddonGroupOption mapping);
    int CountDefaultOptions(int menuAddonGroupId, int? excludeAddonGroupOptionId = null);
}
