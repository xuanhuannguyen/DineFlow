using DineFlow.BusinessObjects.Menu;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Menu;

public class CategoryDAO
{
    private readonly AppDbContext? _dbContext;

    public CategoryDAO()
    {
    }

    public CategoryDAO(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<Category> GetAll()
    {
        return UseDb(db => db.Categories.AsNoTracking().OrderBy(x => x.DisplayOrder).ToList());
    }

    public List<Category> GetActive()
    {
        return UseDb(db => db.Categories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToList());
    }

    public Category? GetById(int id)
    {
        return UseDb(db => db.Categories.FirstOrDefault(x => x.CategoryId == id));
    }

    public bool ExistsByName(string categoryName, int? excludedCategoryId = null)
    {
        return UseDb(db => db.Categories.Any(x =>
            x.CategoryName == categoryName &&
            (!excludedCategoryId.HasValue || x.CategoryId != excludedCategoryId.Value)));
    }

    public Category Add(Category category)
    {
        return UseDb(db =>
        {
            db.Categories.Add(category);
            db.SaveChanges();
            return category;
        });
    }

    public void Update(Category category)
    {
        UseDb(db =>
        {
            db.Categories.Update(category);
            db.SaveChanges();
        });
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
}
