using DineFlow.BusinessObjects.Menu;
using DineFlow.DataAccessObjects.DbContexts;
using DineFlow.DataAccessObjects.Menu;

namespace DineFlow.Repositories.Menu;

public class MenuAddonRepository : IMenuAddonRepository
{
    private readonly MenuAddonDAO _menuAddonDAO;

    public MenuAddonRepository() : this(new MenuAddonDAO())
    {
    }

    public MenuAddonRepository(AppDbContext dbContext) : this(new MenuAddonDAO(dbContext))
    {
    }

    private MenuAddonRepository(MenuAddonDAO menuAddonDAO)
    {
        _menuAddonDAO = menuAddonDAO;
    }

    public List<MenuAddonGroup> GetAllGroups() => _menuAddonDAO.GetAllGroups();
    public List<MenuAddonOption> GetAllOptions() => _menuAddonDAO.GetAllOptions();
    public List<MenuAddonGroup> GetGroupsByParentMenuItemId(int parentMenuItemId) => _menuAddonDAO.GetGroupsByParentMenuItemId(parentMenuItemId);
    public List<MenuAddonGroup> GetActiveGroupsByParentMenuItemIds(IEnumerable<int> parentMenuItemIds) => _menuAddonDAO.GetActiveGroupsByParentMenuItemIds(parentMenuItemIds);
    public List<MenuItemAddonGroup> GetGroupMappingsByMenuItemId(int menuItemId) => _menuAddonDAO.GetGroupMappingsByMenuItemId(menuItemId);
    public MenuAddonGroup? GetGroupById(int menuAddonGroupId) => _menuAddonDAO.GetGroupById(menuAddonGroupId);
    public MenuAddonGroup AddGroup(MenuAddonGroup group) => _menuAddonDAO.AddGroup(group);
    public void UpdateGroup(MenuAddonGroup group) => _menuAddonDAO.UpdateGroup(group);
    public MenuAddonOption AddOption(MenuAddonOption option) => _menuAddonDAO.AddOption(option);
    public MenuAddonOption? GetOptionById(int menuAddonOptionId) => _menuAddonDAO.GetOptionById(menuAddonOptionId);
    public void UpdateOption(MenuAddonOption option) => _menuAddonDAO.UpdateOption(option);
    public MenuItemAddonGroup AssignGroupToMenuItem(MenuItemAddonGroup mapping) => _menuAddonDAO.AssignGroupToMenuItem(mapping);
    public MenuItemAddonGroup? GetMenuItemAddonGroup(int menuItemId, int menuAddonGroupId) => _menuAddonDAO.GetMenuItemAddonGroup(menuItemId, menuAddonGroupId);
    public void UpdateMenuItemAddonGroup(MenuItemAddonGroup mapping) => _menuAddonDAO.UpdateMenuItemAddonGroup(mapping);
    public AddonGroupOption AddOptionToGroup(AddonGroupOption mapping) => _menuAddonDAO.AddOptionToGroup(mapping);
    public AddonGroupOption? GetAddonGroupOptionById(int addonGroupOptionId) => _menuAddonDAO.GetAddonGroupOptionById(addonGroupOptionId);
    public AddonGroupOption? GetAddonGroupOption(int menuAddonGroupId, int menuAddonOptionId) => _menuAddonDAO.GetAddonGroupOption(menuAddonGroupId, menuAddonOptionId);
    public void UpdateAddonGroupOption(AddonGroupOption mapping) => _menuAddonDAO.UpdateAddonGroupOption(mapping);
    public int CountDefaultOptions(int menuAddonGroupId, int? excludeAddonGroupOptionId = null) => _menuAddonDAO.CountDefaultOptions(menuAddonGroupId, excludeAddonGroupOptionId);
}
