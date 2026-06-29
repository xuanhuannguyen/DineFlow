namespace DineFlow.BusinessObjects.Menu;

public class MenuItemChoiceGroup
{
    public int MenuItemId { get; set; }
    public int ChoiceGroupId { get; set; }
    public bool IsRequired { get; set; }
    public int MinSelect { get; set; }
    public int MaxSelect { get; set; } = 1;
    public int DisplayOrder { get; set; }

    public MenuItem? MenuItem { get; set; }
    public ChoiceGroup? ChoiceGroup { get; set; }
}
