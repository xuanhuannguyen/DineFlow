namespace DineFlow.BusinessObjects.Menu;

public class OrderItemRequestDto
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public List<OrderAddonRequestDto> Addons { get; set; } = new();
    public List<int> TouchedAddonGroupIds { get; set; } = new();
}
