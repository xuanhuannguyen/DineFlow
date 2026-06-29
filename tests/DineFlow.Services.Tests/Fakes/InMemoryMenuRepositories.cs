using DineFlow.BusinessObjects.Menu;
using DineFlow.BusinessObjects.Common;
using DineFlow.Repositories.Menu;

namespace DineFlow.Services.Tests.Fakes;

public sealed class InMemoryMenuItemRepository : IMenuItemRepository
{
    private readonly InMemoryMenuData _data;

    public InMemoryMenuItemRepository(InMemoryMenuData data)
    {
        _data = data;
    }

    public List<MenuItem> GetAll() => _data.Items.ToList();

    public List<MenuItem> GetCustomerMenuItems()
    {
        return _data.Items
            .Where(x => x.Status == MenuItemStatus.Active
                && x.VisibilityStatus == VisibilityStatus.Visible
                && x.IsActive
                && x.CanOrderStandalone
                && x.Category is { IsActive: true })
            .OrderBy(x => x.Category!.DisplayOrder)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.ItemName)
            .ToList();
    }

    public MenuItem? GetById(int id) => _data.Items.FirstOrDefault(x => x.MenuItemId == id);

    public List<MenuItem> GetByIdsForUpdate(IEnumerable<int> ids)
    {
        var idSet = ids.ToHashSet();
        return _data.Items.Where(x => idSet.Contains(x.MenuItemId)).ToList();
    }

    public bool ExistsByName(string itemName, int? excludedMenuItemId = null)
    {
        return _data.Items.Any(x =>
            x.ItemName.Equals(itemName.Trim(), StringComparison.OrdinalIgnoreCase)
            && x.MenuItemId != excludedMenuItemId);
    }

    public bool ExistsByCode(string itemCode, int? excludedMenuItemId = null)
    {
        return _data.Items.Any(x =>
            string.Equals(x.ItemCode, itemCode, StringComparison.OrdinalIgnoreCase)
            && (!excludedMenuItemId.HasValue || x.MenuItemId != excludedMenuItemId.Value));
    }

    public List<MenuItem> Search(string keyword)
    {
        return _data.Items
            .Where(x => x.ItemCode.Equals(keyword, StringComparison.OrdinalIgnoreCase)
                || x.ItemName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public MenuItem Add(MenuItem item)
    {
        item.MenuItemId = _data.Items.Count == 0 ? 1 : _data.Items.Max(x => x.MenuItemId) + 1;
        _data.Items.Add(item);
        return item;
    }

    public void Delete(int menuItemId)
    {
        _data.Items.RemoveAll(x => x.MenuItemId == menuItemId);
    }

    public void Update(MenuItem item)
    {
    }

    public void UpdateMany(IEnumerable<MenuItem> items)
    {
    }

    public void MutateLockedItems(IEnumerable<int> ids, Action<List<MenuItem>> mutation)
    {
        mutation(GetByIdsForUpdate(ids));
    }

    public void SaveChanges()
    {
    }
}

public sealed class InMemoryMenuAddonRepository : IMenuAddonRepository
{
    private readonly InMemoryMenuData _data;

    public InMemoryMenuAddonRepository(InMemoryMenuData data)
    {
        _data = data;
    }

    public List<MenuAddonGroup> GetAllGroups() => _data.AddonGroups.ToList();

    public List<MenuAddonOption> GetAllOptions() => _data.AddonOptions.ToList();

    public List<MenuAddonGroup> GetGroupsByParentMenuItemId(int parentMenuItemId)
    {
        return GetGroupMappingsByMenuItemId(parentMenuItemId)
            .Select(x => x.MenuAddonGroup!)
            .ToList();
    }

    public List<MenuAddonGroup> GetActiveGroupsByParentMenuItemIds(IEnumerable<int> parentMenuItemIds)
    {
        var ids = parentMenuItemIds.ToHashSet();
        return _data.ItemAddonMappings
            .Where(x => ids.Contains(x.MenuItemId) && x.IsActive)
            .Select(x => x.MenuAddonGroup!)
            .ToList();
    }

    public List<MenuItemAddonGroup> GetGroupMappingsByMenuItemId(int menuItemId)
    {
        return _data.ItemAddonMappings
            .Where(x => x.MenuItemId == menuItemId)
            .Select(CloneMapping)
            .ToList();
    }

    public MenuAddonGroup? GetGroupById(int menuAddonGroupId)
        => _data.AddonGroups.FirstOrDefault(x => x.MenuAddonGroupId == menuAddonGroupId);

    public MenuAddonGroup AddGroup(MenuAddonGroup group)
    {
        group.MenuAddonGroupId = _data.AddonGroups.Count == 0 ? 1 : _data.AddonGroups.Max(x => x.MenuAddonGroupId) + 1;
        _data.AddonGroups.Add(group);
        return group;
    }

    public void UpdateGroup(MenuAddonGroup group)
    {
    }

    public MenuAddonOption AddOption(MenuAddonOption option)
    {
        option.MenuAddonOptionId = _data.AddonOptions.Count == 0 ? 1 : _data.AddonOptions.Max(x => x.MenuAddonOptionId) + 1;
        _data.AddonOptions.Add(option);
        return option;
    }

    public MenuAddonOption? GetOptionById(int menuAddonOptionId)
        => _data.AddonOptions.FirstOrDefault(x => x.MenuAddonOptionId == menuAddonOptionId);

    public void UpdateOption(MenuAddonOption option)
    {
    }

    public MenuItemAddonGroup AssignGroupToMenuItem(MenuItemAddonGroup mapping)
    {
        mapping.MenuItemAddonGroupId = _data.ItemAddonMappings.Count == 0 ? 1 : _data.ItemAddonMappings.Max(x => x.MenuItemAddonGroupId) + 1;
        _data.ItemAddonMappings.Add(mapping);
        return mapping;
    }

    public MenuItemAddonGroup? GetMenuItemAddonGroup(int menuItemId, int menuAddonGroupId)
        => _data.ItemAddonMappings.FirstOrDefault(x =>
            x.MenuItemId == menuItemId && x.MenuAddonGroupId == menuAddonGroupId);

    public void UpdateMenuItemAddonGroup(MenuItemAddonGroup mapping)
    {
    }

    public AddonGroupOption AddOptionToGroup(AddonGroupOption mapping)
    {
        mapping.AddonGroupOptionId = _data.GroupOptions.Count == 0 ? 1 : _data.GroupOptions.Max(x => x.AddonGroupOptionId) + 1;
        _data.GroupOptions.Add(mapping);
        return mapping;
    }

    public AddonGroupOption? GetAddonGroupOptionById(int addonGroupOptionId)
        => _data.GroupOptions.FirstOrDefault(x => x.AddonGroupOptionId == addonGroupOptionId);

    public AddonGroupOption? GetAddonGroupOption(int menuAddonGroupId, int menuAddonOptionId)
        => _data.GroupOptions.FirstOrDefault(x =>
            x.MenuAddonGroupId == menuAddonGroupId && x.MenuAddonOptionId == menuAddonOptionId);

    public void UpdateAddonGroupOption(AddonGroupOption mapping)
    {
    }

    public int CountDefaultOptions(int menuAddonGroupId, int? excludeAddonGroupOptionId = null)
    {
        return _data.GroupOptions.Count(x =>
            x.MenuAddonGroupId == menuAddonGroupId
            && x.IsActive
            && x.IsDefault
            && (excludeAddonGroupOptionId == null || x.AddonGroupOptionId != excludeAddonGroupOptionId));
    }

    private MenuItemAddonGroup CloneMapping(MenuItemAddonGroup source)
    {
        var group = _data.AddonGroups.First(x => x.MenuAddonGroupId == source.MenuAddonGroupId);
        var options = _data.GroupOptions
            .Where(x => x.MenuAddonGroupId == group.MenuAddonGroupId)
            .Select(CloneGroupOption)
            .ToList();

        return new MenuItemAddonGroup
        {
            MenuItemAddonGroupId = source.MenuItemAddonGroupId,
            MenuItemId = source.MenuItemId,
            MenuItem = _data.Items.FirstOrDefault(x => x.MenuItemId == source.MenuItemId),
            MenuAddonGroupId = source.MenuAddonGroupId,
            MenuAddonGroup = new MenuAddonGroup
            {
                MenuAddonGroupId = group.MenuAddonGroupId,
                GroupName = group.GroupName,
                Description = group.Description,
                DisplayOrder = group.DisplayOrder,
                IsActive = group.IsActive,
                Options = options
            },
            IsRequired = source.IsRequired,
            MinSelect = source.MinSelect,
            MaxSelect = source.MaxSelect,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive
        };
    }

    private AddonGroupOption CloneGroupOption(AddonGroupOption source)
    {
        var option = _data.AddonOptions.First(x => x.MenuAddonOptionId == source.MenuAddonOptionId);
        MenuItem? linkedItem = null;
        if (option.LinkedMenuItemId is int linkedId)
        {
            linkedItem = _data.Items.First(x => x.MenuItemId == linkedId);
        }

        return new AddonGroupOption
        {
            AddonGroupOptionId = source.AddonGroupOptionId,
            MenuAddonGroupId = source.MenuAddonGroupId,
            MenuAddonOptionId = source.MenuAddonOptionId,
            MenuAddonOption = new MenuAddonOption
            {
                MenuAddonOptionId = option.MenuAddonOptionId,
                OptionName = option.OptionName,
                LinkedMenuItemId = option.LinkedMenuItemId,
                LinkedMenuItem = linkedItem,
                IsActive = option.IsActive
            },
            ExtraPrice = source.ExtraPrice,
            IsDefault = source.IsDefault,
            AllowMultiple = source.AllowMultiple,
            MaxQuantityPerOption = source.MaxQuantityPerOption,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive
        };
    }
}

public sealed class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly InMemoryMenuData _data;

    public InMemoryCategoryRepository(InMemoryMenuData data)
    {
        _data = data;
    }

    public List<Category> GetAll() => _data.Categories.ToList();

    public List<Category> GetActive() => _data.Categories.Where(x => x.IsActive).ToList();

    public Category? GetById(int id) => _data.Categories.FirstOrDefault(x => x.CategoryId == id);

    public bool ExistsByName(string categoryName, int? excludedCategoryId = null)
    {
        return _data.Categories.Any(x =>
            x.CategoryName.Equals(categoryName.Trim(), StringComparison.OrdinalIgnoreCase)
            && x.CategoryId != excludedCategoryId);
    }

    public Category Add(Category category) => category;

    public void Update(Category category)
    {
    }
}
