using DineFlow.BusinessObjects.Menu;
using DineFlow.BusinessObjects.Common;

namespace DineFlow.Services.Menu;

public class StockService : IStockService
{
    private readonly IMenuItemService _menuItemService;

    public StockService() : this(new MenuItemService())
    {
    }

    public StockService(IMenuItemService menuItemService)
    {
        _menuItemService = menuItemService;
    }

    public void UpdateStock(int menuItemId, int? availableQuantity, UserRole role)
    {
        _menuItemService.UpdateStock(menuItemId, availableQuantity, role);
    }

    public void UpdateStock(int menuItemId, int? availableQuantity, string? staffNote, UserRole role)
    {
        _menuItemService.UpdateStock(menuItemId, availableQuantity, staffNote, role);
    }

    public Task UpdateStockAsync(int menuItemId, int? availableQuantity, UserRole role)
    {
        UpdateStock(menuItemId, availableQuantity, role);
        return Task.CompletedTask;
    }

    public Task UpdateStockAsync(int menuItemId, int? availableQuantity, string? staffNote, UserRole role)
    {
        UpdateStock(menuItemId, availableQuantity, staffNote, role);
        return Task.CompletedTask;
    }

    public StockStatusDto GetStockStatus(int menuItemId)
    {
        var item = _menuItemService.GetById(menuItemId)
            ?? throw new BusinessException("Mon khong ton tai.");

        return new StockStatusDto
        {
            MenuItemId = item.MenuItemId,
            TrackStock = item.TrackStock,
            AvailableQuantity = item.AvailableQuantity,
            IsAvailable = item.IsAvailable,
            IsActive = item.IsActive,
            SoldOutReason = item.SoldOutReason,
            StaffNote = item.StaffNote
        };
    }

    public Task<StockStatusDto> GetStockStatusAsync(int menuItemId)
    {
        return Task.FromResult(GetStockStatus(menuItemId));
    }

    public bool ValidateStockForOrder(List<OrderItemRequestDto> items)
    {
        return ValidateOrderableItems(items);
    }

    public Task<bool> ValidateStockForOrderAsync(List<OrderItemRequestDto> items)
    {
        return ValidateOrderableItemsAsync(items);
    }

    public bool ValidateOrderableItems(List<OrderItemRequestDto> items)
    {
        return _menuItemService.ValidateOrderableItems(items);
    }

    public Task<bool> ValidateOrderableItemsAsync(List<OrderItemRequestDto> items)
    {
        return _menuItemService.ValidateOrderableItemsAsync(items);
    }

    public void ReserveStockForOrder(List<OrderItemRequestDto> items)
    {
        _menuItemService.ReserveStockForOrder(items);
    }

    public Task ReserveStockForOrderAsync(List<OrderItemRequestDto> items)
    {
        return _menuItemService.ReserveStockForOrderAsync(items);
    }

    public void RollbackStockForCancelledOrder(List<OrderItemRequestDto> items)
    {
        _menuItemService.RollbackStockForCancelledOrder(items);
    }

    public Task RollbackStockForCancelledOrderAsync(List<OrderItemRequestDto> items)
    {
        return _menuItemService.RollbackStockForCancelledOrderAsync(items);
    }
}
