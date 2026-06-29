using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;
using System.Text.RegularExpressions;

namespace DineFlow.Services.Menu.Validation;

internal static class MenuItemValidator
{
    private static readonly Regex ItemCodePattern = new(
        "^[A-Z0-9][A-Z0-9_-]{0,29}$",
        RegexOptions.CultureInvariant);

    public static void ValidateForSave(MenuItem item)
    {
        item.ItemCode = item.ItemCode.Trim().ToUpperInvariant();
        item.ItemName = item.ItemName.Trim();

        if (string.IsNullOrWhiteSpace(item.ItemCode))
        {
            throw new BusinessException("Mã món không được để trống.");
        }

        if (!ItemCodePattern.IsMatch(item.ItemCode))
        {
            throw new BusinessException("Mã món chỉ gồm chữ A-Z, số, dấu gạch ngang hoặc gạch dưới; tối đa 30 ký tự.");
        }

        if (string.IsNullOrWhiteSpace(item.ItemName))
        {
            throw new BusinessException("Ten mon khong duoc de trong.");
        }

        if (item.ItemName.Length > 150)
        {
            throw new BusinessException("Ten mon khong duoc vuot qua 150 ky tu.");
        }

        if (item.CategoryId <= 0)
        {
            throw new BusinessException("Category khong hop le.");
        }

        if (item.Price < 0)
        {
            throw new BusinessException("Gia mon khong duoc am.");
        }

        if (item.ItemType == MenuItemType.AddonOnly && item.CanOrderStandalone)
        {
            throw new BusinessException("AddonOnly khong duoc goi truc tiep.");
        }

        if (item.LowStockThreshold is < 0)
        {
            throw new BusinessException("Nguong canh bao ton kho khong duoc am.");
        }

        if (item.ReservedQuantity < 0)
        {
            throw new BusinessException("So luong giu tam khong duoc am.");
        }

        if (item.PreparationTimeMinutes is < 0)
        {
            throw new BusinessException("Thoi gian chuan bi khong duoc am.");
        }

        if (item.SpicyLevel is < 0 or > 5)
        {
            throw new BusinessException("Muc cay phai nam trong khoang 0-5.");
        }

        if (item.Calories is < 0)
        {
            throw new BusinessException("Calories khong duoc am.");
        }

        if (item.Description?.Length > 1000)
        {
            throw new BusinessException("Mo ta mon khong duoc vuot qua 1000 ky tu.");
        }

        if (item.TrackStock && item.AvailableQuantity is null or < 0)
        {
            throw new BusinessException(MenuBusinessMessages.TrackedMenuItemRequiresStock);
        }

        if (item.TrackStock && item.IsAvailable && item.AvailableQuantity == 0)
        {
            throw new BusinessException("Khong the bat ban mon co ton kho bang 0.");
        }
    }
}
