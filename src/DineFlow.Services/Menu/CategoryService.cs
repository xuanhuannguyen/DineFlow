using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;
using DineFlow.Repositories.Menu;
using DineFlow.Services.Menu.Validation;

namespace DineFlow.Services.Menu;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService() : this(new CategoryRepository())
    {
    }

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public List<Category> GetAll() => _categoryRepository.GetAll();
    public List<Category> GetActiveCategories() => _categoryRepository.GetActive();
    public Category? GetById(int categoryId) => _categoryRepository.GetById(categoryId);

    public Category Create(Category category) => Create(category, UserRole.Admin);

    public Category Create(Category category, UserRole role)
    {
        EnsureAdmin(role);
        CategoryValidator.ValidateForSave(category);
        EnsureUniqueName(category.CategoryName);
        category.IsActive = true;
        return _categoryRepository.Add(category);
    }

    public void Update(Category category) => Update(category, UserRole.Admin);

    public void Update(Category category, UserRole role)
    {
        EnsureAdmin(role);
        CategoryValidator.ValidateForSave(category);
        EnsureUniqueName(category.CategoryName, category.CategoryId);
        category.UpdatedAt = DateTime.UtcNow;
        _categoryRepository.Update(category);
    }

    public void SoftDelete(int categoryId, UserRole role)
    {
        EnsureAdmin(role);
        var category = _categoryRepository.GetById(categoryId)
            ?? throw new BusinessException("Category khong ton tai.");

        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;
        _categoryRepository.Update(category);
    }

    private void EnsureUniqueName(string categoryName, int? excludedCategoryId = null)
    {
        if (_categoryRepository.ExistsByName(categoryName.Trim(), excludedCategoryId))
        {
            throw new BusinessException("Ten category da ton tai.");
        }
    }

    private static void EnsureAdmin(UserRole role)
    {
        if (role != UserRole.Admin)
        {
            throw new BusinessException("Chi Admin duoc phep quan ly category.");
        }
    }
}
