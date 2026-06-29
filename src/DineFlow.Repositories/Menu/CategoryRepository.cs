using DineFlow.BusinessObjects.Menu;
using DineFlow.DataAccessObjects.DbContexts;
using DineFlow.DataAccessObjects.Menu;

namespace DineFlow.Repositories.Menu;

public class CategoryRepository : ICategoryRepository
{
    private readonly CategoryDAO _categoryDAO;

    public CategoryRepository() : this(new CategoryDAO())
    {
    }

    public CategoryRepository(AppDbContext dbContext) : this(new CategoryDAO(dbContext))
    {
    }

    private CategoryRepository(CategoryDAO categoryDAO)
    {
        _categoryDAO = categoryDAO;
    }

    public List<Category> GetAll() => _categoryDAO.GetAll();
    public List<Category> GetActive() => _categoryDAO.GetActive();
    public Category? GetById(int id) => _categoryDAO.GetById(id);
    public bool ExistsByName(string categoryName, int? excludedCategoryId = null) => _categoryDAO.ExistsByName(categoryName, excludedCategoryId);
    public Category Add(Category category) => _categoryDAO.Add(category);
    public void Update(Category category) => _categoryDAO.Update(category);
}
