namespace DineFlow.BusinessObjects.Menu;

public class AddonSnapshotDto
{
    public int ParentMenuItemId { get; set; }
    public int AddonGroupOptionId { get; set; }
    public int MenuAddonGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int MenuAddonOptionId { get; set; }
    public int AddonMenuItemId { get; set; }
    public string OptionName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
