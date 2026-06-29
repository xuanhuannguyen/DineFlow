namespace DineFlow.BusinessObjects.Common;

public enum UserRole
{
    Admin = 1,
    Staff = 2
}

public enum DiningTableStatus
{
    Available = 1,
    Occupied = 2,
    WaitingPayment = 3
}

public enum TableSessionStatus
{
    Open = 1,
    WaitingPayment = 2,
    Closed = 3,
    Cancelled = 4
}

public enum OrderSource
{
    CustomerWeb = 1,
    StaffApp = 2
}

public enum OrderStatus
{
    Accepted = 1,
    Cancelled = 2
}

public enum PrintStatus
{
    PendingPrint = 1,
    Printed = 2,
    PrintFailed = 3
}

public enum ServiceRequestType
{
    CallStaff = 1,
    PaymentRequest = 2
}

public enum ServiceRequestStatus
{
    Pending = 1,
    Confirmed = 2,
    Completed = 3
}

public enum BillStatus
{
    Unpaid = 1,
    Paid = 2,
    Cancelled = 3
}

public enum PaymentMethod
{
    Cash = 1,
    BankTransfer = 2,
    Card = 3,
    EWallet = 4
}

public enum MenuItemType
{
    Single = 1,
    Combo = 2,
    AddonOnly = 3,
    Drink = 4,
    SideDish = 5
}

public enum MenuItemStatus
{
    Draft = 1,
    Active = 2,
    Inactive = 3,
    Deleted = 4
}

public enum VisibilityStatus
{
    Visible = 1,
    Hidden = 2
}

public enum AvailabilityStatus
{
    Available = 1,
    SoldOut = 2,
    TemporarilyUnavailable = 3,
    OutOfServiceTime = 4
}

public enum StockChangeType
{
    Import = 1,
    OrderDeduct = 2,
    CancelReturn = 3,
    Adjustment = 4,
    Waste = 5
}

public enum PriceChangeType
{
    Initial = 1,
    ManualUpdate = 2,
    Promotion = 3
}

public enum PriceApplyType
{
    PerParentItem = 1,
    PerSelection = 2
}

public enum AuditActionType
{
    Create = 1,
    Update = 2,
    Delete = 3,
    Hide = 4,
    Show = 5,
    EnableSale = 6,
    DisableSale = 7,
    ChangePrice = 8,
    StockImport = 9,
    StockAdjust = 10
}
