using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Menu;

public class ComboGroup : BaseEntity
{
    public int ComboGroupId { get; set; }
    public int ComboMenuItemId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRequired { get; set; } = true;
    public int MinSelect { get; set; } = 1;
    public int MaxSelect { get; set; } = 1;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public MenuItem? ComboMenuItem { get; set; }
    public ICollection<ComboGroupItem> Items { get; set; } = new List<ComboGroupItem>();
}
