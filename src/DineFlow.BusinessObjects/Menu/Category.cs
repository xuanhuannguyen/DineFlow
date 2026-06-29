using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Menu;

public class Category : BaseEntity
{
    public int CategoryId { get; set; }
    public int RestaurantId { get; set; } = 1;
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
