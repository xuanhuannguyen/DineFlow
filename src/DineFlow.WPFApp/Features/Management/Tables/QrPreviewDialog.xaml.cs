using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using QRCoder;

namespace DineFlow.WPFApp.Features.Management.Tables
{
    public partial class QrPreviewDialog : Window
    {
        private string _qrUrl;

        public QrPreviewDialog(string tableName, string areaName, string qrUrl)
        {
            InitializeComponent();
            _qrUrl = qrUrl;
            txtTitle.Text = $"{tableName} - {areaName}";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrUrl, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);

            using (var ms = new MemoryStream(qrCodeAsPngByteArr))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                imgQrCode.Source = bitmapImage;
            }
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_qrUrl))
            {
                Clipboard.SetText(_qrUrl);
                MessageBox.Show("Đã copy QR URL vào Clipboard.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            var printSettings = new PrintSettingsDialog();
            printSettings.Owner = this;
            
            if (printSettings.ShowDialog() == true)
            {
                // Hiển thị thông tin lên UI
                txtPrintRestaurantName.Text = printSettings.RestaurantName;
                txtPrintAdditionalInfo.Text = printSettings.AdditionalInfo;
                
                txtPrintRestaurantName.Visibility = string.IsNullOrWhiteSpace(printSettings.RestaurantName) ? Visibility.Collapsed : Visibility.Visible;
                txtPrintAdditionalInfo.Visibility = string.IsNullOrWhiteSpace(printSettings.AdditionalInfo) ? Visibility.Collapsed : Visibility.Visible;
                txtPrintTableName.Visibility = Visibility.Visible;
                txtPrintTableName.Text = txtTitle.Text; // "Mã QR Bàn X"

                // Cập nhật lại Layout toàn bộ Window để nó kịp giãn ra (SizeToContent)
                this.UpdateLayout();
                printArea.UpdateLayout();

                var printDialog = new System.Windows.Controls.PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Ép WPF tính toán lại kích thước chính xác dựa trên vùng in của máy in
                    Size pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
                    printArea.Measure(pageSize);
                    printArea.Arrange(new Rect(new Point(0, 0), printArea.DesiredSize));
                    printArea.UpdateLayout();

                    printDialog.PrintVisual(printArea, "In Mã QR");
                }

                // Ẩn lại đi sau khi in
                txtPrintRestaurantName.Visibility = Visibility.Collapsed;
                txtPrintAdditionalInfo.Visibility = Visibility.Collapsed;
                txtPrintTableName.Visibility = Visibility.Collapsed;

                // Trả layout về bình thường
                this.UpdateLayout();
            }
        }
    }
}
