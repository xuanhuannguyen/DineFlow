using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Menu;

public class AddonGroupOption : BaseEntity
{
    public int AddonGroupOptionId { get; set; }
    public int MenuAddonGroupId { get; set; }
    public int MenuAddonOptionId { get; set; }
    public decimal? ExtraPrice { get; set; }
    public bool IsDefault { get; set; }
    public bool AllowMultiple { get; set; }
    public int? MaxQuantityPerOption { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public MenuAddonGroup? MenuAddonGroup { get; set; }
    public MenuAddonOption? MenuAddonOption { get; set; }
}
