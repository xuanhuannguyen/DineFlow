using DineFlow.Services.Menu;

namespace DineFlow.Services.Tests.Fakes;

public sealed class MenuServiceFactory
{
    public InMemoryMenuData Data { get; }
    public MenuItemService MenuItemService { get; }
    public CustomerMenuService CustomerMenuService { get; }
    public CategoryService CategoryService { get; }
    public MenuAddonService MenuAddonService { get; }

    public MenuServiceFactory(InMemoryMenuData? data = null)
    {
        Data = data ?? InMemoryMenuData.CreateDefault();
        var itemRepository = new InMemoryMenuItemRepository(Data);
        var addonRepository = new InMemoryMenuAddonRepository(Data);
        var categoryRepository = new InMemoryCategoryRepository(Data);

        MenuItemService = new MenuItemService(itemRepository, addonRepository, categoryRepository);
        CategoryService = new CategoryService(categoryRepository);
        CustomerMenuService = new CustomerMenuService(CategoryService, MenuItemService);
        MenuAddonService = new MenuAddonService(addonRepository, itemRepository, MenuItemService);
    }
}
