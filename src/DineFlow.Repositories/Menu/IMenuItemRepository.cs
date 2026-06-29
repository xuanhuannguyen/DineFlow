using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Repositories.Menu;

public interface IMenuItemRepository
{
    List<MenuItem> GetAll();
    List<MenuItem> GetCustomerMenuItems();
    MenuItem? GetById(int id);
    List<MenuItem> GetByIdsForUpdate(IEnumerable<int> ids);
    bool ExistsByName(string itemName, int? excludedMenuItemId = null);
    bool ExistsByCode(string itemCode, int? excludedMenuItemId = null);
    List<MenuItem> Search(string keyword);
    MenuItem Add(MenuItem item);
    void Delete(int menuItemId);
    void Update(MenuItem item);
    void UpdateMany(IEnumerable<MenuItem> items);
    void MutateLockedItems(IEnumerable<int> ids, Action<List<MenuItem>> mutation);
    void SaveChanges();
}
