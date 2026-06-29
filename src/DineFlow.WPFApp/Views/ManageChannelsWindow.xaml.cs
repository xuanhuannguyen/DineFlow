using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;
using DineFlow.Services.Menu;
using DineFlow.WPFApp.Helpers;
using System;
using System.Linq;
using System.Windows;

namespace DineFlow.WPFApp.Views;

public partial class ManageChannelsWindow : Window
{
    private readonly IChannelPricingService _channelPricingService;
    private readonly UserRole _userRole;

    public ManageChannelsWindow(IChannelPricingService channelPricingService, UserRole userRole)
    {
        InitializeComponent();
        _channelPricingService = channelPricingService;
        _userRole = userRole;
        txtPermissionHint.Text = userRole == UserRole.Admin
            ? "Chỉ xóa những kênh không còn được hệ thống tham chiếu."
            : "Bạn cần đăng nhập bằng tài khoản Admin để xóa kênh.";
        LoadChannels();
    }

    private void LoadChannels()
    {
        try
        {
            var channels = _channelPricingService.GetChannels()
                .OrderByDescending(channel => channel.ChannelCode.Equals("DINE_IN", StringComparison.OrdinalIgnoreCase))
                .ThenBy(channel => channel.ChannelName)
                .Select(channel => ChannelManagementRow.From(channel, _userRole))
                .ToList();

            dgChannels.ItemsSource = channels;
            txtChannelCount.Text = $"{channels.Count} kênh";
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: int id })
        {
            return;
        }

        var channel = _channelPricingService.GetChannels()
            .FirstOrDefault(item => item.SalesChannelId == id);
        if (channel is null)
        {
            MessageBoxHelper.ShowError("Kênh bán không còn tồn tại.");
            LoadChannels();
            return;
        }

        var confirmed = MessageBoxHelper.ShowConfirm(
            $"Xóa vĩnh viễn kênh “{channel.ChannelName}”? Giá cộng thêm đã cấu hình cho kênh này cũng sẽ bị xóa.");
        if (!confirmed)
        {
            return;
        }

        try
        {
            _channelPricingService.DeleteChannel(id, _userRole);
            MessageBoxHelper.ShowSuccess($"Đã xóa kênh {channel.ChannelName}.");
            LoadChannels();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void btnClose_Click(object sender, RoutedEventArgs e) => Close();

    private sealed record ChannelManagementRow(
        int SalesChannelId,
        string ChannelName,
        string ChannelCode,
        string StatusText,
        bool CanDelete,
        string DeleteHint)
    {
        public static ChannelManagementRow From(SalesChannel channel, UserRole userRole)
        {
            var isSystemChannel = channel.ChannelCode.Equals("DINE_IN", StringComparison.OrdinalIgnoreCase);
            var canDelete = userRole == UserRole.Admin && !isSystemChannel;
            var deleteHint = isSystemChannel
                ? "Kênh mặc định của hệ thống không thể xóa."
                : userRole != UserRole.Admin
                    ? "Chỉ Admin được phép xóa kênh."
                    : "Xóa kênh bán này.";

            return new ChannelManagementRow(
                channel.SalesChannelId,
                channel.ChannelName,
                channel.ChannelCode,
                channel.IsActive ? "Đang hoạt động" : "Đã tắt",
                canDelete,
                deleteHint);
        }
    }
}
