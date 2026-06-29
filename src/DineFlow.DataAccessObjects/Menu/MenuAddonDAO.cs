using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Menu;

// Compatibility adapter for the current WPF screen. Persistence is exclusively
// backed by the optimized ChoiceGroup/ChoiceItem schema.
public class MenuAddonDAO
{
    private readonly AppDbContext? _dbContext;

    public MenuAddonDAO() { }

    public MenuAddonDAO(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<MenuAddonGroup> GetAllGroups() => UseDb(db => db.ChoiceGroups
        .Include(x => x.ChoiceItems.OrderBy(i => i.DisplayOrder))
        .ThenInclude(x => x.LinkedMenuItem)
        .AsNoTracking()
        .OrderBy(x => x.GroupName)
        .AsEnumerable()
        .Select(ToLegacyGroup)
        .ToList());

    public List<MenuAddonOption> GetAllOptions() => UseDb(db => db.ChoiceItems
        .Include(x => x.LinkedMenuItem)
        .AsNoTracking()
        .OrderBy(x => x.ChoiceName)
        .AsEnumerable()
        .Select(ToLegacyOption)
        .ToList());

    public List<MenuAddonGroup> GetGroupsByParentMenuItemId(int parentMenuItemId) =>
        GetGroupMappingsByMenuItemId(parentMenuItemId)
            .Select(x => x.MenuAddonGroup!)
            .ToList();

    public List<MenuItemAddonGroup> GetGroupMappingsByMenuItemId(int menuItemId) => UseDb(db => db.MenuItemChoiceGroups
        .Include(x => x.ChoiceGroup!)
        .ThenInclude(x => x.ChoiceItems.OrderBy(i => i.DisplayOrder))
        .ThenInclude(x => x.LinkedMenuItem)
        .AsNoTracking()
        .Where(x => x.MenuItemId == menuItemId && x.ChoiceGroup!.IsAvailable)
        .OrderBy(x => x.DisplayOrder)
        .AsEnumerable()
        .Select(ToLegacyMapping)
        .ToList());

    public List<MenuAddonGroup> GetActiveGroupsByParentMenuItemIds(IEnumerable<int> parentMenuItemIds)
    {
        var ids = parentMenuItemIds.Distinct().ToList();
        return UseDb(db => db.MenuItemChoiceGroups
            .Include(x => x.ChoiceGroup!)
            .ThenInclude(x => x.ChoiceItems.OrderBy(i => i.DisplayOrder))
            .ThenInclude(x => x.LinkedMenuItem)
            .AsNoTracking()
            .Where(x => ids.Contains(x.MenuItemId) && x.ChoiceGroup!.IsAvailable)
            .AsEnumerable()
            .Select(x => ToLegacyGroup(x.ChoiceGroup!))
            .ToList());
    }

    public MenuAddonGroup? GetGroupById(int menuAddonGroupId) => UseDb(db =>
    {
        var group = db.ChoiceGroups
            .Include(x => x.ChoiceItems.OrderBy(i => i.DisplayOrder))
            .ThenInclude(x => x.LinkedMenuItem)
            .AsNoTracking()
            .FirstOrDefault(x => x.ChoiceGroupId == menuAddonGroupId);
        return group is null ? null : ToLegacyGroup(group);
    });

    public MenuAddonGroup AddGroup(MenuAddonGroup group) => UseDb(db =>
    {
        var entity = new ChoiceGroup { GroupName = group.GroupName, IsAvailable = group.IsActive };
        db.ChoiceGroups.Add(entity);
        db.SaveChanges();
        group.MenuAddonGroupId = entity.ChoiceGroupId;
        return group;
    });

    public void UpdateGroup(MenuAddonGroup group) => UseDb(db =>
    {
        var entity = db.ChoiceGroups.Find(group.MenuAddonGroupId)
            ?? throw new BusinessException("Nhom lua chon khong ton tai.");
        entity.GroupName = group.GroupName;
        entity.IsAvailable = group.IsActive;
        db.SaveChanges();
    });

    public MenuAddonOption AddOption(MenuAddonOption option) =>
        throw new BusinessException("ChoiceItem phai duoc tao truc tiep trong mot ChoiceGroup.");

    public MenuAddonOption? GetOptionById(int menuAddonOptionId) => UseDb(db =>
    {
        var item = db.ChoiceItems.Include(x => x.LinkedMenuItem)
            .AsNoTracking()
            .FirstOrDefault(x => x.ChoiceItemId == menuAddonOptionId);
        return item is null ? null : ToLegacyOption(item);
    });

    public void UpdateOption(MenuAddonOption option) => UseDb(db =>
    {
        var item = db.ChoiceItems.Find(option.MenuAddonOptionId)
            ?? throw new BusinessException("Lua chon khong ton tai.");
        item.ChoiceName = option.OptionName;
        item.LinkedMenuItemId = option.LinkedMenuItemId;
        item.IsAvailable = option.IsActive;
        db.SaveChanges();
    });

    public MenuItemAddonGroup AssignGroupToMenuItem(MenuItemAddonGroup mapping) => UseDb(db =>
    {
        var entity = new MenuItemChoiceGroup
        {
            MenuItemId = mapping.MenuItemId,
            ChoiceGroupId = mapping.MenuAddonGroupId,
            IsRequired = mapping.IsRequired,
            MinSelect = mapping.MinSelect,
            MaxSelect = mapping.MaxSelect,
            DisplayOrder = mapping.DisplayOrder
        };
        db.MenuItemChoiceGroups.Add(entity);
        db.SaveChanges();
        return mapping;
    });

    public MenuItemAddonGroup? GetMenuItemAddonGroup(int menuItemId, int menuAddonGroupId) => UseDb(db =>
    {
        var mapping = db.MenuItemChoiceGroups.AsNoTracking()
            .FirstOrDefault(x => x.MenuItemId == menuItemId && x.ChoiceGroupId == menuAddonGroupId);
        return mapping is null ? null : ToLegacyMapping(mapping);
    });

    public void UpdateMenuItemAddonGroup(MenuItemAddonGroup mapping) => UseDb(db =>
    {
        var entity = db.MenuItemChoiceGroups.Find(mapping.MenuItemId, mapping.MenuAddonGroupId)
            ?? throw new BusinessException("Mapping nhom lua chon khong ton tai.");
        entity.IsRequired = mapping.IsRequired;
        entity.MinSelect = mapping.MinSelect;
        entity.MaxSelect = mapping.MaxSelect;
        entity.DisplayOrder = mapping.DisplayOrder;
        if (!mapping.IsActive)
        {
            db.MenuItemChoiceGroups.Remove(entity);
        }
        db.SaveChanges();
    });

    public AddonGroupOption AddOptionToGroup(AddonGroupOption mapping) =>
        throw new BusinessException("Hay tao ChoiceItem voi ChoiceGroupId thay vi mapping option trung gian.");

    public AddonGroupOption? GetAddonGroupOptionById(int addonGroupOptionId) => UseDb(db =>
    {
        var item = db.ChoiceItems.Include(x => x.ChoiceGroup).Include(x => x.LinkedMenuItem)
            .AsNoTracking().FirstOrDefault(x => x.ChoiceItemId == addonGroupOptionId);
        return item is null ? null : ToLegacyGroupOption(item);
    });

    public AddonGroupOption? GetAddonGroupOption(int menuAddonGroupId, int menuAddonOptionId) => UseDb(db =>
    {
        var item = db.ChoiceItems.Include(x => x.ChoiceGroup).Include(x => x.LinkedMenuItem)
            .AsNoTracking()
            .FirstOrDefault(x => x.ChoiceGroupId == menuAddonGroupId && x.ChoiceItemId == menuAddonOptionId);
        return item is null ? null : ToLegacyGroupOption(item);
    });

    public void UpdateAddonGroupOption(AddonGroupOption mapping) => UseDb(db =>
    {
        var item = db.ChoiceItems.Find(mapping.AddonGroupOptionId)
            ?? throw new BusinessException("Lua chon khong ton tai.");
        item.ExtraPrice = mapping.ExtraPrice ?? 0;
        item.DisplayOrder = mapping.DisplayOrder;
        item.IsAvailable = mapping.IsActive;
        db.SaveChanges();
    });

    public int CountDefaultOptions(int menuAddonGroupId, int? excludeAddonGroupOptionId = null) => 0;

    private static MenuAddonGroup ToLegacyGroup(ChoiceGroup group) => new()
    {
        MenuAddonGroupId = group.ChoiceGroupId,
        GroupName = group.GroupName,
        IsActive = group.IsAvailable,
        Options = group.ChoiceItems.Select(ToLegacyGroupOption).ToList()
    };

    private static MenuAddonOption ToLegacyOption(ChoiceItem item) => new()
    {
        MenuAddonOptionId = item.ChoiceItemId,
        OptionName = item.ChoiceName,
        LinkedMenuItemId = item.LinkedMenuItemId,
        LinkedMenuItem = item.LinkedMenuItem,
        IsActive = item.IsAvailable
    };

    private static AddonGroupOption ToLegacyGroupOption(ChoiceItem item) => new()
    {
        AddonGroupOptionId = item.ChoiceItemId,
        MenuAddonGroupId = item.ChoiceGroupId,
        MenuAddonOptionId = item.ChoiceItemId,
        ExtraPrice = item.ExtraPrice,
        DisplayOrder = item.DisplayOrder,
        IsActive = item.IsAvailable,
        MenuAddonGroup = item.ChoiceGroup is null ? null : ToLegacyGroupWithoutOptions(item.ChoiceGroup),
        MenuAddonOption = ToLegacyOption(item)
    };

    private static MenuAddonGroup ToLegacyGroupWithoutOptions(ChoiceGroup group) => new()
    {
        MenuAddonGroupId = group.ChoiceGroupId,
        GroupName = group.GroupName,
        IsActive = group.IsAvailable
    };

    private static MenuItemAddonGroup ToLegacyMapping(MenuItemChoiceGroup mapping) => new()
    {
        MenuItemAddonGroupId = HashCode.Combine(mapping.MenuItemId, mapping.ChoiceGroupId),
        MenuItemId = mapping.MenuItemId,
        MenuAddonGroupId = mapping.ChoiceGroupId,
        IsRequired = mapping.IsRequired,
        MinSelect = mapping.MinSelect,
        MaxSelect = mapping.MaxSelect,
        DisplayOrder = mapping.DisplayOrder,
        IsActive = true,
        MenuAddonGroup = mapping.ChoiceGroup is null ? null : ToLegacyGroup(mapping.ChoiceGroup)
    };

    private TResult UseDb<TResult>(Func<AppDbContext, TResult> action)
    {
        if (_dbContext is not null) return action(_dbContext);
        using var db = new AppDbContext();
        return action(db);
    }

    private void UseDb(Action<AppDbContext> action)
    {
        if (_dbContext is not null) { action(_dbContext); return; }
        using var db = new AppDbContext();
        action(db);
    }
}
