using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Tables;
using DineFlow.BusinessObjects.Menu;

namespace DineFlow.BusinessObjects.Orders;

public class Order : BaseEntity
{
    public int OrderId { get; set; }
    public int SalesChannelId { get; set; }
    public string? ExternalOrderCode { get; set; }
    public int TableSessionId { get; set; }
    public int? SessionCustomerId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public OrderSource OrderSource { get; set; } = OrderSource.CustomerWeb;
    public string? ClientToken { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Accepted;
    public PrintStatus PrintStatus { get; set; } = PrintStatus.PendingPrint;
    public string? CustomerNote { get; set; }
    public string? SystemNote { get; set; }
    public string? CancelReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? PrintedAt { get; set; }
    public string? PrintError { get; set; }
    public int PrintRetryCount { get; set; }
    public int? CreatedBy { get; set; }
    public int? CancelledBy { get; set; }

    public TableSession? TableSession { get; set; }
    public TableSessionCustomer? SessionCustomer { get; set; }
    public User? CreatedByUser { get; set; }
    public User? CancelledByUser { get; set; }
    public SalesChannel? SalesChannel { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
