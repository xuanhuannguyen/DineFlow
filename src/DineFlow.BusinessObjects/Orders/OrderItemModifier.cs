using DineFlow.BusinessObjects.Menu;

namespace DineFlow.BusinessObjects.Orders;

public class OrderItemModifier
{
    public int OrderItemModifierId { get; set; }
    public int OrderItemId { get; set; }
    public int MenuAddonGroupId { get; set; }
    public int MenuAddonOptionId { get; set; }
    public int? AddonGroupOptionId { get; set; }
    public int? LinkedMenuItemId { get; set; }
    public string AddonGroupNameSnapshot { get; set; } = string.Empty;
    public string AddonOptionNameSnapshot { get; set; } = string.Empty;
    public decimal ExtraPriceSnapshot { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public OrderItem? OrderItem { get; set; }
    public MenuAddonGroup? MenuAddonGroup { get; set; }
    public MenuAddonOption? MenuAddonOption { get; set; }
    public AddonGroupOption? AddonGroupOption { get; set; }
    public MenuItem? LinkedMenuItem { get; set; }
}
