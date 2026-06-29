using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Services.Menu;

public interface IChannelPricingService
{
    List<SalesChannel> GetChannels();
    decimal GetMenuItemExtraPrice(int menuItemId, int salesChannelId);
    decimal GetChoiceItemExtraPrice(int choiceItemId, int salesChannelId);
    ChannelMenuDto GetMenu(string channelCode);
    ChannelMenuItemDetailDto GetItemDetail(int menuItemId, string channelCode);
    void SetMenuItemExtraPrice(int menuItemId, int salesChannelId, decimal channelExtraPrice, UserRole role);
    void SetChoiceItemExtraPrice(int choiceItemId, int salesChannelId, decimal channelExtraPrice, UserRole role);
    SalesChannel CreateChannel(string channelName, string channelCode, UserRole role);
    void DeleteChannel(int salesChannelId, UserRole role);
    void ReactivateChannel(int salesChannelId, UserRole role);
}
