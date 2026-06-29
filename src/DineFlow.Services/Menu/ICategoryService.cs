using DineFlow.BusinessObjects.Menu;
using DineFlow.BusinessObjects.Common;

namespace DineFlow.Services.Menu;

public interface ICategoryService
{
    List<Category> GetAll();
    List<Category> GetActiveCategories();
    Category? GetById(int categoryId);
    Category Create(Category category);
    Category Create(Category category, UserRole role);
    void Update(Category category);
    void Update(Category category, UserRole role);
    void SoftDelete(int categoryId, UserRole role);
}
