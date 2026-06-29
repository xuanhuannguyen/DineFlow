using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Services.Menu;

public interface ICustomerMenuService
{
    CustomerMenuDto GetCustomerMenu();
    Task<CustomerMenuDto> GetCustomerMenuAsync();
    CustomerMenuItemDetailDto GetMenuItemDetail(int menuItemId);
    Task<CustomerMenuItemDetailDto> GetMenuItemDetailAsync(int menuItemId);
    List<MenuItem> GetAvailableMenuItems();
    Task<List<MenuItem>> GetAvailableMenuItemsAsync();
    MenuItemSnapshotDto GetMenuItemSnapshot(int menuItemId);
    Task<MenuItemSnapshotDto> GetMenuItemSnapshotAsync(int menuItemId);
}
