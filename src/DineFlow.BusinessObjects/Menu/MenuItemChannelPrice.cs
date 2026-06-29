namespace DineFlow.BusinessObjects.Menu;

public class MenuItemChannelPrice
{
    public int MenuItemId { get; set; }
    public int SalesChannelId { get; set; }
    public decimal ChannelExtraPrice { get; set; }

    public MenuItem? MenuItem { get; set; }
    public SalesChannel? SalesChannel { get; set; }
}
