using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Menu;

public class MenuAddonOption : BaseEntity
{
    public int MenuAddonOptionId { get; set; }
    public string OptionName { get; set; } = string.Empty;
    public int? LinkedMenuItemId { get; set; }
    public string? Description { get; set; }
    public PriceApplyType PriceApplyType { get; set; } = PriceApplyType.PerParentItem;
    public bool IsActive { get; set; } = true;

    public MenuItem? LinkedMenuItem { get; set; }
    public ICollection<AddonGroupOption> Groups { get; set; } = new List<AddonGroupOption>();
}
