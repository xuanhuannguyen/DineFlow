using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;
using DineFlow.Services.Menu;
using DineFlow.Services.Tests.Fakes;
using Xunit;

namespace DineFlow.Services.Tests;

public class CriticalBusinessRulesTests
{
    private static OrderItemRequestDto OrderComGa(
        int quantity = 1,
        params OrderAddonRequestDto[] addons)
    {
        var factory = new MenuServiceFactory();
        return new OrderItemRequestDto
        {
            MenuItemId = factory.Data.ComGa.MenuItemId,
            Quantity = quantity,
            Addons = addons.ToList()
        };
    }

    private static OrderAddonRequestDto Addon(int addonGroupOptionId, int quantity = 1)
    {
        return new OrderAddonRequestDto
        {
            AddonGroupOptionId = addonGroupOptionId,
            Quantity = quantity
        };
    }

    [Fact(DisplayName = "1. Category inactive thi mon khong hien tren Customer Web")]
    public void InactiveCategory_HidesItemsFromCustomerMenu()
    {
        var factory = new MenuServiceFactory();

        var menu = factory.CustomerMenuService.GetCustomerMenu();

        Assert.DoesNotContain(menu.Categories, x => x.CategoryName == "Danh muc an");
        Assert.DoesNotContain(menu.Items, x => x.ItemName == "Mon trong danh muc an");
        Assert.Contains(menu.Items, x => x.ItemName == "Com ga xoi mo");
    }

    [Fact(DisplayName = "2. MenuItem inactive thi khong order duoc")]
    public void InactiveMenuItem_CannotBeOrdered()
    {
        var factory = new MenuServiceFactory();
        var item = factory.Data.InactiveItem;

        var ex = Assert.Throws<BusinessException>(() =>
            factory.MenuItemService.ValidateOrderableItems([
                new OrderItemRequestDto { MenuItemId = item.MenuItemId, Quantity = 1 }
            ]));

        Assert.Contains("hien khong the dat", ex.Message);
    }

    [Fact(DisplayName = "3. MenuItem unavailable thi khong order duoc")]
    public void UnavailableMenuItem_CannotBeOrdered()
    {
        var factory = new MenuServiceFactory();
        var item = factory.Data.UnavailableItem;

        var ex = Assert.Throws<BusinessException>(() =>
            factory.MenuItemService.ValidateOrderableItems([
                new OrderItemRequestDto { MenuItemId = item.MenuItemId, Quantity = 1 }
            ]));

        Assert.Contains("hien khong the dat", ex.Message);
    }

    [Fact(DisplayName = "4. CanOrderStandalone = false thi khong hien o menu chinh")]
    public void AddonOnlyItem_NotShownInCustomerMenu()
    {
        var factory = new MenuServiceFactory();

        var menuItems = factory.MenuItemService.GetCustomerMenuItems();

        Assert.DoesNotContain(menuItems, x => x.ItemName == "Trung op la");
        Assert.Throws<BusinessException>(() =>
            factory.CustomerMenuService.GetMenuItemDetail(factory.Data.TrungOpLa.MenuItemId));
    }

    [Fact(DisplayName = "5. Addon option khong thuoc group cua mon thi reject")]
    public void AddonNotMappedToItem_IsRejected()
    {
        var factory = new MenuServiceFactory();
        var foreignOptionId = 9999;

        var ex = Assert.Throws<BusinessException>(() =>
            factory.MenuItemService.ValidateAddonsForOrder([
                OrderComGa(1, Addon(foreignOptionId))
            ]));

        Assert.Equal("Mon phu khong thuoc mon chinh da chon.", ex.Message);
    }

    [Fact(DisplayName = "6. Group required nhung khong chon option thi reject")]
    public void RequiredAddonGroupWithoutSelection_IsRejected()
    {
        var factory = new MenuServiceFactory();
        factory.Data.CloneMappingWithRules(isRequired: true, minSelect: 1, maxSelect: 1);

        var ex = Assert.Throws<BusinessException>(() =>
            factory.MenuItemService.ValidateAddonsForOrder([
                OrderComGa(1)
            ]));

        Assert.Contains("la bat buoc", ex.Message);
    }

    [Fact(DisplayName = "7. Chon qua MaxSelect thi reject")]
    public void ExceedingMaxSelect_IsRejected()
    {
        var factory = new MenuServiceFactory();
        var egg = factory.Data.LinkedEggOption;
        var sauce = factory.Data.ExtraSauceOption;
        factory.Data.CloneMappingWithRules(isRequired: false, minSelect: 0, maxSelect: 1);

        var ex = Assert.Throws<BusinessException>(() =>
            factory.MenuItemService.ValidateAddonsForOrder([
                OrderComGa(1, Addon(egg.AddonGroupOptionId), Addon(sauce.AddonGroupOptionId))
            ]));

        Assert.Contains("vuot so luong toi da", ex.Message);
    }

    [Fact(DisplayName = "8. Option co LinkedMenuItemId het stock thi reject")]
    public void LinkedAddonOutOfStock_IsRejected()
    {
        var factory = new MenuServiceFactory();
        factory.Data.TrungOpLa.AvailableQuantity = 0;
        factory.Data.TrungOpLa.IsAvailable = false;

        var ex = Assert.Throws<BusinessException>(() =>
            factory.MenuItemService.ValidateAddonsForOrder([
                OrderComGa(1, Addon(factory.Data.LinkedEggOption.AddonGroupOptionId))
            ]));

        Assert.Contains("khong du so luong", ex.Message);
    }

    [Fact(DisplayName = "9. Option khong co LinkedMenuItemId thi khong tru stock")]
    public void AddonWithoutLinkedMenuItem_DoesNotDeductStock()
    {
        var factory = new MenuServiceFactory();
        var comGa = factory.Data.ComGa;
        var trung = factory.Data.TrungOpLa;
        var beforeComGa = comGa.AvailableQuantity;
        var beforeTrung = trung.AvailableQuantity;

        factory.MenuItemService.ReserveStockForOrder([
            OrderComGa(1, Addon(factory.Data.ExtraSauceOption.AddonGroupOptionId))
        ]);

        Assert.Equal(beforeComGa - 1, comGa.AvailableQuantity);
        Assert.Equal(beforeTrung, trung.AvailableQuantity);
    }

    [Fact(DisplayName = "10. Nhieu dong order cung addon thi normalize stock dung")]
    public void MultipleLinesWithSameAddon_NormalizesStockDeduction()
    {
        var factory = new MenuServiceFactory();
        var comGa = factory.Data.ComGa;
        var trung = factory.Data.TrungOpLa;
        var egg = factory.Data.LinkedEggOption;

        factory.MenuItemService.ReserveStockForOrder([
            OrderComGa(1, Addon(egg.AddonGroupOptionId)),
            OrderComGa(2, Addon(egg.AddonGroupOptionId))
        ]);

        Assert.Equal(9, comGa.AvailableQuantity);
        Assert.Equal(27, trung.AvailableQuantity);
    }

    [Fact(DisplayName = "11. Order thanh cong thi tru stock mon chinh va addon")]
    public void SuccessfulOrder_DeductsMainAndLinkedAddonStock()
    {
        var factory = new MenuServiceFactory();
        var comGa = factory.Data.ComGa;
        var trung = factory.Data.TrungOpLa;

        factory.MenuItemService.ReserveStockForOrder([
            OrderComGa(2, Addon(factory.Data.LinkedEggOption.AddonGroupOptionId, quantity: 1))
        ]);

        Assert.Equal(10, comGa.AvailableQuantity);
        Assert.Equal(28, trung.AvailableQuantity);
    }

    [Fact(DisplayName = "12. Cancel order thi rollback stock mon chinh va addon")]
    public void CancelOrder_RestoresMainAndLinkedAddonStock()
    {
        var factory = new MenuServiceFactory();
        var comGa = factory.Data.ComGa;
        var trung = factory.Data.TrungOpLa;
        var request = OrderComGa(2, Addon(factory.Data.LinkedEggOption.AddonGroupOptionId));

        factory.MenuItemService.ReserveStockForOrder([request]);
        factory.MenuItemService.RollbackStockForCancelledOrder([request]);

        Assert.Equal(12, comGa.AvailableQuantity);
        Assert.Equal(30, trung.AvailableQuantity);
    }

    [Fact(DisplayName = "13. Admin doi gia sau khi order thi bill cu khong doi")]
    public void PriceSnapshot_RemainsUnchangedAfterAdminPriceUpdate()
    {
        var factory = new MenuServiceFactory();
        var item = factory.Data.ComGa;
        var snapshotAtOrder = factory.MenuItemService.GetMenuItemSnapshot(item.MenuItemId);

        item.Price = 99000;
        factory.MenuItemService.Update(item, UserRole.Admin);

        Assert.Equal(59000, snapshotAtOrder.UnitPrice);
        Assert.Equal(99000, factory.MenuItemService.GetMenuItemSnapshot(item.MenuItemId).UnitPrice);
    }

    [Fact(DisplayName = "14. Required group khong con option available thi mon khong orderable")]
    public void RequiredGroupWithoutAvailableOptions_BlocksOrdering()
    {
        var factory = new MenuServiceFactory();
        factory.Data.CloneMappingWithRules(isRequired: true, minSelect: 1, maxSelect: 1);
        factory.Data.TrungOpLa.AvailableQuantity = 0;
        factory.Data.TrungOpLa.IsAvailable = false;

        var ex = Assert.Throws<BusinessException>(() =>
            factory.MenuItemService.ValidateOrderableItems([
                OrderComGa(1)
            ]));

        Assert.Contains("khong con du lua chon hop le", ex.Message);
    }

    [Fact(DisplayName = "15. Option mac dinh duoc tu dong ap vao order")]
    public void DefaultAddonOption_IsAppliedAutomatically()
    {
        var factory = new MenuServiceFactory();
        factory.Data.LinkedEggOption.IsDefault = true;
        var comGa = factory.Data.ComGa;
        var trung = factory.Data.TrungOpLa;

        factory.MenuItemService.ReserveStockForOrder([
            OrderComGa(1)
        ]);

        Assert.Equal(11, comGa.AvailableQuantity);
        Assert.Equal(29, trung.AvailableQuantity);
    }

    [Fact(DisplayName = "15b. Khach bo chon group thi khong tu add option mac dinh")]
    public void TouchedAddonGroup_SkipsDefaultAddon()
    {
        var factory = new MenuServiceFactory();
        factory.Data.LinkedEggOption.IsDefault = true;
        var comGa = factory.Data.ComGa;
        var trung = factory.Data.TrungOpLa;

        var request = OrderComGa(1);
        request.TouchedAddonGroupIds.Add(factory.Data.LinkedEggOption.MenuAddonGroupId);

        factory.MenuItemService.ReserveStockForOrder([request]);

        Assert.Equal(11, comGa.AvailableQuantity);
        Assert.Equal(30, trung.AvailableQuantity);
    }

    [Fact(DisplayName = "16. Moi group chi co mot option mac dinh")]
    public void MultipleDefaultOptionsInSameGroup_AreRejected()
    {
        var factory = new MenuServiceFactory();
        factory.Data.LinkedEggOption.IsDefault = true;

        var ex = Assert.Throws<BusinessException>(() =>
            factory.MenuAddonService.AddOptionToGroup(new AddonGroupOption
            {
                MenuAddonGroupId = factory.Data.ExtraSauceOption.MenuAddonGroupId,
                MenuAddonOptionId = factory.Data.ExtraSauceOption.MenuAddonOptionId,
                IsDefault = true,
                DisplayOrder = 3
            }, UserRole.Admin));

        Assert.Contains("mot lua chon mac dinh", ex.Message);
    }

    [Fact(DisplayName = "17. MaxSelect cua group phai >= 1")]
    public void AddonGroupMaxSelectZero_IsRejected()
    {
        var factory = new MenuServiceFactory();

        var ex = Assert.Throws<BusinessException>(() =>
            factory.MenuAddonService.AssignGroupToMenuItem(new MenuItemAddonGroup
            {
                MenuItemId = factory.Data.ComGa.MenuItemId,
                MenuAddonGroupId = factory.Data.LinkedEggOption.MenuAddonGroupId,
                MinSelect = 0,
                MaxSelect = 0,
                DisplayOrder = 1
            }, UserRole.Admin));

        Assert.Contains("MaxSelect phai >= 1", ex.Message);
    }

    [Fact(DisplayName = "18. Khong tao trung option link cung mot mon phu")]
    public void DuplicateLinkedAddonOption_IsRejected()
    {
        var factory = new MenuServiceFactory();

        var ex = Assert.Throws<BusinessException>(() =>
            factory.MenuAddonService.CreateOption(new MenuAddonOption
            {
                OptionName = "Trung op la 2",
                LinkedMenuItemId = factory.Data.TrungOpLa.MenuItemId
            }, UserRole.Admin));

        Assert.Contains("da duoc dung lam lua chon modifier", ex.Message);
    }

    [Fact(DisplayName = "19. Staff khong duoc sua gia/master data")]
    public void Staff_CannotModifyMasterData()
    {
        var factory = new MenuServiceFactory();
        var item = factory.Data.ComGa;
        var category = factory.Data.ActiveCategory;

        Assert.Throws<BusinessException>(() =>
            factory.MenuItemService.Update(item, UserRole.Staff));
        Assert.Throws<BusinessException>(() =>
            factory.MenuItemService.Create(new MenuItem
            {
                CategoryId = category.CategoryId,
                ItemCode = "NEW-STAFF",
                ItemName = "Mon moi",
                Price = 10000
            }, UserRole.Staff));
        Assert.Throws<BusinessException>(() =>
            factory.CategoryService.Create(new Category
            {
                CategoryName = "Loai moi",
                DisplayOrder = 1
            }, UserRole.Staff));
        Assert.Throws<BusinessException>(() =>
            factory.MenuAddonService.CreateGroup(new MenuAddonGroup
            {
                GroupName = "Addon moi",
                DisplayOrder = 1
            }, UserRole.Staff));

        Assert.Null(Record.Exception(() =>
            factory.MenuItemService.UpdateStock(item.MenuItemId, 20, UserRole.Staff)));
    }

    [Fact(DisplayName = "20. Staff duoc ghi ly do sold out va note noi bo")]
    public void Staff_CanRecordSoldOutReasonAndInternalNote()
    {
        var factory = new MenuServiceFactory();
        var item = factory.Data.ComGa;

        factory.MenuItemService.SetAvailability(
            item.MenuItemId,
            false,
            "Het ga",
            "Kiem lai sau ca trua",
            UserRole.Staff);

        Assert.False(item.IsAvailable);
        Assert.Equal("Het ga", item.SoldOutReason);
        Assert.Equal("Kiem lai sau ca trua", item.StaffNote);
    }

    [Fact(DisplayName = "21. Reopen xoa ly do sold out va cho cap nhat note")]
    public void Reopen_ClearsSoldOutReasonAndUpdatesStaffNote()
    {
        var factory = new MenuServiceFactory();
        var item = factory.Data.ComGa;
        item.SoldOutReason = "Het ga";

        factory.MenuItemService.SetAvailability(
            item.MenuItemId,
            true,
            soldOutReason: null,
            "Da nhap them ga",
            UserRole.Staff);

        Assert.True(item.IsAvailable);
        Assert.Null(item.SoldOutReason);
        Assert.Equal("Da nhap them ga", item.StaffNote);
    }

    [Fact(DisplayName = "22. Admin sua master data khong xoa note van hanh")]
    public void AdminMasterDataUpdate_PreservesOperationalStockNotes()
    {
        var factory = new MenuServiceFactory();
        var item = factory.Data.ComGa;
        item.SoldOutReason = "Tam ngung ban";
        item.StaffNote = "Cho bep xac nhan";

        item.Price = 65000;
        factory.MenuItemService.Update(item, UserRole.Admin);

        Assert.Equal("Tam ngung ban", item.SoldOutReason);
        Assert.Equal("Cho bep xac nhan", item.StaffNote);
    }

    [Fact(DisplayName = "23. Tao mon inactive thi khong bi normalize thanh active")]
    public void CreateInactiveItem_RemainsHiddenAndUnavailable()
    {
        var factory = new MenuServiceFactory();

        var item = factory.MenuItemService.Create(new MenuItem
        {
            CategoryId = factory.Data.ActiveCategory.CategoryId,
            ItemCode = "INACTIVE-01",
            ItemName = "Mon inactive moi",
            Price = 25000,
            IsActive = false,
            IsAvailable = true
        }, UserRole.Admin);

        Assert.False(item.IsActive);
        Assert.False(item.IsAvailable);
        Assert.Equal(MenuItemStatus.Inactive, item.Status);
        Assert.Equal(VisibilityStatus.Hidden, item.VisibilityStatus);
        Assert.Equal(AvailabilityStatus.TemporarilyUnavailable, item.AvailabilityStatus);
    }

    [Fact(DisplayName = "24. Mark sold out mon co stock thi dua quantity ve 0 va status SoldOut")]
    public void MarkSoldOut_TrackedItem_ZeroesStockAndSetsSoldOut()
    {
        var factory = new MenuServiceFactory();
        var item = factory.Data.ComGa;

        factory.MenuItemService.SetAvailability(item.MenuItemId, false, "Het ga", "Bep bao het", UserRole.Staff);

        Assert.False(item.IsAvailable);
        Assert.Equal(0, item.AvailableQuantity);
        Assert.Equal(AvailabilityStatus.SoldOut, item.AvailabilityStatus);
        Assert.Equal("Het ga", item.SoldOutReason);
        Assert.NotNull(item.SoldOutAt);
    }

    [Fact(DisplayName = "25. Nhap stock lai tu SoldOut thi tu mo ban lai")]
    public void RestockSoldOutItem_ReopensAvailability()
    {
        var factory = new MenuServiceFactory();
        var item = factory.Data.ComGa;
        item.AvailableQuantity = 0;
        item.IsAvailable = false;
        item.AvailabilityStatus = AvailabilityStatus.SoldOut;
        item.SoldOutReason = "Het ga";
        item.SoldOutAt = DateTime.UtcNow;

        factory.MenuItemService.UpdateStock(item.MenuItemId, 6, "Da nhap ga", UserRole.Staff);

        Assert.True(item.IsAvailable);
        Assert.Equal(6, item.AvailableQuantity);
        Assert.Equal(AvailabilityStatus.Available, item.AvailabilityStatus);
        Assert.Null(item.SoldOutReason);
        Assert.Null(item.SoldOutAt);
    }
}
