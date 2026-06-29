namespace DineFlow.BusinessObjects.Menu;

public class MenuItemSnapshotDto
{
    public int MenuItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int? AvailableQuantity { get; set; }
    public bool TrackStock { get; set; }
    public bool IsActive { get; set; }
    public bool IsAvailable { get; set; }
}
