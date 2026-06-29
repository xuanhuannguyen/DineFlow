using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Services.Menu.Validation;

internal static class OrderRequestValidator
{
    public static void ValidateForOrdering(List<OrderItemRequestDto> items)
    {
        if (items.Count == 0)
        {
            throw new BusinessException("Danh sach mon dat khong duoc de trong.");
        }

        if (items.Any(x => x.MenuItemId <= 0 || x.Quantity <= 0))
        {
            throw new BusinessException("Mon dat va so luong phai hop le.");
        }

        if (items.SelectMany(x => x.Addons).Any(x =>
            x.Quantity <= 0 ||
            (x.AddonGroupOptionId <= 0 && x.AddonMenuItemId <= 0 && (x.MenuAddonGroupId <= 0 || x.MenuAddonOptionId <= 0))))
        {
            throw new BusinessException("Addon dat kem phai hop le.");
        }
    }
}
