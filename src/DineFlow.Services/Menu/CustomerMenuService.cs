using DineFlow.BusinessObjects.Menu;
using DineFlow.BusinessObjects.Common;

namespace DineFlow.Services.Menu;

public class CustomerMenuService : ICustomerMenuService
{
    private readonly ICategoryService _categoryService;
    private readonly IMenuItemService _menuItemService;

    public CustomerMenuService() : this(new CategoryService(), new MenuItemService())
    {
    }

    public CustomerMenuService(ICategoryService categoryService, IMenuItemService menuItemService)
    {
        _categoryService = categoryService;
        _menuItemService = menuItemService;
    }

    public CustomerMenuDto GetCustomerMenu()
    {
        return new CustomerMenuDto
        {
            Categories = _categoryService.GetActiveCategories(),
            Items = _menuItemService.GetCustomerMenuItems()
        };
    }

    public Task<CustomerMenuDto> GetCustomerMenuAsync()
    {
        return Task.FromResult(GetCustomerMenu());
    }

    public CustomerMenuItemDetailDto GetMenuItemDetail(int menuItemId)
    {
        var item = _menuItemService.GetById(menuItemId)
            ?? throw new BusinessException("Mon khong ton tai.");

        if (!item.CanShowToCustomer())
        {
            throw new BusinessException("Mon khong hien thi tren menu khach.");
        }

        var addonGroups = _menuItemService.GetAddonGroupMappings(menuItemId)
            .Where(x => x.IsActive)
            .Select(x =>
            {
                if (x.MenuAddonGroup is null)
                {
                    return x;
                }

                x.MenuAddonGroup.Options = x.MenuAddonGroup.Options
                    .Where(o => o.IsActive
                    && o.MenuAddonOption is not null
                    && o.MenuAddonOption.IsActive
                    && (o.MenuAddonOption.LinkedMenuItem is null
                        || (o.MenuAddonOption.LinkedMenuItem.Status == MenuItemStatus.Active
                            && o.MenuAddonOption.LinkedMenuItem.VisibilityStatus == VisibilityStatus.Visible
                            && o.MenuAddonOption.LinkedMenuItem.IsActive)))
                    .OrderBy(o => o.DisplayOrder)
                    .ToList();
                return x;
            })
            .OrderBy(x => x.DisplayOrder)
            .ToList();

        return new CustomerMenuItemDetailDto
        {
            Item = item,
            AddonGroups = addonGroups
        };
    }

    public Task<CustomerMenuItemDetailDto> GetMenuItemDetailAsync(int menuItemId)
    {
        return Task.FromResult(GetMenuItemDetail(menuItemId));
    }

    public List<MenuItem> GetAvailableMenuItems()
    {
        return _menuItemService.GetCustomerMenuItems()
            .Where(x => x.IsAvailable)
            .ToList();
    }

    public Task<List<MenuItem>> GetAvailableMenuItemsAsync()
    {
        return Task.FromResult(GetAvailableMenuItems());
    }

    public MenuItemSnapshotDto GetMenuItemSnapshot(int menuItemId)
    {
        return _menuItemService.GetMenuItemSnapshot(menuItemId);
    }

    public Task<MenuItemSnapshotDto> GetMenuItemSnapshotAsync(int menuItemId)
    {
        return _menuItemService.GetMenuItemSnapshotAsync(menuItemId);
    }
}
