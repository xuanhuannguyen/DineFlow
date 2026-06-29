using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Services.Menu;

public interface IMenuItemService
{
    List<MenuItem> GetAll();
    List<MenuItem> GetCustomerMenuItems();
    MenuItem? GetById(int menuItemId);
    List<MenuAddonGroup> GetAddonGroups(int parentMenuItemId);
    List<MenuItemAddonGroup> GetAddonGroupMappings(int parentMenuItemId);
    List<MenuItem> Search(string keyword);
    bool ValidateOrderableItems(List<OrderItemRequestDto> items);
    Task<bool> ValidateOrderableItemsAsync(List<OrderItemRequestDto> items);
    bool ValidateAddonsForOrder(List<OrderItemRequestDto> items);
    Task<bool> ValidateAddonsForOrderAsync(List<OrderItemRequestDto> items);
    List<AddonSnapshotDto> GetAddonSnapshotsForOrder(List<OrderItemRequestDto> items);
    Task<List<AddonSnapshotDto>> GetAddonSnapshotsForOrderAsync(List<OrderItemRequestDto> items);
    void ReserveStockForOrder(List<OrderItemRequestDto> items);
    Task ReserveStockForOrderAsync(List<OrderItemRequestDto> items);
    void RollbackStockForCancelledOrder(List<OrderItemRequestDto> items);
    Task RollbackStockForCancelledOrderAsync(List<OrderItemRequestDto> items);
    MenuItemSnapshotDto GetMenuItemSnapshot(int menuItemId);
    Task<MenuItemSnapshotDto> GetMenuItemSnapshotAsync(int menuItemId);
    MenuItem Create(MenuItem item);
    MenuItem Create(MenuItem item, UserRole role);
    void Update(MenuItem item);
    void Update(MenuItem item, UserRole role);
    void SoftDelete(int menuItemId, UserRole role);
    void UpdateStock(int menuItemId, int? availableQuantity, UserRole role);
    void UpdateStock(int menuItemId, int? availableQuantity, string? staffNote, UserRole role);
    void SetAvailability(int menuItemId, bool isAvailable, UserRole role);
    void SetAvailability(int menuItemId, bool isAvailable, string? soldOutReason, string? staffNote, UserRole role);
}
