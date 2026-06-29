namespace DineFlow.BusinessObjects.Menu;

public static class MenuBusinessMessages
{
    public const string MenuItemNotFound = "Mon khong ton tai.";
    public const string MenuItemNotOrderableFormat = "Mon '{0}' hien khong the dat.";
    public const string MenuItemNotStandaloneFormat = "Mon '{0}' khong the dat rieng.";
    public const string MenuItemInsufficientStockFormat = "Mon '{0}' khong du so luong.";
    public const string CannotEnableAvailabilityWithoutStock = "Khong the bat ban lai mon khi ton kho bang 0.";
    public const string CannotEnableInactiveMenuItem = "Khong the bat ban mon da bi an.";
    public const string TrackedMenuItemRequiresStock = "Mon co quan ly stock phai co so luong >= 0.";
}
