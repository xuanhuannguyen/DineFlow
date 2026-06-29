using DineFlow.BusinessObjects.Menu;
using DineFlow.BusinessObjects.Common;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.Repositories.Menu;

public class ChoiceRepository : IChoiceRepository
{
    private readonly AppDbContext _db;

    public ChoiceRepository(AppDbContext db)
    {
        _db = db;
    }

    public List<ChoiceGroup> GetGroups() => _db.ChoiceGroups
        .Include(x => x.ChoiceItems.OrderBy(i => i.DisplayOrder))
        .ThenInclude(x => x.LinkedMenuItem)
        .Include(x => x.MenuItems)
        .ThenInclude(x => x.MenuItem)
        .ThenInclude(x => x!.Category)
        .AsNoTracking()
        .OrderBy(x => x.ChoiceGroupId)
        .ToList();

    public ChoiceGroup? GetGroup(int choiceGroupId) => _db.ChoiceGroups
        .Include(x => x.ChoiceItems.OrderBy(i => i.DisplayOrder))
        .ThenInclude(x => x.LinkedMenuItem)
        .FirstOrDefault(x => x.ChoiceGroupId == choiceGroupId);

    public ChoiceItem? GetChoiceItem(int choiceItemId) => _db.ChoiceItems
        .Include(x => x.LinkedMenuItem)
        .FirstOrDefault(x => x.ChoiceItemId == choiceItemId);

    public List<MenuItemChoiceGroup> GetMappings(int menuItemId) => _db.MenuItemChoiceGroups
        .Include(x => x.ChoiceGroup!)
        .ThenInclude(x => x.ChoiceItems.OrderBy(i => i.DisplayOrder))
        .ThenInclude(x => x.LinkedMenuItem)
        .AsNoTracking()
        .Where(x => x.MenuItemId == menuItemId)
        .OrderBy(x => x.DisplayOrder)
        .ToList();

    public ChoiceGroup AddGroup(ChoiceGroup group)
    {
        _db.ChoiceGroups.Add(group);
        _db.SaveChanges();
        return group;
    }

    public ChoiceItem AddChoiceItem(ChoiceItem item)
    {
        _db.ChoiceItems.Add(item);
        _db.SaveChanges();
        return item;
    }

    public MenuItemChoiceGroup UpsertMapping(MenuItemChoiceGroup mapping)
    {
        var existing = _db.MenuItemChoiceGroups.Find(mapping.MenuItemId, mapping.ChoiceGroupId);
        if (existing is null)
        {
            _db.MenuItemChoiceGroups.Add(mapping);
            existing = mapping;
        }
        else
        {
            existing.IsRequired = mapping.IsRequired;
            existing.MinSelect = mapping.MinSelect;
            existing.MaxSelect = mapping.MaxSelect;
            existing.DisplayOrder = mapping.DisplayOrder;
        }

        _db.SaveChanges();
        return existing;
    }

    public void UpdateGroup(ChoiceGroup group)
    {
        _db.ChoiceGroups.Update(group);
        _db.SaveChanges();
    }

    public void UpdateChoiceItem(ChoiceItem item)
    {
        _db.ChoiceItems.Update(item);
        _db.SaveChanges();
    }

    public void DeleteChoiceItem(int choiceItemId)
    {
        var item = _db.ChoiceItems.FirstOrDefault(x => x.ChoiceItemId == choiceItemId)
            ?? throw new BusinessException("Lựa chọn không tồn tại hoặc đã được xóa.");
        if (_db.OrderItemSelectedChoices.Any(x => x.ChoiceItemId == choiceItemId))
        {
            throw new BusinessException("Không thể xóa lựa chọn đã phát sinh trong đơn hàng.");
        }

        _db.ChoiceItems.Remove(item);
        _db.SaveChanges();
    }

    public void DeleteGroup(int choiceGroupId)
    {
        var group = _db.ChoiceGroups
            .Include(x => x.ChoiceItems)
            .Include(x => x.MenuItems)
            .FirstOrDefault(x => x.ChoiceGroupId == choiceGroupId)
            ?? throw new BusinessException("Nhóm lựa chọn không tồn tại hoặc đã được xóa.");

        if (_db.OrderItemSelectedChoices.Any(x => x.ChoiceGroupId == choiceGroupId))
        {
            throw new BusinessException("Không thể xóa nhóm đã phát sinh trong đơn hàng. Bạn có thể tạm ẩn nhóm thay vì xóa.");
        }

        _db.MenuItemChoiceGroups.RemoveRange(group.MenuItems);
        _db.ChoiceGroups.Remove(group);
        _db.SaveChanges();
    }

    public void RemoveMapping(int menuItemId, int choiceGroupId)
    {
        var mapping = _db.MenuItemChoiceGroups.Find(menuItemId, choiceGroupId);
        if (mapping is null)
        {
            return;
        }

        _db.MenuItemChoiceGroups.Remove(mapping);
        _db.SaveChanges();
    }
}
