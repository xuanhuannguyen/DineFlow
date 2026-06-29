namespace DineFlow.BusinessObjects.Menu;

public class SalesChannel
{
    public int SalesChannelId { get; set; }
    public string ChannelCode { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<MenuItemChannelPrice> MenuItemPrices { get; set; } = new List<MenuItemChannelPrice>();
    public ICollection<ChoiceItemChannelPrice> ChoiceItemPrices { get; set; } = new List<ChoiceItemChannelPrice>();
}
