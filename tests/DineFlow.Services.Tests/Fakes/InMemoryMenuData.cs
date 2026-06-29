using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Services.Tests.Fakes;

public sealed class InMemoryMenuData
{
    public List<Category> Categories { get; } = new();
    public List<MenuItem> Items { get; } = new();
    public List<MenuAddonGroup> AddonGroups { get; } = new();
    public List<MenuAddonOption> AddonOptions { get; } = new();
    public List<MenuItemAddonGroup> ItemAddonMappings { get; } = new();
    public List<AddonGroupOption> GroupOptions { get; } = new();

    public Category ActiveCategory => Categories.First(x => x.CategoryName == "Mon chinh");
    public Category InactiveCategory => Categories.First(x => x.CategoryName == "Danh muc an");
    public MenuItem ComGa => Items.First(x => x.ItemName == "Com ga xoi mo");
    public MenuItem TrungOpLa => Items.First(x => x.ItemName == "Trung op la");
    public MenuItem InactiveItem => Items.First(x => x.ItemName == "Combo trua");
    public MenuItem UnavailableItem => Items.First(x => x.ItemName == "Pho bo dac biet");
    public MenuItem HiddenCategoryItem => Items.First(x => x.ItemName == "Mon trong danh muc an");
    public MenuItem NoStockItem => Items.First(x => x.ItemName == "Tra dao cam sa");
    public AddonGroupOption LinkedEggOption => GroupOptions.First(x => x.MenuAddonOption!.OptionName == "Trung op la");
    public AddonGroupOption ExtraSauceOption => GroupOptions.First(x => x.MenuAddonOption!.OptionName == "Nuoc cham them");

    public static InMemoryMenuData CreateDefault()
    {
        var data = new InMemoryMenuData();

        var activeCategory = new Category
        {
            CategoryId = 1,
            CategoryName = "Mon chinh",
            DisplayOrder = 1,
            IsActive = true
        };
        var drinkCategory = new Category
        {
            CategoryId = 2,
            CategoryName = "Do uong",
            DisplayOrder = 2,
            IsActive = true
        };
        var inactiveCategory = new Category
        {
            CategoryId = 99,
            CategoryName = "Danh muc an",
            DisplayOrder = 99,
            IsActive = false
        };
        data.Categories.AddRange([activeCategory, drinkCategory, inactiveCategory]);

        var comGa = new MenuItem
        {
            MenuItemId = 1,
            ItemCode = "M1",
            CategoryId = activeCategory.CategoryId,
            Category = activeCategory,
            ItemName = "Com ga xoi mo",
            Price = 59000,
            IsActive = true,
            IsAvailable = true,
            CanOrderStandalone = true,
            TrackStock = true,
            AvailableQuantity = 12
        };
        var trungOpLa = new MenuItem
        {
            MenuItemId = 2,
            ItemCode = "M2",
            CategoryId = activeCategory.CategoryId,
            Category = activeCategory,
            ItemName = "Trung op la",
            Price = 12000,
            IsActive = true,
            IsAvailable = true,
            CanOrderStandalone = false,
            TrackStock = true,
            AvailableQuantity = 30
        };
        var inactiveItem = new MenuItem
        {
            MenuItemId = 3,
            ItemCode = "M3",
            CategoryId = activeCategory.CategoryId,
            Category = activeCategory,
            ItemName = "Combo trua",
            Price = 99000,
            IsActive = false,
            IsAvailable = false,
            CanOrderStandalone = true,
            TrackStock = true,
            AvailableQuantity = 5
        };
        var unavailableItem = new MenuItem
        {
            MenuItemId = 4,
            ItemCode = "M4",
            CategoryId = activeCategory.CategoryId,
            Category = activeCategory,
            ItemName = "Pho bo dac biet",
            Price = 65000,
            IsActive = true,
            IsAvailable = false,
            CanOrderStandalone = true,
            TrackStock = true,
            AvailableQuantity = 0
        };
        var hiddenCategoryItem = new MenuItem
        {
            MenuItemId = 5,
            ItemCode = "M5",
            CategoryId = inactiveCategory.CategoryId,
            Category = inactiveCategory,
            ItemName = "Mon trong danh muc an",
            Price = 45000,
            IsActive = true,
            IsAvailable = true,
            CanOrderStandalone = true,
            TrackStock = true,
            AvailableQuantity = 8
        };
        var noStockItem = new MenuItem
        {
            MenuItemId = 6,
            ItemCode = "M6",
            CategoryId = drinkCategory.CategoryId,
            Category = drinkCategory,
            ItemName = "Tra dao cam sa",
            Price = 29000,
            IsActive = true,
            IsAvailable = true,
            CanOrderStandalone = true,
            TrackStock = false,
            AvailableQuantity = null
        };
        data.Items.AddRange([comGa, trungOpLa, inactiveItem, unavailableItem, hiddenCategoryItem, noStockItem]);

        var addonGroup = new MenuAddonGroup
        {
            MenuAddonGroupId = 1,
            GroupName = "Mon phu them",
            DisplayOrder = 1,
            IsActive = true
        };
        data.AddonGroups.Add(addonGroup);

        var linkedEggOption = new MenuAddonOption
        {
            MenuAddonOptionId = 1,
            OptionName = "Trung op la",
            LinkedMenuItemId = trungOpLa.MenuItemId,
            LinkedMenuItem = trungOpLa,
            IsActive = true
        };
        var extraSauceOption = new MenuAddonOption
        {
            MenuAddonOptionId = 2,
            OptionName = "Nuoc cham them",
            LinkedMenuItemId = null,
            LinkedMenuItem = null,
            IsActive = true
        };
        data.AddonOptions.AddRange([linkedEggOption, extraSauceOption]);

        var linkedGroupOption = new AddonGroupOption
        {
            AddonGroupOptionId = 1,
            MenuAddonGroupId = addonGroup.MenuAddonGroupId,
            MenuAddonGroup = addonGroup,
            MenuAddonOptionId = linkedEggOption.MenuAddonOptionId,
            MenuAddonOption = linkedEggOption,
            ExtraPrice = 10000,
            AllowMultiple = false,
            IsActive = true,
            DisplayOrder = 1
        };
        var sauceGroupOption = new AddonGroupOption
        {
            AddonGroupOptionId = 2,
            MenuAddonGroupId = addonGroup.MenuAddonGroupId,
            MenuAddonGroup = addonGroup,
            MenuAddonOptionId = extraSauceOption.MenuAddonOptionId,
            MenuAddonOption = extraSauceOption,
            ExtraPrice = 5000,
            AllowMultiple = false,
            IsActive = true,
            DisplayOrder = 2
        };
        addonGroup.Options = [linkedGroupOption, sauceGroupOption];
        data.GroupOptions.AddRange([linkedGroupOption, sauceGroupOption]);

        var mapping = new MenuItemAddonGroup
        {
            MenuItemAddonGroupId = 1,
            MenuItemId = comGa.MenuItemId,
            MenuItem = comGa,
            MenuAddonGroupId = addonGroup.MenuAddonGroupId,
            MenuAddonGroup = addonGroup,
            IsRequired = false,
            MinSelect = 0,
            MaxSelect = 2,
            DisplayOrder = 1,
            IsActive = true
        };
        data.ItemAddonMappings.Add(mapping);

        return data;
    }

    public MenuItemAddonGroup CloneMappingWithRules(
        bool isRequired,
        int minSelect,
        int maxSelect,
        Action<MenuItemAddonGroup>? configure = null)
    {
        var source = ItemAddonMappings.First(x => x.MenuItemId == ComGa.MenuItemId);
        var clone = new MenuItemAddonGroup
        {
            MenuItemAddonGroupId = source.MenuItemAddonGroupId + 100,
            MenuItemId = source.MenuItemId,
            MenuItem = source.MenuItem,
            MenuAddonGroupId = source.MenuAddonGroupId,
            MenuAddonGroup = source.MenuAddonGroup,
            IsRequired = isRequired,
            MinSelect = minSelect,
            MaxSelect = maxSelect,
            DisplayOrder = source.DisplayOrder,
            IsActive = true
        };
        configure?.Invoke(clone);
        ItemAddonMappings.RemoveAll(x => x.MenuItemId == ComGa.MenuItemId);
        ItemAddonMappings.Add(clone);
        return clone;
    }
}
