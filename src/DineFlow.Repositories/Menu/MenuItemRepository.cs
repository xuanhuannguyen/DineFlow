using DineFlow.BusinessObjects.Menu;
using DineFlow.DataAccessObjects.DbContexts;
using DineFlow.DataAccessObjects.Menu;

namespace DineFlow.Repositories.Menu;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly MenuItemDAO _menuItemDAO;

    public MenuItemRepository() : this(new MenuItemDAO())
    {
    }

    public MenuItemRepository(AppDbContext dbContext) : this(new MenuItemDAO(dbContext))
    {
    }

    private MenuItemRepository(MenuItemDAO menuItemDAO)
    {
        _menuItemDAO = menuItemDAO;
    }

    public List<MenuItem> GetAll() => _menuItemDAO.GetAll();
    public List<MenuItem> GetCustomerMenuItems() => _menuItemDAO.GetCustomerMenuItems();
    public MenuItem? GetById(int id) => _menuItemDAO.GetById(id);
    public List<MenuItem> GetByIdsForUpdate(IEnumerable<int> ids) => _menuItemDAO.GetByIdsForUpdate(ids);
    public bool ExistsByName(string itemName, int? excludedMenuItemId = null) => _menuItemDAO.ExistsByName(itemName, excludedMenuItemId);
    public bool ExistsByCode(string itemCode, int? excludedMenuItemId = null) => _menuItemDAO.ExistsByCode(itemCode, excludedMenuItemId);
    public List<MenuItem> Search(string keyword) => _menuItemDAO.Search(keyword);
    public MenuItem Add(MenuItem item) => _menuItemDAO.Add(item);
    public void Delete(int menuItemId) => _menuItemDAO.Delete(menuItemId);
    public void Update(MenuItem item) => _menuItemDAO.Update(item);
    public void UpdateMany(IEnumerable<MenuItem> items) => _menuItemDAO.UpdateMany(items);
    public void MutateLockedItems(IEnumerable<int> ids, Action<List<MenuItem>> mutation) => _menuItemDAO.MutateLockedItems(ids, mutation);
    public void SaveChanges() => _menuItemDAO.SaveChanges();
}
