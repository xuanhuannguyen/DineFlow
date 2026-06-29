using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Services.Menu.Validation;

internal static class MenuAddonValidator
{
    public static void ValidateGroupForSave(MenuAddonGroup group)
    {
        group.GroupName = group.GroupName.Trim();
        if (string.IsNullOrWhiteSpace(group.GroupName))
        {
            throw new BusinessException("Ten nhom modifier khong duoc de trong.");
        }

        if (group.DisplayOrder < 0)
        {
            throw new BusinessException("DisplayOrder khong duoc am.");
        }
    }

    public static void ValidateOptionForSave(MenuAddonOption option)
    {
        option.OptionName = option.OptionName.Trim();
        if (string.IsNullOrWhiteSpace(option.OptionName))
        {
            throw new BusinessException("Ten lua chon modifier khong duoc de trong.");
        }
    }

    public static void ValidateGroupOptionForSave(AddonGroupOption mapping)
    {
        if (mapping.MenuAddonGroupId <= 0 || mapping.MenuAddonOptionId <= 0)
        {
            throw new BusinessException("Nhom va lua chon modifier phai hop le.");
        }

        if (mapping.ExtraPrice < 0 || mapping.DisplayOrder < 0)
        {
            throw new BusinessException("ExtraPrice va DisplayOrder khong duoc am.");
        }

        if (mapping.MaxQuantityPerOption is <= 0)
        {
            throw new BusinessException("MaxQuantityPerOption phai lon hon 0 neu duoc cau hinh.");
        }

        if (!mapping.AllowMultiple)
        {
            mapping.MaxQuantityPerOption = null;
        }
    }

    public static void ValidateMenuItemGroupRules(MenuItemAddonGroup mapping)
    {
        if (mapping.DisplayOrder < 0 || mapping.MinSelect < 0 || mapping.MaxSelect < 0)
        {
            throw new BusinessException("MinSelect, MaxSelect va DisplayOrder khong duoc am.");
        }

        if (mapping.MaxSelect < 1)
        {
            throw new BusinessException("MaxSelect phai >= 1 khi gan nhom modifier cho mon.");
        }

        if (mapping.IsRequired && mapping.MinSelect < 1)
        {
            throw new BusinessException("Required group phai co MinSelect >= 1.");
        }

        if (mapping.MinSelect > mapping.MaxSelect)
        {
            throw new BusinessException("MaxSelect phai >= MinSelect.");
        }
    }
}
