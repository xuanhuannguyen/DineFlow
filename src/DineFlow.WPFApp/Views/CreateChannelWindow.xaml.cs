using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;
using DineFlow.Services.Menu;
using DineFlow.WPFApp.Helpers;
using System;
using System.Windows;

namespace DineFlow.WPFApp.Views
{
    public partial class CreateChannelWindow : Window
    {
        private readonly IChannelPricingService _channelPricingService;
        private readonly UserRole _userRole;

        public SalesChannel? CreatedChannel { get; private set; }

        public CreateChannelWindow(IChannelPricingService channelPricingService, UserRole userRole)
        {
            InitializeComponent();
            _channelPricingService = channelPricingService;
            _userRole = userRole;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            var name = txtChannelName.Text.Trim();
            var code = txtChannelCode.Text.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBoxHelper.ShowError("Tên kênh không được để trống.");
                txtChannelName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBoxHelper.ShowError("Mã kênh không được để trống.");
                txtChannelCode.Focus();
                return;
            }

            try
            {
                CreatedChannel = _channelPricingService.CreateChannel(name, code, _userRole);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError(ex.Message);
            }
        }
    }
}
