using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Menu;

public class StockTransaction : BaseEntity
{
    public int StockTransactionId { get; set; }
    public int RestaurantId { get; set; } = 1;
    public int MenuItemId { get; set; }
    public int? MenuItemVariantId { get; set; }
    public StockChangeType ChangeType { get; set; }
    public int QuantityChange { get; set; }
    public int BeforeQuantity { get; set; }
    public int AfterQuantity { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public int? CreatedBy { get; set; }

    public MenuItem? MenuItem { get; set; }
    public MenuItemVariant? MenuItemVariant { get; set; }
}
