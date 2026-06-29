using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Services.Menu.Validation;

internal static class CategoryValidator
{
    public static void ValidateForSave(Category category)
    {
        category.CategoryName = category.CategoryName.Trim();

        if (string.IsNullOrWhiteSpace(category.CategoryName))
        {
            throw new BusinessException("Ten category khong duoc de trong.");
        }

        if (category.CategoryName.Length > 100)
        {
            throw new BusinessException("Ten category khong duoc vuot qua 100 ky tu.");
        }

        if (category.Description?.Length > 500)
        {
            throw new BusinessException("Mo ta category khong duoc vuot qua 500 ky tu.");
        }

        if (category.DisplayOrder < 0)
        {
            throw new BusinessException("DisplayOrder khong duoc am.");
        }
    }
}
