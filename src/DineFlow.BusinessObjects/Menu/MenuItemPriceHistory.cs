using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Menu;

public class MenuItemPriceHistory : BaseEntity
{
    public int MenuItemPriceHistoryId { get; set; }
    public int RestaurantId { get; set; } = 1;
    public int MenuItemId { get; set; }
    public int? MenuItemVariantId { get; set; }
    public decimal? OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public PriceChangeType ChangeType { get; set; } = PriceChangeType.ManualUpdate;
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo { get; set; }
    public string? Reason { get; set; }
    public int? ChangedBy { get; set; }

    public MenuItem? MenuItem { get; set; }
    public MenuItemVariant? MenuItemVariant { get; set; }
}
