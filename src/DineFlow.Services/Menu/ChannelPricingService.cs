using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.Services.Menu;

public class ChannelPricingService : IChannelPricingService
{
    private readonly AppDbContext _db;

    public ChannelPricingService() : this(new AppDbContext())
    {
    }

    public ChannelPricingService(AppDbContext db)
    {
        _db = db;
    }

    public List<SalesChannel> GetChannels() => _db.SalesChannels
        .AsNoTracking()
        .OrderBy(x => x.SalesChannelId)
        .ToList();

    public decimal GetMenuItemExtraPrice(int menuItemId, int salesChannelId) => _db.MenuItemChannelPrices
        .AsNoTracking()
        .Where(x => x.MenuItemId == menuItemId && x.SalesChannelId == salesChannelId)
        .Select(x => x.ChannelExtraPrice)
        .FirstOrDefault();

    public decimal GetChoiceItemExtraPrice(int choiceItemId, int salesChannelId) => _db.ChoiceItemChannelPrices
        .AsNoTracking()
        .Where(x => x.ChoiceItemId == choiceItemId && x.SalesChannelId == salesChannelId)
        .Select(x => x.ChannelExtraPrice)
        .FirstOrDefault();

    public ChannelMenuDto GetMenu(string channelCode)
    {
        var channel = GetActiveChannel(channelCode);
        var categories = _db.Categories.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToList();
        var items = _db.MenuItems.AsNoTracking()
            .Include(x => x.ChannelPrices)
            .Where(x => x.Status == MenuItemStatus.Active
                && x.VisibilityStatus == VisibilityStatus.Visible
                && x.CanOrderStandalone
                && x.Category != null
                && x.Category.IsActive)
            .OrderBy(x => x.Category!.DisplayOrder)
            .ThenBy(x => x.DisplayOrder)
            .AsEnumerable()
            .Select(x => ToChannelItem(x, channel.SalesChannelId))
            .ToList();

        return new ChannelMenuDto
        {
            ChannelCode = channel.ChannelCode,
            Categories = categories,
            Items = items
        };
    }

    public ChannelMenuItemDetailDto GetItemDetail(int menuItemId, string channelCode)
    {
        var channel = GetActiveChannel(channelCode);
        var item = _db.MenuItems.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ChannelPrices)
            .FirstOrDefault(x => x.MenuItemId == menuItemId)
            ?? throw new BusinessException("Mon khong ton tai.");

        if (item.Status != MenuItemStatus.Active || item.VisibilityStatus != VisibilityStatus.Visible)
        {
            throw new BusinessException("Mon khong hien thi tren kenh ban.");
        }

        var baseDto = ToChannelItem(item, channel.SalesChannelId);
        var mappings = _db.MenuItemChoiceGroups.AsNoTracking()
            .Include(x => x.ChoiceGroup!)
            .ThenInclude(x => x.ChoiceItems.Where(i => i.IsAvailable).OrderBy(i => i.DisplayOrder))
            .ThenInclude(x => x.ChannelPrices)
            .Include(x => x.ChoiceGroup!)
            .ThenInclude(x => x.ChoiceItems)
            .ThenInclude(x => x.LinkedMenuItem)
            .Where(x => x.MenuItemId == menuItemId && x.ChoiceGroup!.IsAvailable)
            .OrderBy(x => x.DisplayOrder)
            .ToList();

        return new ChannelMenuItemDetailDto
        {
            MenuItemId = baseDto.MenuItemId,
            CategoryId = baseDto.CategoryId,
            Name = baseDto.Name,
            Description = baseDto.Description,
            ImageUrl = baseDto.ImageUrl,
            BasePrice = baseDto.BasePrice,
            ChannelExtraPrice = baseDto.ChannelExtraPrice,
            FinalPrice = baseDto.FinalPrice,
            IsAvailable = baseDto.IsAvailable,
            ChoiceGroups = mappings.Select(mapping => new ChannelChoiceGroupDto
            {
                ChoiceGroupId = mapping.ChoiceGroupId,
                GroupName = mapping.ChoiceGroup!.GroupName,
                IsRequired = mapping.IsRequired,
                MinSelect = mapping.MinSelect,
                MaxSelect = mapping.MaxSelect,
                DisplayOrder = mapping.DisplayOrder,
                ChoiceItems = mapping.ChoiceGroup.ChoiceItems
                    .Where(IsChoiceAvailable)
                    .Select(choice =>
                    {
                        var extra = choice.ChannelPrices
                            .Where(x => x.SalesChannelId == channel.SalesChannelId)
                            .Select(x => x.ChannelExtraPrice)
                            .FirstOrDefault();
                        return new ChannelChoiceItemDto
                        {
                            ChoiceItemId = choice.ChoiceItemId,
                            ChoiceName = choice.ChoiceName,
                            ExtraPrice = choice.ExtraPrice,
                            ChannelExtraPrice = extra,
                            FinalExtraPrice = choice.ExtraPrice + extra,
                            LinkedMenuItemId = choice.LinkedMenuItemId,
                            IsAvailable = true,
                            DisplayOrder = choice.DisplayOrder
                        };
                    }).ToList()
            }).ToList()
        };
    }

    public void SetMenuItemExtraPrice(int menuItemId, int salesChannelId, decimal channelExtraPrice, UserRole role)
    {
        EnsureAdminAndNonNegative(role, channelExtraPrice);
        EnsureChannelExists(salesChannelId);
        if (!_db.MenuItems.Any(x => x.MenuItemId == menuItemId))
        {
            throw new BusinessException("Mon khong ton tai.");
        }

        var price = _db.MenuItemChannelPrices.Find(menuItemId, salesChannelId);
        if (price is null)
        {
            _db.MenuItemChannelPrices.Add(new MenuItemChannelPrice
            {
                MenuItemId = menuItemId,
                SalesChannelId = salesChannelId,
                ChannelExtraPrice = channelExtraPrice
            });
        }
        else
        {
            price.ChannelExtraPrice = channelExtraPrice;
        }
        _db.SaveChanges();
    }

    public void SetChoiceItemExtraPrice(int choiceItemId, int salesChannelId, decimal channelExtraPrice, UserRole role)
    {
        EnsureAdminAndNonNegative(role, channelExtraPrice);
        EnsureChannelExists(salesChannelId);
        if (!_db.ChoiceItems.Any(x => x.ChoiceItemId == choiceItemId))
        {
            throw new BusinessException("Lua chon khong ton tai.");
        }

        var price = _db.ChoiceItemChannelPrices.Find(choiceItemId, salesChannelId);
        if (price is null)
        {
            _db.ChoiceItemChannelPrices.Add(new ChoiceItemChannelPrice
            {
                ChoiceItemId = choiceItemId,
                SalesChannelId = salesChannelId,
                ChannelExtraPrice = channelExtraPrice
            });
        }
        else
        {
            price.ChannelExtraPrice = channelExtraPrice;
        }
        _db.SaveChanges();
    }

    private SalesChannel GetActiveChannel(string channelCode)
    {
        var normalized = channelCode.Trim().ToUpperInvariant();
        return _db.SalesChannels.AsNoTracking()
            .FirstOrDefault(x => x.ChannelCode == normalized && x.IsActive)
            ?? throw new BusinessException("Kenh ban khong ton tai hoac dang ngung hoat dong.");
    }

    private static ChannelMenuItemDto ToChannelItem(MenuItem item, int channelId)
    {
        var extra = item.ChannelPrices
            .Where(x => x.SalesChannelId == channelId)
            .Select(x => x.ChannelExtraPrice)
            .FirstOrDefault();
        return new ChannelMenuItemDto
        {
            MenuItemId = item.MenuItemId,
            CategoryId = item.CategoryId,
            Name = item.Name,
            Description = item.Description,
            ImageUrl = item.ImageUrl,
            BasePrice = item.BasePrice,
            ChannelExtraPrice = extra,
            FinalPrice = item.BasePrice + extra,
            IsAvailable = item.IsAvailable && (!item.TrackStock || (item.AvailableQuantity ?? 0) > 0)
        };
    }

    private static bool IsChoiceAvailable(ChoiceItem choice)
    {
        if (!choice.IsAvailable)
        {
            return false;
        }
        var linked = choice.LinkedMenuItem;
        return linked is null
            || (linked.Status == MenuItemStatus.Active
                && linked.IsAvailable
                && (!linked.TrackStock || (linked.AvailableQuantity ?? 0) > 0));
    }

    public SalesChannel CreateChannel(string channelName, string channelCode, UserRole role)
    {
        EnsureAdminAndNonNegative(role, 0);
        var code = channelCode.Trim().ToUpperInvariant();
        var name = channelName.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessException("Tên kênh không được để trống.");
        }
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new BusinessException("Mã kênh không được để trống.");
        }
        if (_db.SalesChannels.Any(x => x.ChannelCode == code))
        {
            throw new BusinessException("Mã kênh bán đã tồn tại.");
        }

        var channel = new SalesChannel
        {
            ChannelName = name,
            ChannelCode = code,
            IsActive = true
        };
        _db.SalesChannels.Add(channel);
        _db.SaveChanges();
        return channel;
    }

    public void DeleteChannel(int salesChannelId, UserRole role)
    {
        if (role != UserRole.Admin)
        {
            throw new BusinessException("Chỉ Admin được xóa kênh bán.");
        }

        var channel = _db.SalesChannels.FirstOrDefault(x => x.SalesChannelId == salesChannelId)
            ?? throw new BusinessException("Kênh bán không tồn tại.");

        if (channel.ChannelCode.Equals("DINE_IN", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Không thể xóa kênh 'Tại quán' - đây là kênh mặc định của hệ thống.");
        }

        // Prevent delete if there are existing orders referencing this sales channel
        var hasOrders = _db.Orders.Any(o => o.SalesChannelId == salesChannelId);
        if (hasOrders)
        {
            throw new BusinessException("Không thể xóa kênh vì đã có đơn hàng liên quan. Vui lòng kiểm tra dữ liệu trước khi xóa.");
        }

        // Permanently remove the sales channel. Related MenuItem/ChoiceItem channel prices
        // are configured with cascade delete in the model, so they will be removed automatically.
        _db.SalesChannels.Remove(channel);
        _db.SaveChanges();
    }

    public void ReactivateChannel(int salesChannelId, UserRole role)
    {
        if (role != UserRole.Admin)
        {
            throw new BusinessException("Chỉ Admin được kích hoạt lại kênh bán.");
        }

        var channel = _db.SalesChannels.FirstOrDefault(x => x.SalesChannelId == salesChannelId)
            ?? throw new BusinessException("Kênh bán không tồn tại.");

        channel.IsActive = true;
        _db.SaveChanges();
    }

    private void EnsureChannelExists(int salesChannelId)
    {
        if (!_db.SalesChannels.Any(x => x.SalesChannelId == salesChannelId))
        {
            throw new BusinessException("Kenh ban khong ton tai.");
        }
    }

    private static void EnsureAdminAndNonNegative(UserRole role, decimal value)
    {
        if (role != UserRole.Admin)
        {
            throw new BusinessException("Chi Admin duoc cau hinh gia theo kenh.");
        }
        if (value < 0)
        {
            throw new BusinessException("ChannelExtraPrice khong duoc am.");
        }
    }
}
