using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace DineFlow.DataAccessObjects.Menu;

public class MenuItemDAO
{
    private readonly AppDbContext? _dbContext;

    public MenuItemDAO()
    {
    }

    public MenuItemDAO(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<MenuItem> GetAll()
    {
        return UseDb(db => db.MenuItems
            .Include(x => x.Category)
            .AsNoTracking()
            .Where(x => x.Status != MenuItemStatus.Deleted
                && x.VisibilityStatus != VisibilityStatus.Hidden)
            .OrderBy(x => x.MenuItemId)
            .ToList());
    }

    public List<MenuItem> GetCustomerMenuItems()
    {
        return UseDb(db => db.MenuItems
            .Include(x => x.Category)
            .AsNoTracking()
            .Where(x => x.Status == MenuItemStatus.Active
                && x.VisibilityStatus == VisibilityStatus.Visible
                && x.CanOrderStandalone
                && x.Category != null
                && x.Category.IsActive)
            .OrderBy(x => x.Category!.DisplayOrder)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToList());
    }

    public MenuItem? GetById(int id)
    {
        return UseDb(db => db.MenuItems
            .Include(x => x.Category)
            .AsNoTracking()
            .FirstOrDefault(x => x.MenuItemId == id));
    }

    public List<MenuItem> GetByIdsForUpdate(IEnumerable<int> ids)
    {
        var menuItemIds = ids.Distinct().ToList();
        return UseDb(db => db.MenuItems
            .Include(x => x.Category)
            .Where(x => menuItemIds.Contains(x.MenuItemId))
            .ToList());
    }

    public bool ExistsByName(string itemName, int? excludedMenuItemId = null)
    {
        return UseDb(db => db.MenuItems.Any(x =>
            x.Name == itemName &&
            (!excludedMenuItemId.HasValue || x.MenuItemId != excludedMenuItemId.Value)));
    }

    public bool ExistsByCode(string itemCode, int? excludedMenuItemId = null)
    {
        return UseDb(db => db.MenuItems.Any(x =>
            x.ItemCode == itemCode
            && (!excludedMenuItemId.HasValue || x.MenuItemId != excludedMenuItemId.Value)));
    }

    public List<MenuItem> Search(string keyword)
    {
        var normalizedKeyword = keyword.Trim();
        return UseDb(db => db.MenuItems
            .Include(x => x.Category)
            .AsNoTracking()
            .Where(x => x.Name.Contains(normalizedKeyword)
                || (x.Category != null && x.Category.CategoryName.Contains(normalizedKeyword))
                || x.ItemCode == normalizedKeyword.ToUpper())
            .OrderBy(x => x.Name)
            .ToList());
    }

    public MenuItem Add(MenuItem item)
    {
        return UseDb(db =>
        {
            db.MenuItems.Add(item);
            db.SaveChanges();
            return item;
        });
    }

    public void Delete(int menuItemId)
    {
        UseDb(db =>
        {
            var strategy = db.Database.CreateExecutionStrategy();
            strategy.Execute(() =>
            {
                using var transaction = db.Database.BeginTransaction();
                var itemExists = db.MenuItems.Any(x => x.MenuItemId == menuItemId);
                if (!itemExists)
                {
                    throw new BusinessException("Món không tồn tại hoặc đã được xóa.");
                }

                var hasOrderHistory = db.OrderItems.Any(x => x.MenuItemId == menuItemId)
                    || db.BillDetails.Any(x => x.MenuItemId == menuItemId);
                if (hasOrderHistory)
                {
                    throw new BusinessException("Không thể xóa vĩnh viễn món đã phát sinh đơn hàng hoặc hóa đơn.");
                }

                foreach (var choice in db.ChoiceItems.Where(x => x.LinkedMenuItemId == menuItemId))
                {
                    choice.LinkedMenuItemId = null;
                }

                db.SaveChanges();
                db.MenuItems.Where(x => x.MenuItemId == menuItemId).ExecuteDelete();

                transaction.Commit();
            });
        });
    }

    public void Update(MenuItem item)
    {
        UseDb(db =>
        {
            var entry = db.Entry(item);
            if (entry.State == EntityState.Detached)
            {
                db.MenuItems.Attach(item);
                entry = db.Entry(item);
            }

            entry.State = EntityState.Modified;
            db.SaveChanges();
        });
    }

    public void UpdateMany(IEnumerable<MenuItem> items)
    {
        var itemList = items.ToList();
        if (itemList.Count == 0)
        {
            return;
        }

        UseDb(db =>
        {
            using var transaction = db.Database.BeginTransaction();
            foreach (var item in itemList)
            {
                var entry = db.Entry(item);
                if (entry.State == EntityState.Detached)
                {
                    db.MenuItems.Attach(item);
                    entry = db.Entry(item);
                }

                entry.State = EntityState.Modified;
            }

            db.SaveChanges();
            transaction.Commit();
        });
    }

    public void MutateLockedItems(IEnumerable<int> ids, Action<List<MenuItem>> mutation)
    {
        var menuItemIds = ids.Distinct().OrderBy(x => x).ToList();
        if (menuItemIds.Count == 0)
        {
            return;
        }

        UseDb(db =>
        {
            using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
            LockMenuItemRows(db, menuItemIds);

            var items = db.MenuItems
                .Include(x => x.Category)
                .Where(x => menuItemIds.Contains(x.MenuItemId))
                .ToList();

            mutation(items);
            db.SaveChanges();
            transaction.Commit();
        });
    }

    public void SaveChanges()
    {
        UseDb(db => db.SaveChanges());
    }

    private TResult UseDb<TResult>(Func<AppDbContext, TResult> action)
    {
        if (_dbContext is not null)
        {
            return action(_dbContext);
        }

        using var db = new AppDbContext();
        return action(db);
    }

    private void UseDb(Action<AppDbContext> action)
    {
        if (_dbContext is not null)
        {
            action(_dbContext);
            return;
        }

        using var db = new AppDbContext();
        action(db);
    }

    private static void LockMenuItemRows(AppDbContext db, List<int> menuItemIds)
    {
        var parameterNames = menuItemIds.Select((_, index) => $"@p{index}").ToList();
        var parameters = menuItemIds
            .Select((id, index) => new NpgsqlParameter<int>($"p{index}", id))
            .Cast<object>()
            .ToArray();
        var sql = "SELECT \"MenuItemId\" AS \"Value\" FROM \"MenuItems\" WHERE \"MenuItemId\" IN ("
            + string.Join(", ", parameterNames)
            + ") FOR UPDATE";

        _ = db.Database
            .SqlQueryRaw<int>(sql, parameters)
            .ToList();
    }
}
