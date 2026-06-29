namespace DineFlow.BusinessObjects.Menu;

public class OrderAddonRequestDto
{
    public int AddonGroupOptionId { get; set; }
    public int MenuAddonGroupId { get; set; }
    public int MenuAddonOptionId { get; set; }
    public int AddonMenuItemId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}
