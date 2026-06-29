using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Menu;

public class MenuItemVariant : BaseEntity
{
    public int MenuItemVariantId { get; set; }
    public int MenuItemId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsDefault { get; set; }
    public bool IsAvailable { get; set; } = true;
    public AvailabilityStatus AvailabilityStatus { get; set; } = AvailabilityStatus.Available;
    public bool TrackStock { get; set; }
    public int? AvailableQuantity { get; set; }
    public int? LowStockThreshold { get; set; }
    public int DisplayOrder { get; set; }
    public MenuItemStatus Status { get; set; } = MenuItemStatus.Active;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public MenuItem? MenuItem { get; set; }
}
