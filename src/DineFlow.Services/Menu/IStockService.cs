using DineFlow.BusinessObjects.Menu;
using DineFlow.BusinessObjects.Common;

namespace DineFlow.Services.Menu;

public interface IStockService
{
    void UpdateStock(int menuItemId, int? availableQuantity, UserRole role);
    void UpdateStock(int menuItemId, int? availableQuantity, string? staffNote, UserRole role);
    Task UpdateStockAsync(int menuItemId, int? availableQuantity, UserRole role);
    Task UpdateStockAsync(int menuItemId, int? availableQuantity, string? staffNote, UserRole role);
    StockStatusDto GetStockStatus(int menuItemId);
    Task<StockStatusDto> GetStockStatusAsync(int menuItemId);
    bool ValidateStockForOrder(List<OrderItemRequestDto> items);
    Task<bool> ValidateStockForOrderAsync(List<OrderItemRequestDto> items);
    bool ValidateOrderableItems(List<OrderItemRequestDto> items);
    Task<bool> ValidateOrderableItemsAsync(List<OrderItemRequestDto> items);
    void ReserveStockForOrder(List<OrderItemRequestDto> items);
    Task ReserveStockForOrderAsync(List<OrderItemRequestDto> items);
    void RollbackStockForCancelledOrder(List<OrderItemRequestDto> items);
    Task RollbackStockForCancelledOrderAsync(List<OrderItemRequestDto> items);
}
