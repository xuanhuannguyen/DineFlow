namespace DineFlow.BusinessObjects.Menu;

public class StockStatusDto
{
    public int MenuItemId { get; set; }
    public bool TrackStock { get; set; }
    public int? AvailableQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsActive { get; set; }
    public string? SoldOutReason { get; set; }
    public string? StaffNote { get; set; }
}
