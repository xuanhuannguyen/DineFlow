using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Menu;

public class KitchenStation : BaseEntity
{
    public int KitchenStationId { get; set; }
    public int RestaurantId { get; set; } = 1;
    public string StationName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
