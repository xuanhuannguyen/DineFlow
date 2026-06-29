using DineFlow.BusinessObjects.Menu;

namespace DineFlow.BusinessObjects.Orders;

public class OrderItemSelectedChoice
{
    public int OrderItemSelectedChoiceId { get; set; }
    public int OrderItemId { get; set; }
    public int ChoiceGroupId { get; set; }
    public int ChoiceItemId { get; set; }
    public string GroupNameSnapshot { get; set; } = string.Empty;
    public string ChoiceNameSnapshot { get; set; } = string.Empty;
    public decimal ExtraPriceSnapshot { get; set; }
    public decimal ChannelExtraPriceSnapshot { get; set; }
    public decimal FinalExtraPriceSnapshot { get; set; }
    public int Quantity { get; set; } = 1;

    public OrderItem? OrderItem { get; set; }
    public ChoiceGroup? ChoiceGroup { get; set; }
    public ChoiceItem? ChoiceItem { get; set; }
}
