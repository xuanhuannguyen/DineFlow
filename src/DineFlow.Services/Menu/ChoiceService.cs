using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;
using DineFlow.Repositories.Menu;
using DineFlow.DataAccessObjects.DbContexts;

namespace DineFlow.Services.Menu;

public class ChoiceService : IChoiceService
{
    private readonly IChoiceRepository _choices;
    private readonly IMenuItemRepository _menuItems;

    public ChoiceService() : this(new AppDbContext())
    {
    }

    private ChoiceService(AppDbContext dbContext)
        : this(new ChoiceRepository(dbContext), new MenuItemRepository(dbContext))
    {
    }

    public ChoiceService(IChoiceRepository choices, IMenuItemRepository menuItems)
    {
        _choices = choices;
        _menuItems = menuItems;
    }

    public List<ChoiceGroup> GetGroups() => _choices.GetGroups();

    public List<MenuItemChoiceGroup> GetMappings(int menuItemId) => _choices.GetMappings(menuItemId);

    public ChoiceGroup CreateGroup(ChoiceGroup group, UserRole role)
    {
        EnsureAdmin(role);
        group.GroupName = NormalizeName(group.GroupName, "Ten nhom lua chon");
        if (_choices.GetGroups().Any(x => x.GroupName.Equals(group.GroupName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessException("Ten nhom lua chon da ton tai.");
        }

        group.IsAvailable = true;
        ValidateGroupDefaults(group);
        return _choices.AddGroup(group);
    }

    public void UpdateGroup(ChoiceGroup group, UserRole role)
    {
        EnsureAdmin(role);
        var existing = _choices.GetGroup(group.ChoiceGroupId)
            ?? throw new BusinessException("Nhom lua chon khong ton tai.");
        var normalizedName = NormalizeName(group.GroupName, "Ten nhom lua chon");
        if (_choices.GetGroups().Any(x => x.ChoiceGroupId != group.ChoiceGroupId
            && x.GroupName.Equals(normalizedName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessException("Ten nhom lua chon da ton tai.");
        }
        existing.GroupName = normalizedName;
        ValidateGroupDefaults(group);
        existing.DefaultMinSelect = group.DefaultMinSelect;
        existing.DefaultMaxSelect = group.DefaultMaxSelect;
        existing.IsAvailable = group.IsAvailable;
        _choices.UpdateGroup(existing);
    }

    public ChoiceItem CreateChoiceItem(ChoiceItem item, UserRole role)
    {
        EnsureAdmin(role);
        ValidateChoiceItem(item, null);
        return _choices.AddChoiceItem(item);
    }

    public void UpdateChoiceItem(ChoiceItem item, UserRole role)
    {
        EnsureAdmin(role);
        ValidateChoiceItem(item, item.ChoiceItemId);
        var existing = _choices.GetChoiceItem(item.ChoiceItemId)
            ?? throw new BusinessException("Lua chon khong ton tai.");
        existing.ChoiceGroupId = item.ChoiceGroupId;
        existing.ChoiceName = item.ChoiceName.Trim();
        existing.ExtraPrice = item.ExtraPrice;
        existing.LinkedMenuItemId = item.LinkedMenuItemId;
        existing.IsAvailable = item.IsAvailable;
        existing.DisplayOrder = item.DisplayOrder;
        _choices.UpdateChoiceItem(existing);
    }

    public void DeleteChoiceItem(int choiceItemId, UserRole role)
    {
        EnsureAdmin(role);
        _choices.DeleteChoiceItem(choiceItemId);
    }

    public void DeleteGroup(int choiceGroupId, UserRole role)
    {
        EnsureAdmin(role);
        _choices.DeleteGroup(choiceGroupId);
    }

    public MenuItemChoiceGroup AssignGroup(MenuItemChoiceGroup mapping, UserRole role)
    {
        EnsureAdmin(role);
        if (_menuItems.GetById(mapping.MenuItemId) is null)
        {
            throw new BusinessException("Mon khong ton tai.");
        }
        var group = _choices.GetGroup(mapping.ChoiceGroupId);
        if (group is null)
        {
            throw new BusinessException("Nhom lua chon khong ton tai.");
        }
        if (group.ChoiceItems.Any(x => x.LinkedMenuItemId == mapping.MenuItemId))
        {
            throw new BusinessException("Khong the gan nhom co lua chon lien ket nguoc ve chinh mon dang cau hinh.");
        }
        if (mapping.MinSelect < 0 || mapping.MaxSelect < 1 || mapping.MinSelect > mapping.MaxSelect)
        {
            throw new BusinessException("Min/Max lựa chọn không hợp lệ.");
        }
        if (mapping.IsRequired && mapping.MinSelect < 1)
        {
            throw new BusinessException("Nhóm bắt buộc phải có Min ít nhất là 1.");
        }
        if (mapping.DisplayOrder < 0)
        {
            throw new BusinessException("DisplayOrder khong duoc am.");
        }
        return _choices.UpsertMapping(mapping);
    }

    public void RemoveGroup(int menuItemId, int choiceGroupId, UserRole role)
    {
        EnsureAdmin(role);
        _choices.RemoveMapping(menuItemId, choiceGroupId);
    }

    private void ValidateChoiceItem(ChoiceItem item, int? excludedChoiceItemId)
    {
        item.ChoiceName = NormalizeName(item.ChoiceName, "Ten lua chon");
        var group = _choices.GetGroup(item.ChoiceGroupId);
        if (group is null)
        {
            throw new BusinessException("Nhom lua chon khong ton tai.");
        }
        if (group.ChoiceItems.Any(x => x.ChoiceItemId != excludedChoiceItemId
            && x.ChoiceName.Equals(item.ChoiceName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessException("Ten lua chon da ton tai trong nhom nay.");
        }
        if (item.ExtraPrice < 0)
        {
            throw new BusinessException("ExtraPrice khong duoc am.");
        }
        if (item.DisplayOrder < 0)
        {
            throw new BusinessException("DisplayOrder khong duoc am.");
        }
        if (item.LinkedMenuItemId is int linkedId && _menuItems.GetById(linkedId) is null)
        {
            throw new BusinessException("Mon lien ket khong ton tai.");
        }
        if (item.LinkedMenuItemId is int linkedMenuItemId)
        {
            if (group.ChoiceItems.Any(x => x.ChoiceItemId != excludedChoiceItemId
                && x.LinkedMenuItemId == linkedMenuItemId))
            {
                throw new BusinessException("Mon nay da duoc lien ket trong nhom lua chon.");
            }
            if (_choices.GetMappings(linkedMenuItemId).Any(x => x.ChoiceGroupId == item.ChoiceGroupId))
            {
                throw new BusinessException("Khong the lien ket choice ve chinh mon dang su dung nhom nay.");
            }
        }
    }

    private static string NormalizeName(string value, string field)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new BusinessException($"{field} khong duoc de trong.");
        }
        return normalized;
    }

    private static void ValidateGroupDefaults(ChoiceGroup group)
    {
        if (group.DefaultMinSelect < 0
            || group.DefaultMaxSelect < 1
            || group.DefaultMinSelect > group.DefaultMaxSelect)
        {
            throw new BusinessException("Min/Max mặc định của nhóm không hợp lệ.");
        }
    }

    private static void EnsureAdmin(UserRole role)
    {
        if (role != UserRole.Admin)
        {
            throw new BusinessException("Chi Admin duoc phep quan ly lua chon.");
        }
    }
}
