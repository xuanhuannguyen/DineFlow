using System.Windows;

namespace DineFlow.WPFApp.Features.Management.Tables
{
    public partial class PrintSettingsDialog : Window
    {
        public string RestaurantName { get; private set; } = string.Empty;
        public string AdditionalInfo { get; private set; } = string.Empty;

        public PrintSettingsDialog()
        {
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            RestaurantName = txtRestaurantName.Text.Trim();
            AdditionalInfo = txtAdditionalInfo.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
