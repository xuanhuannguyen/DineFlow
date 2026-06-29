using DineFlow.BusinessObjects.Menu;
using DineFlow.BusinessObjects.Tables;

namespace DineFlow.BusinessObjects.Orders;

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int MenuItemId { get; set; }
    public int? SessionCustomerId { get; set; }
    public string MenuItemNameSnapshot { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal BasePriceSnapshot { get; set; }
    public decimal ChannelExtraPriceSnapshot { get; set; }
    public decimal FinalUnitPriceSnapshot { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Order? Order { get; set; }
    public MenuItem? MenuItem { get; set; }
    public TableSessionCustomer? SessionCustomer { get; set; }
    public ICollection<OrderItemSelectedChoice> SelectedChoices { get; set; } = new List<OrderItemSelectedChoice>();

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string ItemName
    {
        get => MenuItemNameSnapshot;
        set => MenuItemNameSnapshot = value;
    }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal UnitPrice
    {
        get => FinalUnitPriceSnapshot;
        set => FinalUnitPriceSnapshot = value;
    }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public ICollection<OrderItemModifier> Modifiers { get; set; } = new List<OrderItemModifier>();
}
