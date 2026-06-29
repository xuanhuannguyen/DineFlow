using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Repositories.Menu;

public interface ICategoryRepository
{
    List<Category> GetAll();
    List<Category> GetActive();
    Category? GetById(int id);
    bool ExistsByName(string categoryName, int? excludedCategoryId = null);
    Category Add(Category category);
    void Update(Category category);
}
