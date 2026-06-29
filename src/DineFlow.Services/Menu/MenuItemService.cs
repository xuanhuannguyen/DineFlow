using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;
using DineFlow.Repositories.Menu;
using DineFlow.Services.Menu.Validation;

namespace DineFlow.Services.Menu;

public class MenuItemService : IMenuItemService
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IMenuAddonRepository _menuAddonRepository;
    private readonly ICategoryRepository _categoryRepository;

    public MenuItemService() : this(new MenuItemRepository(), new MenuAddonRepository(), new CategoryRepository())
    {
    }

    public MenuItemService(
        IMenuItemRepository menuItemRepository,
        IMenuAddonRepository menuAddonRepository,
        ICategoryRepository categoryRepository)
    {
        _menuItemRepository = menuItemRepository;
        _menuAddonRepository = menuAddonRepository;
        _categoryRepository = categoryRepository;
    }

    public List<MenuItem> GetAll() => _menuItemRepository.GetAll();
    public List<MenuItem> GetCustomerMenuItems() => _menuItemRepository.GetCustomerMenuItems();
    public MenuItem? GetById(int menuItemId) => _menuItemRepository.GetById(menuItemId);
    public List<MenuAddonGroup> GetAddonGroups(int parentMenuItemId) => _menuAddonRepository.GetGroupsByParentMenuItemId(parentMenuItemId);
    public List<MenuItemAddonGroup> GetAddonGroupMappings(int parentMenuItemId) => _menuAddonRepository.GetGroupMappingsByMenuItemId(parentMenuItemId);

    public List<MenuItem> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return _menuItemRepository.GetAll();
        }

        return _menuItemRepository.Search(keyword.Trim());
    }

    public bool ValidateOrderableItems(List<OrderItemRequestDto> items)
    {
        var preparedOrder = PrepareOrder(items, applyDefaultAddons: true);
        var requestedItems = BuildStockRequests(preparedOrder);
        var menuItems = _menuItemRepository.GetByIdsForUpdate(requestedItems.Select(x => x.MenuItemId));

        foreach (var request in preparedOrder.Items)
        {
            var item = menuItems.FirstOrDefault(x => x.MenuItemId == request.MenuItemId);
            EnsureMenuItemCanBeOrdered(item, request.Quantity, requireStandalone: true);
        }

        foreach (var request in requestedItems)
        {
            var item = menuItems.FirstOrDefault(x => x.MenuItemId == request.MenuItemId);
            EnsureMenuItemCanBeOrdered(item, request.Quantity, requireStandalone: false);
        }

        return true;
    }

    public Task<bool> ValidateOrderableItemsAsync(List<OrderItemRequestDto> items)
    {
        return Task.FromResult(ValidateOrderableItems(items));
    }

    public bool ValidateAddonsForOrder(List<OrderItemRequestDto> items)
    {
        PrepareOrder(items, applyDefaultAddons: true);
        return true;
    }

    public Task<bool> ValidateAddonsForOrderAsync(List<OrderItemRequestDto> items)
    {
        return Task.FromResult(ValidateAddonsForOrder(items));
    }

    public List<AddonSnapshotDto> GetAddonSnapshotsForOrder(List<OrderItemRequestDto> items)
    {
        var preparedOrder = PrepareOrder(items, applyDefaultAddons: true);
        var snapshots = new List<AddonSnapshotDto>();

        foreach (var resolved in preparedOrder.Addons)
        {
            var group = resolved.Mapping.MenuAddonGroup!;
            var option = resolved.Option;
            var linkedMenuItem = option.MenuAddonOption?.LinkedMenuItem;
            var unitPrice = ResolveOptionUnitPrice(option);
            var quantity = resolved.Addon.Quantity * resolved.Parent.Quantity;
            var optionName = option.MenuAddonOption?.OptionName ?? string.Empty;

            snapshots.Add(new AddonSnapshotDto
            {
                ParentMenuItemId = resolved.Parent.MenuItemId,
                AddonGroupOptionId = option.AddonGroupOptionId,
                MenuAddonGroupId = group.MenuAddonGroupId,
                GroupName = group.GroupName,
                MenuAddonOptionId = option.MenuAddonOptionId,
                AddonMenuItemId = linkedMenuItem?.MenuItemId ?? 0,
                OptionName = optionName,
                ItemName = linkedMenuItem?.ItemName ?? optionName,
                Quantity = quantity,
                UnitPrice = unitPrice,
                TotalPrice = unitPrice * quantity
            });
        }

        return snapshots;
    }

    public Task<List<AddonSnapshotDto>> GetAddonSnapshotsForOrderAsync(List<OrderItemRequestDto> items)
    {
        return Task.FromResult(GetAddonSnapshotsForOrder(items));
    }

    public void ReserveStockForOrder(List<OrderItemRequestDto> items)
    {
        var preparedOrder = PrepareOrder(items, applyDefaultAddons: true);
        var requestedItems = BuildStockRequests(preparedOrder);

        _menuItemRepository.MutateLockedItems(requestedItems.Select(x => x.MenuItemId), menuItems =>
        {
            foreach (var request in preparedOrder.Items)
            {
                var item = menuItems.FirstOrDefault(x => x.MenuItemId == request.MenuItemId);
                EnsureMenuItemCanBeOrdered(item, request.Quantity, requireStandalone: true);
            }

            foreach (var request in requestedItems)
            {
                var item = menuItems.FirstOrDefault(x => x.MenuItemId == request.MenuItemId);
                EnsureMenuItemCanBeOrdered(item, request.Quantity, requireStandalone: false);
                item!.ReserveStock(request.Quantity);
            }
        });
    }

    public Task ReserveStockForOrderAsync(List<OrderItemRequestDto> items)
    {
        ReserveStockForOrder(items);
        return Task.CompletedTask;
    }

    public void RollbackStockForCancelledOrder(List<OrderItemRequestDto> items)
    {
        OrderRequestValidator.ValidateForOrdering(items);
        var preparedOrder = PrepareOrder(items, applyDefaultAddons: true);
        var requestedItems = BuildStockRequests(preparedOrder);

        _menuItemRepository.MutateLockedItems(requestedItems.Select(x => x.MenuItemId), menuItems =>
        {
            foreach (var request in requestedItems)
            {
                var item = menuItems.FirstOrDefault(x => x.MenuItemId == request.MenuItemId)
                    ?? throw new BusinessException("Mon can hoan kho khong ton tai.");

                item.RestoreStock(request.Quantity);
            }
        });
    }

    public Task RollbackStockForCancelledOrderAsync(List<OrderItemRequestDto> items)
    {
        RollbackStockForCancelledOrder(items);
        return Task.CompletedTask;
    }

    public MenuItemSnapshotDto GetMenuItemSnapshot(int menuItemId)
    {
        var item = _menuItemRepository.GetById(menuItemId)
            ?? throw new BusinessException("Mon khong ton tai.");

        return new MenuItemSnapshotDto
        {
            MenuItemId = item.MenuItemId,
            ItemName = item.ItemName,
            UnitPrice = item.Price,
            AvailableQuantity = item.AvailableQuantity,
            TrackStock = item.TrackStock,
            IsActive = item.IsActive,
            IsAvailable = item.IsAvailable
        };
    }

    public Task<MenuItemSnapshotDto> GetMenuItemSnapshotAsync(int menuItemId)
    {
        return Task.FromResult(GetMenuItemSnapshot(menuItemId));
    }

    public MenuItem Create(MenuItem item) => Create(item, UserRole.Admin);

    public MenuItem Create(MenuItem item, UserRole role)
    {
        EnsureAdmin(role);
        MenuItemValidator.ValidateForSave(item);
        EnsureCategoryActive(item.CategoryId);
        EnsureUniqueName(item.ItemName);
        EnsureUniqueCode(item.ItemCode);
        NormalizeMenuState(item);
        item.ApplyStockAvailabilityRule();
        return _menuItemRepository.Add(item);
    }

    public void Update(MenuItem item) => Update(item, UserRole.Admin);

    public void Update(MenuItem item, UserRole role)
    {
        EnsureAdmin(role);
        MenuItemValidator.ValidateForSave(item);
        EnsureCategoryActive(item.CategoryId);
        EnsureUniqueName(item.ItemName, item.MenuItemId);
        EnsureUniqueCode(item.ItemCode, item.MenuItemId);
        PreserveOperationalStockFields(item);
        NormalizeMenuState(item);
        item.ApplyStockAvailabilityRule();
        item.UpdatedAt = DateTime.UtcNow;
        _menuItemRepository.Update(item);
    }

    public void SoftDelete(int menuItemId, UserRole role)
    {
        EnsureAdmin(role);
        _menuItemRepository.Delete(menuItemId);
    }

    public void UpdateStock(int menuItemId, int? availableQuantity, UserRole role)
    {
        UpdateStock(menuItemId, availableQuantity, staffNote: null, role);
    }

    public void UpdateStock(int menuItemId, int? availableQuantity, string? staffNote, UserRole role)
    {
        EnsureStockOperator(role);
        var item = _menuItemRepository.GetById(menuItemId)
            ?? throw new BusinessException("Mon khong ton tai.");

        item.SetStockQuantity(availableQuantity, staffNote);
        _menuItemRepository.Update(item);
    }

    public void SetAvailability(int menuItemId, bool isAvailable, UserRole role)
    {
        SetAvailability(menuItemId, isAvailable, soldOutReason: null, staffNote: null, role);
    }

    public void SetAvailability(int menuItemId, bool isAvailable, string? soldOutReason, string? staffNote, UserRole role)
    {
        EnsureStockOperator(role);
        var item = _menuItemRepository.GetById(menuItemId)
            ?? throw new BusinessException("Mon khong ton tai.");

        item.SetSaleAvailability(isAvailable, soldOutReason, staffNote);
        _menuItemRepository.Update(item);
    }

    private void PreserveOperationalStockFields(MenuItem item)
    {
        var existing = _menuItemRepository.GetById(item.MenuItemId);
        if (existing is null)
        {
            return;
        }

        item.SoldOutReason = existing.SoldOutReason;
        item.StaffNote = existing.StaffNote;
    }

    private void EnsureUniqueName(string itemName, int? excludedMenuItemId = null)
    {
        if (_menuItemRepository.ExistsByName(itemName.Trim(), excludedMenuItemId))
        {
            throw new BusinessException("Ten mon da ton tai.");
        }
    }

    private void EnsureUniqueCode(string itemCode, int? excludedMenuItemId = null)
    {
        if (_menuItemRepository.ExistsByCode(itemCode.Trim().ToUpperInvariant(), excludedMenuItemId))
        {
            throw new BusinessException("Mã món đã tồn tại.");
        }
    }

    private void EnsureCategoryActive(int categoryId)
    {
        var category = _categoryRepository.GetById(categoryId)
            ?? throw new BusinessException("Category khong ton tai.");

        if (!category.IsActive)
        {
            throw new BusinessException("Khong the them mon vao loai mon da bi an.");
        }
    }

    private static void EnsureMenuItemCanBeOrdered(MenuItem? item, int quantity, bool requireStandalone)
    {
        if (item is null)
        {
            throw new BusinessException(MenuBusinessMessages.MenuItemNotFound);
        }

        item.EnsureCanBeOrdered(quantity, requireStandalone);
    }

    private static List<OrderItemRequestDto> NormalizeRequests(List<OrderItemRequestDto> items)
    {
        return items
            .GroupBy(x => x.MenuItemId)
            .Select(x => new OrderItemRequestDto
            {
                MenuItemId = x.Key,
                Quantity = x.Sum(y => y.Quantity),
                Addons = x.SelectMany(y => y.Addons)
                    .GroupBy(a => new { a.AddonGroupOptionId, a.MenuAddonGroupId, a.MenuAddonOptionId, a.AddonMenuItemId })
                    .Select(a => new OrderAddonRequestDto
                    {
                        AddonGroupOptionId = a.Key.AddonGroupOptionId,
                        MenuAddonGroupId = a.Key.MenuAddonGroupId,
                        MenuAddonOptionId = a.Key.MenuAddonOptionId,
                        AddonMenuItemId = a.Key.AddonMenuItemId,
                        Quantity = a.Sum(v => v.Quantity)
                    })
                    .ToList(),
                TouchedAddonGroupIds = x
                    .SelectMany(y => y.TouchedAddonGroupIds)
                    .Where(groupId => groupId > 0)
                    .Distinct()
                    .ToList()
            })
            .ToList();
    }

    private PreparedOrder PrepareOrder(List<OrderItemRequestDto> items, bool applyDefaultAddons)
    {
        OrderRequestValidator.ValidateForOrdering(items);
        var normalizedItems = NormalizeRequests(items);
        var resolvedAddons = new List<ResolvedAddon>();

        foreach (var request in normalizedItems)
        {
            var groupMappingsForItem = GetActiveAddonGroupMappings(request.MenuItemId);

            if (applyDefaultAddons)
            {
                ApplyDefaultAddons(request, groupMappingsForItem);
            }

            EnsureAddonGroupSelectionsSatisfyRules(request, groupMappingsForItem);
            resolvedAddons.AddRange(ResolveSelectedAddons(request, groupMappingsForItem));
        }

        return new PreparedOrder(normalizedItems, resolvedAddons);
    }

    private List<MenuItemAddonGroup> GetActiveAddonGroupMappings(int menuItemId)
    {
        return _menuAddonRepository.GetGroupMappingsByMenuItemId(menuItemId)
            .Where(x => x.IsActive)
            .ToList();
    }

    private static void EnsureAddonGroupSelectionsSatisfyRules(
        OrderItemRequestDto request,
        List<MenuItemAddonGroup> groupMappingsForItem)
    {
        foreach (var mapping in groupMappingsForItem)
        {
            var group = mapping.MenuAddonGroup
                ?? throw new BusinessException("Nhom addon khong ton tai.");
            EnsureRequiredGroupHasAvailableOptions(request.MenuItemId, mapping);

            var selectedCount = request.Addons
                .Where(x => MatchesGroup(x, mapping))
                .Sum(x => x.Quantity);

            if (mapping.IsRequired && selectedCount == 0)
            {
                throw new BusinessException($"Nhom addon '{group.GroupName}' la bat buoc.");
            }

            if (selectedCount < mapping.MinSelect)
            {
                throw new BusinessException($"Nhom addon '{group.GroupName}' chua du so luong toi thieu.");
            }

            if (selectedCount > mapping.MaxSelect)
            {
                throw new BusinessException($"Nhom addon '{group.GroupName}' vuot so luong toi da.");
            }
        }
    }

    private static List<ResolvedAddon> ResolveSelectedAddons(
        OrderItemRequestDto request,
        List<MenuItemAddonGroup> groupMappingsForItem)
    {
        var resolvedAddons = new List<ResolvedAddon>();

        foreach (var addon in request.Addons)
        {
            var resolved = ResolveAddon(groupMappingsForItem, addon);
            var option = resolved.Option;
            ValidateOptionQuantity(option, addon.Quantity);
            EnsureAddonDoesNotLinkToParentItem(option, request.MenuItemId);
            EnsureLinkedAddonItemCanBeOrdered(option, addon.Quantity * request.Quantity);
            resolvedAddons.Add(new ResolvedAddon(request, addon, resolved.Mapping, option));
        }

        return resolvedAddons;
    }

    private static void EnsureAddonDoesNotLinkToParentItem(AddonGroupOption option, int parentMenuItemId)
    {
        if (option.MenuAddonOption?.LinkedMenuItemId == parentMenuItemId)
        {
            throw new BusinessException("Mon phu khong duoc trung voi mon chinh.");
        }
    }

    private static void EnsureLinkedAddonItemCanBeOrdered(AddonGroupOption option, int requestedQuantity)
    {
        if (option.MenuAddonOption?.LinkedMenuItem is not null)
        {
            EnsureMenuItemCanBeOrdered(option.MenuAddonOption.LinkedMenuItem, requestedQuantity, requireStandalone: false);
        }
    }

    private static void ApplyDefaultAddons(OrderItemRequestDto request, List<MenuItemAddonGroup> groupMappingsForItem)
    {
        foreach (var mapping in groupMappingsForItem)
        {
            if (request.Addons.Any(x => MatchesGroup(x, mapping)))
            {
                continue;
            }

            if (request.TouchedAddonGroupIds.Contains(mapping.MenuAddonGroupId))
            {
                continue;
            }

            var defaultOption = mapping.MenuAddonGroup?.Options
                .Where(x => x.IsDefault)
                .Where(x => IsOptionOrderable(request.MenuItemId, x))
                .OrderBy(x => x.DisplayOrder)
                .FirstOrDefault();

            if (defaultOption is null)
            {
                continue;
            }

            request.Addons.Add(new OrderAddonRequestDto
            {
                AddonGroupOptionId = defaultOption.AddonGroupOptionId,
                MenuAddonGroupId = mapping.MenuAddonGroupId,
                MenuAddonOptionId = defaultOption.MenuAddonOptionId,
                AddonMenuItemId = defaultOption.MenuAddonOption?.LinkedMenuItemId ?? 0,
                Quantity = 1
            });
        }
    }

    private static List<OrderItemRequestDto> BuildStockRequests(PreparedOrder preparedOrder)
    {
        var stockRequests = preparedOrder.Items
            .Select(x => new OrderItemRequestDto { MenuItemId = x.MenuItemId, Quantity = x.Quantity })
            .ToList();

        stockRequests.AddRange(preparedOrder.Addons
            .Where(x => x.Option.MenuAddonOption?.LinkedMenuItemId > 0)
            .Select(x => new OrderItemRequestDto
            {
                MenuItemId = x.Option.MenuAddonOption!.LinkedMenuItemId!.Value,
                Quantity = x.Addon.Quantity * x.Parent.Quantity
            }));

        return stockRequests
            .GroupBy(x => x.MenuItemId)
            .Select(x => new OrderItemRequestDto
            {
                MenuItemId = x.Key,
                Quantity = x.Sum(y => y.Quantity)
            })
            .ToList();
    }

    private static bool MatchesGroup(OrderAddonRequestDto addon, MenuItemAddonGroup mapping)
    {
        var group = mapping.MenuAddonGroup;
        if (group is null)
        {
            return false;
        }

        if (addon.AddonGroupOptionId > 0)
        {
            return group.Options.Any(x => x.AddonGroupOptionId == addon.AddonGroupOptionId);
        }

        if (addon.MenuAddonGroupId > 0)
        {
            return addon.MenuAddonGroupId == group.MenuAddonGroupId;
        }

        return group.Options.Any(x => x.MenuAddonOption?.LinkedMenuItemId == addon.AddonMenuItemId);
    }

    private static (MenuItemAddonGroup Mapping, AddonGroupOption Option) ResolveAddon(
        List<MenuItemAddonGroup> groupsForItem,
        OrderAddonRequestDto addon)
    {
        foreach (var mapping in groupsForItem)
        {
            var group = mapping.MenuAddonGroup;
            if (group is null)
            {
                continue;
            }

            if (addon.MenuAddonGroupId > 0 && addon.MenuAddonGroupId != group.MenuAddonGroupId)
            {
                continue;
            }

            var option = addon.AddonGroupOptionId > 0
                ? group.Options.FirstOrDefault(x => x.AddonGroupOptionId == addon.AddonGroupOptionId)
                : addon.MenuAddonOptionId > 0
                ? group.Options.FirstOrDefault(x => x.MenuAddonOptionId == addon.MenuAddonOptionId)
                : group.Options.FirstOrDefault(x => x.MenuAddonOption?.LinkedMenuItemId == addon.AddonMenuItemId);

            if (option is not null && option.IsActive && option.MenuAddonOption is { IsActive: true })
            {
                return (mapping, option);
            }
        }

        throw new BusinessException("Mon phu khong thuoc mon chinh da chon.");
    }

    private static void ValidateOptionQuantity(AddonGroupOption option, int quantity)
    {
        if (!option.AllowMultiple && quantity != 1)
        {
            throw new BusinessException($"Lua chon '{option.MenuAddonOption?.OptionName}' chi duoc chon 1 lan.");
        }

        if (option.AllowMultiple && option.MaxQuantityPerOption.HasValue && quantity > option.MaxQuantityPerOption.Value)
        {
            throw new BusinessException($"Lua chon '{option.MenuAddonOption?.OptionName}' vuot so luong toi da.");
        }
    }

    private static void EnsureRequiredGroupHasAvailableOptions(int parentMenuItemId, MenuItemAddonGroup mapping)
    {
        if (!mapping.IsRequired || mapping.MinSelect <= 0 || mapping.MenuAddonGroup is null)
        {
            return;
        }

        var capacity = mapping.MenuAddonGroup.Options
            .Where(x => IsOptionOrderable(parentMenuItemId, x))
            .Sum(GetOptionCapacity);

        if (capacity < mapping.MinSelect)
        {
            throw new BusinessException($"Nhom addon bat buoc '{mapping.MenuAddonGroup.GroupName}' khong con du lua chon hop le.");
        }
    }

    private static bool IsOptionOrderable(int parentMenuItemId, AddonGroupOption option)
    {
        if (!option.IsActive || option.MenuAddonOption is not { IsActive: true })
        {
            return false;
        }

        var linkedItem = option.MenuAddonOption.LinkedMenuItem;
        if (linkedItem is null)
        {
            return true;
        }

        return linkedItem.MenuItemId != parentMenuItemId
            && linkedItem.Status == MenuItemStatus.Active
            && linkedItem.VisibilityStatus == VisibilityStatus.Visible
            && linkedItem.IsActive
            && linkedItem.IsAvailable
            && linkedItem.Category is { IsActive: true }
            && (!linkedItem.TrackStock || (linkedItem.AvailableQuantity ?? 0) > 0);
    }

    private static void NormalizeMenuState(MenuItem item)
    {
        if (!item.IsActive)
        {
            item.Status = item.Status == MenuItemStatus.Deleted
                ? MenuItemStatus.Deleted
                : MenuItemStatus.Inactive;
            item.VisibilityStatus = VisibilityStatus.Hidden;
            item.IsAvailable = false;
            if (item.AvailabilityStatus == AvailabilityStatus.Available)
            {
                item.AvailabilityStatus = AvailabilityStatus.TemporarilyUnavailable;
            }
        }
        else if (item.Status is not MenuItemStatus.Deleted)
        {
            item.Status = MenuItemStatus.Active;
            item.VisibilityStatus = VisibilityStatus.Visible;
        }

        if (item.ItemType == MenuItemType.AddonOnly)
        {
            item.CanOrderStandalone = false;
        }

        if (item.TrackStock && (item.AvailableQuantity ?? 0) <= 0)
        {
            item.IsAvailable = false;
            item.AvailabilityStatus = AvailabilityStatus.SoldOut;
        }
        else if (!item.IsAvailable && item.AvailabilityStatus == AvailabilityStatus.Available)
        {
            item.AvailabilityStatus = AvailabilityStatus.TemporarilyUnavailable;
        }
        else if (item.IsAvailable)
        {
            item.AvailabilityStatus = AvailabilityStatus.Available;
        }
    }

    private static int GetOptionCapacity(AddonGroupOption option)
    {
        var maxByRule = option.AllowMultiple ? option.MaxQuantityPerOption ?? int.MaxValue : 1;
        var linkedItem = option.MenuAddonOption?.LinkedMenuItem;
        if (linkedItem?.TrackStock == true)
        {
            return Math.Min(maxByRule, linkedItem.AvailableQuantity ?? 0);
        }

        return maxByRule;
    }

    private static decimal ResolveOptionUnitPrice(AddonGroupOption option)
    {
        if (option.ExtraPrice.HasValue)
        {
            return option.ExtraPrice.Value;
        }

        return option.MenuAddonOption?.LinkedMenuItem?.Price ?? 0;
    }

    private static void EnsureAdmin(UserRole role)
    {
        if (role != UserRole.Admin)
        {
            throw new BusinessException("Chi Admin duoc phep quan ly thong tin goc cua mon.");
        }
    }

    private static void EnsureStockOperator(UserRole role)
    {
        if (role is not (UserRole.Admin or UserRole.Staff))
        {
            throw new BusinessException("Nguoi dung khong co quyen van hanh kho.");
        }
    }

    private sealed record PreparedOrder(
        List<OrderItemRequestDto> Items,
        List<ResolvedAddon> Addons);

    private sealed record ResolvedAddon(
        OrderItemRequestDto Parent,
        OrderAddonRequestDto Addon,
        MenuItemAddonGroup Mapping,
        AddonGroupOption Option);
}
