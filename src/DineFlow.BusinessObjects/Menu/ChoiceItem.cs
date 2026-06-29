namespace DineFlow.BusinessObjects.Menu;

public class ChoiceItem
{
    public int ChoiceItemId { get; set; }
    public int ChoiceGroupId { get; set; }
    public string ChoiceName { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; }
    public int? LinkedMenuItemId { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int DisplayOrder { get; set; }

    public ChoiceGroup? ChoiceGroup { get; set; }
    public MenuItem? LinkedMenuItem { get; set; }
    public ICollection<ChoiceItemChannelPrice> ChannelPrices { get; set; } = new List<ChoiceItemChannelPrice>();
}
