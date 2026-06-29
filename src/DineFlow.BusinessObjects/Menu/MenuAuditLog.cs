using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Menu;

public class MenuAuditLog : BaseEntity
{
    public int MenuAuditLogId { get; set; }
    public int RestaurantId { get; set; } = 1;
    public string EntityName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int? MenuItemId { get; set; }
    public AuditActionType ActionType { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Reason { get; set; }
    public int? CreatedBy { get; set; }

    public MenuItem? MenuItem { get; set; }
}
