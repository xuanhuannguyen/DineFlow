using DineFlow.BusinessObjects.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace DineFlow.BusinessObjects.Menu;

public class MenuItem : BaseEntity
{
    private const int OperationalTextMaxLength = 500;

    public int MenuItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public int RestaurantId { get; set; } = 1;
    public int CategoryId { get; set; }
    public int? KitchenStationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public string? Currency { get; set; } = "VND";
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public bool CanOrderStandalone { get; set; } = true;
    public MenuItemType ItemType { get; set; } = MenuItemType.Single;
    public MenuItemStatus Status { get; set; } = MenuItemStatus.Active;
    public VisibilityStatus VisibilityStatus { get; set; } = VisibilityStatus.Visible;
    public string? HiddenReason { get; set; }
    public DateTime? HiddenAt { get; set; }
    public AvailabilityStatus AvailabilityStatus { get; set; } = AvailabilityStatus.Available;
    public string? UnavailableReason { get; set; }
    public bool TrackStock { get; set; }
    public int? AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int? LowStockThreshold { get; set; }
    public string? SoldOutReason { get; set; }
    public DateTime? SoldOutAt { get; set; }
    public int? PreparationTimeMinutes { get; set; }
    public int? SpicyLevel { get; set; }
    public int? Calories { get; set; }
    public string? AllergenNote { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public string? StaffNote { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Category? Category { get; set; }
    public KitchenStation? KitchenStation { get; set; }
    public ICollection<MenuItemChoiceGroup> ChoiceGroups { get; set; } = new List<MenuItemChoiceGroup>();
    public ICollection<ChoiceItem> UsedAsLinkedChoiceItems { get; set; } = new List<ChoiceItem>();
    public ICollection<MenuItemChannelPrice> ChannelPrices { get; set; } = new List<MenuItemChannelPrice>();
    [NotMapped] public ICollection<MenuItemAddonGroup> AddonGroups { get; set; } = new List<MenuItemAddonGroup>();
    [NotMapped] public ICollection<MenuAddonOption> UsedAsLinkedAddonOptions { get; set; } = new List<MenuAddonOption>();
    public ICollection<MenuItemImage> Images { get; set; } = new List<MenuItemImage>();
    [NotMapped] public ICollection<MenuItemVariant> Variants { get; set; } = new List<MenuItemVariant>();
    public ICollection<MenuItemAvailabilitySchedule> AvailabilitySchedules { get; set; } = new List<MenuItemAvailabilitySchedule>();
    public ICollection<MenuItemPriceHistory> PriceHistories { get; set; } = new List<MenuItemPriceHistory>();
    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    public ICollection<MenuAuditLog> AuditLogs { get; set; } = new List<MenuAuditLog>();
    public ICollection<ComboGroup> ComboGroups { get; set; } = new List<ComboGroup>();
    public ICollection<ComboGroupItem> UsedInComboGroups { get; set; } = new List<ComboGroupItem>();

    [NotMapped]
    public string ItemName
    {
        get => Name;
        set => Name = value;
    }

    [NotMapped]
    public decimal Price
    {
        get => BasePrice;
        set => BasePrice = value;
    }

    public void MarkHidden()
    {
        VisibilityStatus = VisibilityStatus.Hidden;
        HiddenAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        Status = MenuItemStatus.Deleted;
        IsActive = false;
        IsAvailable = false;
        AvailabilityStatus = AvailabilityStatus.TemporarilyUnavailable;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStockQuantity(int? availableQuantity, string? staffNote = null)
    {
        if (!TrackStock)
        {
            throw new BusinessException("Chi mon co quan ly stock moi duoc cap nhat ton kho.");
        }

        if (availableQuantity is null or < 0)
        {
            throw new BusinessException(MenuBusinessMessages.TrackedMenuItemRequiresStock);
        }

        AvailableQuantity = availableQuantity;
        SetStaffNote(staffNote);
        ApplyStockAvailabilityRule();
        if (TrackStock && AvailableQuantity > 0 && AvailabilityStatus == AvailabilityStatus.SoldOut)
        {
            IsAvailable = true;
            AvailabilityStatus = AvailabilityStatus.Available;
            SoldOutAt = null;
            SoldOutReason = null;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSaleAvailability(bool isAvailable, string? soldOutReason = null, string? staffNote = null)
    {
        if (isAvailable && TrackStock && (AvailableQuantity ?? 0) <= 0)
        {
            throw new BusinessException(MenuBusinessMessages.CannotEnableAvailabilityWithoutStock);
        }

        if (isAvailable && !IsActive)
        {
            throw new BusinessException(MenuBusinessMessages.CannotEnableInactiveMenuItem);
        }

        if (isAvailable)
        {
            IsAvailable = true;
            AvailabilityStatus = AvailabilityStatus.Available;
            SoldOutAt = null;
            SoldOutReason = null;
        }
        else if (TrackStock)
        {
            AvailableQuantity = 0;
            IsAvailable = false;
            AvailabilityStatus = AvailabilityStatus.SoldOut;
            SoldOutAt = DateTime.UtcNow;
            SoldOutReason = NormalizeOperationalText(soldOutReason) ?? "Ban het/het hang";
        }
        else
        {
            IsAvailable = false;
            AvailabilityStatus = AvailabilityStatus.TemporarilyUnavailable;
            SoldOutReason = NormalizeOperationalText(soldOutReason);
        }
        SetStaffNote(staffNote);
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnsureCanBeOrdered(int quantity, bool requireStandalone)
    {
        var activeInMenu = Status == MenuItemStatus.Active
            && IsActive
            && VisibilityStatus == VisibilityStatus.Visible
            && Category is { IsActive: true };

        if (!activeInMenu || !IsAvailable || AvailabilityStatus != AvailabilityStatus.Available)
        {
            throw new BusinessException(string.Format(MenuBusinessMessages.MenuItemNotOrderableFormat, Name));
        }

        if (requireStandalone && !CanOrderStandalone)
        {
            throw new BusinessException(string.Format(MenuBusinessMessages.MenuItemNotStandaloneFormat, Name));
        }

        if (TrackStock && (AvailableQuantity ?? 0) < quantity)
        {
            throw new BusinessException(string.Format(MenuBusinessMessages.MenuItemInsufficientStockFormat, Name));
        }
    }

    public void ReserveStock(int quantity)
    {
        if (!TrackStock)
        {
            return;
        }

        AvailableQuantity -= quantity;
        ApplyStockAvailabilityRule();
        UpdatedAt = DateTime.UtcNow;
        RefreshRowVersion();
    }

    public void RestoreStock(int quantity)
    {
        if (!TrackStock)
        {
            return;
        }

        AvailableQuantity = (AvailableQuantity ?? 0) + quantity;
        if (AvailabilityStatus == AvailabilityStatus.SoldOut && AvailableQuantity > 0)
        {
            IsAvailable = true;
            AvailabilityStatus = AvailabilityStatus.Available;
            SoldOutAt = null;
            SoldOutReason = null;
        }
        UpdatedAt = DateTime.UtcNow;
        RefreshRowVersion();
    }

    public void ApplyStockAvailabilityRule()
    {
        if (TrackStock && (AvailableQuantity ?? 0) <= 0)
        {
            AvailableQuantity = 0;
            IsAvailable = false;
            AvailabilityStatus = AvailabilityStatus.SoldOut;
            SoldOutAt ??= DateTime.UtcNow;
            SoldOutReason ??= "Ban het/het hang";
        }
    }

    public bool CanShowToCustomer()
    {
        return Status == MenuItemStatus.Active
            && VisibilityStatus == VisibilityStatus.Visible
            && IsActive
            && CanOrderStandalone
            && Category is { IsActive: true };
    }

    public void RefreshRowVersion()
    {
        RowVersion = Guid.NewGuid().ToByteArray();
    }

    private void SetStaffNote(string? staffNote)
    {
        if (staffNote is not null)
        {
            StaffNote = NormalizeOperationalText(staffNote);
        }
    }

    private static string? NormalizeOperationalText(string? value)
    {
        var normalized = value?.Trim();
        if (normalized?.Length > OperationalTextMaxLength)
        {
            throw new BusinessException("Ghi chu van hanh khong duoc vuot qua 500 ky tu.");
        }

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
