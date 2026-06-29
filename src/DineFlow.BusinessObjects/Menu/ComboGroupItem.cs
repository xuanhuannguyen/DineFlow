using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Menu;

public class ComboGroupItem : BaseEntity
{
    public int ComboGroupItemId { get; set; }
    public int ComboGroupId { get; set; }
    public int MenuItemId { get; set; }
    public int? MenuItemVariantId { get; set; }
    public decimal ExtraPrice { get; set; }
    public bool IsDefault { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int DisplayOrder { get; set; }

    public ComboGroup? ComboGroup { get; set; }
    public MenuItem? MenuItem { get; set; }
    public MenuItemVariant? MenuItemVariant { get; set; }
}
