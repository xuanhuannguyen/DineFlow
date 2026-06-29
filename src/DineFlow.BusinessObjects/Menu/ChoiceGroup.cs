namespace DineFlow.BusinessObjects.Menu;

public class ChoiceGroup
{
    public int ChoiceGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int DefaultMinSelect { get; set; }
    public int DefaultMaxSelect { get; set; } = 1;
    public bool IsAvailable { get; set; } = true;

    public ICollection<ChoiceItem> ChoiceItems { get; set; } = new List<ChoiceItem>();
    public ICollection<MenuItemChoiceGroup> MenuItems { get; set; } = new List<MenuItemChoiceGroup>();
}
