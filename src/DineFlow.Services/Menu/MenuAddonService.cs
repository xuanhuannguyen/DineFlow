using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;
using DineFlow.Repositories.Menu;
using DineFlow.Services.Menu.Validation;

namespace DineFlow.Services.Menu;

public class MenuAddonService : IMenuAddonService
{
    private readonly IMenuAddonRepository _menuAddonRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IMenuItemService _menuItemService;

    public MenuAddonService() : this(new MenuAddonRepository(), new MenuItemRepository(), new MenuItemService())
    {
    }

    public MenuAddonService(
        IMenuAddonRepository menuAddonRepository,
        IMenuItemRepository menuItemRepository,
        IMenuItemService menuItemService)
    {
        _menuAddonRepository = menuAddonRepository;
        _menuItemRepository = menuItemRepository;
        _menuItemService = menuItemService;
    }

    public List<MenuAddonGroup> GetAllGroups() => _menuAddonRepository.GetAllGroups();

    public List<MenuAddonOption> GetAllOptions() => _menuAddonRepository.GetAllOptions();

    public List<MenuAddonGroup> GetGroupsByParentMenuItemId(int parentMenuItemId)
    {
        return _menuAddonRepository.GetGroupsByParentMenuItemId(parentMenuItemId);
    }

    public MenuAddonGroup CreateGroup(MenuAddonGroup group, UserRole role)
    {
        EnsureAdmin(role);
        MenuAddonValidator.ValidateGroupForSave(group);
        EnsureUniqueGroupName(group.GroupName);
        group.GroupName = group.GroupName.Trim();
        group.IsActive = true;
        return _menuAddonRepository.AddGroup(group);
    }

    public void UpdateGroup(MenuAddonGroup group, UserRole role)
    {
        EnsureAdmin(role);
        MenuAddonValidator.ValidateGroupForSave(group);
        EnsureUniqueGroupName(group.GroupName, group.MenuAddonGroupId);
        var existing = _menuAddonRepository.GetGroupById(group.MenuAddonGroupId)
            ?? throw new BusinessException("Nhom modifier khong ton tai.");

        existing.GroupName = group.GroupName.Trim();
        existing.Description = group.Description;
        existing.DisplayOrder = group.DisplayOrder;
        existing.IsActive = group.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        _menuAddonRepository.UpdateGroup(existing);
    }

    public void HideGroup(int menuAddonGroupId, UserRole role)
    {
        EnsureAdmin(role);
        var group = _menuAddonRepository.GetGroupById(menuAddonGroupId)
            ?? throw new BusinessException("Nhom modifier khong ton tai.");
        group.IsActive = false;
        group.UpdatedAt = DateTime.UtcNow;
        _menuAddonRepository.UpdateGroup(group);
    }

    public MenuAddonOption CreateOption(MenuAddonOption option, UserRole role)
    {
        EnsureAdmin(role);
        MenuAddonValidator.ValidateOptionForSave(option);
        EnsureLinkedMenuItemExists(option.LinkedMenuItemId);
        EnsureUniqueOption(option);
        option.OptionName = option.OptionName.Trim();
        option.IsActive = true;
        return _menuAddonRepository.AddOption(option);
    }

    public void UpdateOption(MenuAddonOption option, UserRole role)
    {
        EnsureAdmin(role);
        MenuAddonValidator.ValidateOptionForSave(option);
        EnsureLinkedMenuItemExists(option.LinkedMenuItemId);
        EnsureUniqueOption(option, option.MenuAddonOptionId);
        var existing = _menuAddonRepository.GetOptionById(option.MenuAddonOptionId)
            ?? throw new BusinessException("Lua chon modifier khong ton tai.");

        existing.OptionName = option.OptionName.Trim();
        existing.Description = option.Description;
        existing.LinkedMenuItemId = option.LinkedMenuItemId;
        existing.IsActive = option.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        _menuAddonRepository.UpdateOption(existing);
    }

    public void HideOption(int menuAddonOptionId, UserRole role)
    {
        EnsureAdmin(role);
        var option = _menuAddonRepository.GetOptionById(menuAddonOptionId)
            ?? throw new BusinessException("Lua chon modifier khong ton tai.");
        option.IsActive = false;
        option.UpdatedAt = DateTime.UtcNow;
        _menuAddonRepository.UpdateOption(option);
    }

    public MenuItemAddonGroup AssignGroupToMenuItem(MenuItemAddonGroup mapping, UserRole role)
    {
        EnsureAdmin(role);
        EnsureMenuItemExists(mapping.MenuItemId);
        EnsureGroupExists(mapping.MenuAddonGroupId);
        MenuAddonValidator.ValidateMenuItemGroupRules(mapping);
        EnsureGroupDoesNotSelfLink(mapping.MenuItemId, mapping.MenuAddonGroupId);

        var existing = _menuAddonRepository.GetMenuItemAddonGroup(mapping.MenuItemId, mapping.MenuAddonGroupId);
        if (existing is not null)
        {
            existing.DisplayOrder = mapping.DisplayOrder;
            existing.IsRequired = mapping.IsRequired;
            existing.MinSelect = mapping.MinSelect;
            existing.MaxSelect = mapping.MaxSelect;
            existing.IsActive = true;
            existing.UpdatedAt = DateTime.UtcNow;
            _menuAddonRepository.UpdateMenuItemAddonGroup(existing);
            return existing;
        }

        mapping.IsActive = true;
        return _menuAddonRepository.AssignGroupToMenuItem(mapping);
    }

    public void HideGroupFromMenuItem(int menuItemId, int menuAddonGroupId, UserRole role)
    {
        EnsureAdmin(role);
        var existing = _menuAddonRepository.GetMenuItemAddonGroup(menuItemId, menuAddonGroupId)
            ?? throw new BusinessException("Mon chua duoc gan nhom modifier nay.");
        existing.IsActive = false;
        existing.UpdatedAt = DateTime.UtcNow;
        _menuAddonRepository.UpdateMenuItemAddonGroup(existing);
    }

    public AddonGroupOption AddOptionToGroup(AddonGroupOption mapping, UserRole role)
    {
        EnsureAdmin(role);
        MenuAddonValidator.ValidateGroupOptionForSave(mapping);
        EnsureGroupExists(mapping.MenuAddonGroupId);
        EnsureOptionExists(mapping.MenuAddonOptionId);

        var existing = _menuAddonRepository.GetAddonGroupOption(mapping.MenuAddonGroupId, mapping.MenuAddonOptionId);
        EnsureSingleDefaultOption(mapping, existing?.AddonGroupOptionId);
        if (existing is not null)
        {
            existing.ExtraPrice = mapping.ExtraPrice;
            existing.IsDefault = mapping.IsDefault;
            existing.AllowMultiple = mapping.AllowMultiple;
            existing.MaxQuantityPerOption = mapping.MaxQuantityPerOption;
            existing.DisplayOrder = mapping.DisplayOrder;
            existing.IsActive = true;
            existing.UpdatedAt = DateTime.UtcNow;
            _menuAddonRepository.UpdateAddonGroupOption(existing);
            return existing;
        }

        mapping.IsActive = true;
        return _menuAddonRepository.AddOptionToGroup(mapping);
    }

    public void UpdateGroupOption(AddonGroupOption mapping, UserRole role)
    {
        EnsureAdmin(role);
        MenuAddonValidator.ValidateGroupOptionForSave(mapping);
        EnsureSingleDefaultOption(mapping, mapping.AddonGroupOptionId);
        var existing = _menuAddonRepository.GetAddonGroupOptionById(mapping.AddonGroupOptionId)
            ?? throw new BusinessException("Mapping modifier option khong ton tai.");

        existing.MenuAddonOptionId = mapping.MenuAddonOptionId;
        existing.ExtraPrice = mapping.ExtraPrice;
        existing.IsDefault = mapping.IsDefault;
        existing.AllowMultiple = mapping.AllowMultiple;
        existing.MaxQuantityPerOption = mapping.MaxQuantityPerOption;
        existing.DisplayOrder = mapping.DisplayOrder;
        existing.IsActive = mapping.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        _menuAddonRepository.UpdateAddonGroupOption(existing);
    }

    public void HideOptionFromGroup(int addonGroupOptionId, UserRole role)
    {
        EnsureAdmin(role);
        var existing = _menuAddonRepository.GetAddonGroupOptionById(addonGroupOptionId)
            ?? throw new BusinessException("Mapping modifier option khong ton tai.");
        existing.IsActive = false;
        existing.UpdatedAt = DateTime.UtcNow;
        _menuAddonRepository.UpdateAddonGroupOption(existing);
    }

    public bool ValidateAddonsForOrder(List<OrderItemRequestDto> items)
    {
        return _menuItemService.ValidateAddonsForOrder(items);
    }

    public Task<bool> ValidateAddonsForOrderAsync(List<OrderItemRequestDto> items)
    {
        return _menuItemService.ValidateAddonsForOrderAsync(items);
    }

    public List<AddonSnapshotDto> GetAddonSnapshotsForOrder(List<OrderItemRequestDto> items)
    {
        return _menuItemService.GetAddonSnapshotsForOrder(items);
    }

    public Task<List<AddonSnapshotDto>> GetAddonSnapshotsForOrderAsync(List<OrderItemRequestDto> items)
    {
        return _menuItemService.GetAddonSnapshotsForOrderAsync(items);
    }

    private void EnsureMenuItemExists(int menuItemId)
    {
        if (_menuItemRepository.GetById(menuItemId) is null)
        {
            throw new BusinessException("Mon khong ton tai.");
        }
    }

    private void EnsureGroupExists(int groupId)
    {
        if (_menuAddonRepository.GetGroupById(groupId) is null)
        {
            throw new BusinessException("Nhom modifier khong ton tai.");
        }
    }

    private void EnsureOptionExists(int optionId)
    {
        if (_menuAddonRepository.GetOptionById(optionId) is null)
        {
            throw new BusinessException("Lua chon modifier khong ton tai.");
        }
    }

    private void EnsureGroupDoesNotSelfLink(int menuItemId, int groupId)
    {
        var group = _menuAddonRepository.GetGroupById(groupId)
            ?? throw new BusinessException("Nhom modifier khong ton tai.");

        if (group.Options.Any(x => x.MenuAddonOption?.LinkedMenuItemId == menuItemId))
        {
            throw new BusinessException("Nhom modifier co option dang link nguoc ve chinh mon nay.");
        }
    }

    private void EnsureLinkedMenuItemExists(int? menuItemId)
    {
        if (menuItemId is not null && _menuItemRepository.GetById(menuItemId.Value) is null)
        {
            throw new BusinessException("Mon lien ket voi modifier option khong ton tai.");
        }
    }

    private void EnsureUniqueGroupName(string groupName, int? excludedGroupId = null)
    {
        if (_menuAddonRepository.GetAllGroups().Any(x =>
            x.GroupName.Equals(groupName.Trim(), StringComparison.OrdinalIgnoreCase)
            && (!excludedGroupId.HasValue || x.MenuAddonGroupId != excludedGroupId.Value)))
        {
            throw new BusinessException("Ten nhom modifier da ton tai.");
        }
    }

    private void EnsureUniqueOption(MenuAddonOption option, int? excludedOptionId = null)
    {
        var options = _menuAddonRepository.GetAllOptions();
        if (options.Any(x =>
            x.OptionName.Equals(option.OptionName.Trim(), StringComparison.OrdinalIgnoreCase)
            && (!excludedOptionId.HasValue || x.MenuAddonOptionId != excludedOptionId.Value)))
        {
            throw new BusinessException("Ten lua chon modifier da ton tai.");
        }

        if (option.LinkedMenuItemId is int linkedMenuItemId
            && options.Any(x =>
                x.LinkedMenuItemId == linkedMenuItemId
                && (!excludedOptionId.HasValue || x.MenuAddonOptionId != excludedOptionId.Value)))
        {
            throw new BusinessException("Mon nay da duoc dung lam lua chon modifier.");
        }
    }

    private void EnsureSingleDefaultOption(AddonGroupOption mapping, int? excludedAddonGroupOptionId)
    {
        if (!mapping.IsDefault)
        {
            return;
        }

        if (_menuAddonRepository.CountDefaultOptions(mapping.MenuAddonGroupId, excludedAddonGroupOptionId) > 0)
        {
            throw new BusinessException("Moi nhom modifier chi duoc co mot lua chon mac dinh.");
        }
    }

    private static void EnsureAdmin(UserRole role)
    {
        if (role != UserRole.Admin)
        {
            throw new BusinessException("Chi Admin duoc phep quan ly modifier.");
        }
    }
}
