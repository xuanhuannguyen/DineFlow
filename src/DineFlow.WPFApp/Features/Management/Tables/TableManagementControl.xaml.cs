using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using DineFlow.BusinessObjects.Tables;
using DineFlow.Services.Tables;

namespace DineFlow.WPFApp.Features.Management.Tables
{
    public partial class TableManagementControl : UserControl
    {
        private readonly ITableService _tableService;
        private readonly ITableQrService _qrService;
        private readonly IAreaService _areaService;
        private DiningTableDto? _selectedTable;

        public TableManagementControl(ITableService tableService, ITableQrService qrService, IAreaService areaService)
        {
            InitializeComponent();
            _tableService = tableService;
            _qrService = qrService;
            _areaService = areaService;
            Loaded += TableManagementControl_Loaded;
        }

        private void TableManagementControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAreas();
            LoadTables();
        }

        private void LoadAreas()
        {
            try
            {
                var areas = _areaService.GetActiveAreas();
                
                // Binding to Form ComboBox
                cboArea.ItemsSource = areas;
                
                // Binding to Filter ComboBox (Add an "All" option)
                var filterAreas = areas.ToList();
                filterAreas.Insert(0, new AreaDto { AreaId = 0, AreaName = "-- Tất cả khu vực --" });
                cboFilterArea.ItemsSource = filterAreas;
                cboFilterArea.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách khu vực: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTables()
        {
            try
            {
                int? areaId = null;
                if (cboFilterArea.SelectedValue is int id && id > 0)
                {
                    areaId = id;
                }

                var keyword = txtSearch.Text.Trim();
                var tables = _tableService.GetAllTables(keyword: string.IsNullOrEmpty(keyword) ? null : keyword, areaId: areaId);
                
                dgTables.ItemsSource = tables;
                UpdateUIState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách bàn ăn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgTables.SelectedItem is DiningTableDto table)
            {
                _selectedTable = table;
                txtTableName.Text = table.TableName;
                cboArea.SelectedValue = table.AreaId;
                txtStatus.Text = table.Status;
                txtQrToken.Text = table.QrToken;
                txtQrUrl.Text = table.QrUrl;
            }
            else
            {
                ClearForm();
            }
            UpdateUIState();
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (cboArea.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn khu vực.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var request = new CreateTableRequest
                {
                    TableName = txtTableName.Text,
                    AreaId = (int)cboArea.SelectedValue
                };

                _tableService.CreateTable(request);
                MessageBox.Show("Thêm bàn mới thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                
                ClearForm();
                LoadTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTable == null) return;
            if (cboArea.SelectedValue == null) return;

            try
            {
                var request = new UpdateTableRequest
                {
                    TableName = txtTableName.Text,
                    AreaId = (int)cboArea.SelectedValue
                };

                _tableService.UpdateTable(_selectedTable.TableId, request);
                MessageBox.Show("Cập nhật bàn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                
                LoadTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private DiningTableDto? GetTableFromSender(object sender)
        {
            if (sender is FrameworkElement element && element.DataContext is DiningTableDto table)
            {
                return table;
            }
            return null;
        }

        private void BtnGridToggleActive_Click(object sender, RoutedEventArgs e)
        {
            var table = GetTableFromSender(sender);
            if (table == null) return;

            string action = table.IsActive ? "NGỪNG HOẠT ĐỘNG" : "BẬT HOẠT ĐỘNG";
            var result = MessageBox.Show($"Bạn có chắc muốn {action} bàn '{table.TableName}'?", 
                                         "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                if (table.IsActive)
                    _tableService.DeactivateTable(table.TableId);
                else
                    _tableService.ReactivateTable(table.TableId);
                    
                MessageBox.Show($"Đã {action.ToLower()} bàn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                LoadTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnGridResetQr_Click(object sender, RoutedEventArgs e)
        {
            var table = GetTableFromSender(sender);
            if (table == null) return;

            var result = MessageBox.Show($"CẢNH BÁO: Tạo mã QR mới sẽ khiến mã QR cũ bị vô hiệu hóa.\nBạn có chắc muốn tạo mã QR mới cho '{table.TableName}'?", 
                                         "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                _qrService.ResetQrToken(table.TableId);
                MessageBox.Show("Đã tạo mã QR mới thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                LoadTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnGridCopyUrl_Click(object sender, RoutedEventArgs e)
        {
            var table = GetTableFromSender(sender);
            if (table != null && !string.IsNullOrEmpty(table.QrUrl))
            {
                Clipboard.SetText(table.QrUrl);
                MessageBox.Show("Đã copy QR URL vào Clipboard.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        private void BtnGridViewQr_Click(object sender, RoutedEventArgs e)
        {
            var table = GetTableFromSender(sender);
            if (table == null || string.IsNullOrEmpty(table.QrUrl))
            {
                MessageBox.Show("Bàn này chưa có mã QR hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new QrPreviewDialog(table.TableName, table.AreaName, table.QrUrl);
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadTables();
        }

        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            var tables = dgTables.ItemsSource as IEnumerable<DiningTableDto>;
            if (tables == null || !tables.Any())
            {
                MessageBox.Show("Không có bàn nào trong danh sách để xuất.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Mở Dialog lấy thông tin tên quán/wifi
            var printSettings = new PrintSettingsDialog();
            printSettings.Owner = Window.GetWindow(this);
            if (printSettings.ShowDialog() != true)
                return;

            string restaurantName = printSettings.RestaurantName;
            string additionalInfo = printSettings.AdditionalInfo;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Lưu danh sách mã QR ra file PDF",
                FileName = "QR_DanhSachBan.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    GeneratePdfDocument(tables.ToList(), saveFileDialog.FileName, restaurantName, additionalInfo);
                    MessageBox.Show("Xuất file PDF thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất PDF: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GeneratePdfDocument(List<DiningTableDto> tables, string filePath, string restaurantName, string additionalInfo)
        {
            var qrGenerator = new QRCodeGenerator();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        foreach (var tableItem in tables)
                        {
                            // Create QR Code byte array
                            byte[]? qrBytes = null;
                            if (!string.IsNullOrEmpty(tableItem.QrUrl))
                            {
                                var qrData = qrGenerator.CreateQrCode(tableItem.QrUrl, QRCodeGenerator.ECCLevel.Q);
                                var pngQr = new PngByteQRCode(qrData);
                                qrBytes = pngQr.GetGraphic(10);
                            }

                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
                            {
                                column.Spacing(5);
                                
                                if (!string.IsNullOrWhiteSpace(restaurantName))
                                {
                                    column.Item().AlignCenter().Text(restaurantName).SemiBold().FontSize(14);
                                }

                                if (qrBytes != null)
                                {
                                    column.Item().AlignCenter().Width(120).Height(120).Image(qrBytes);
                                }
                                else
                                {
                                    column.Item().AlignCenter().Height(120).AlignMiddle().Text("Chưa có mã QR");
                                }

                                column.Item().AlignCenter().Text($"{tableItem.TableName} - {tableItem.AreaName}").Bold().FontSize(16);

                                if (!string.IsNullOrWhiteSpace(additionalInfo))
                                {
                                    column.Item().AlignCenter().Text(additionalInfo).FontSize(11);
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Trang ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            })
            .GeneratePdf(filePath);
        }

        private void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            LoadTables();
        }

        private void ClearForm()
        {
            _selectedTable = null;
            dgTables.SelectedItem = null;
            txtTableName.Text = string.Empty;
            cboArea.SelectedItem = null;
            txtStatus.Text = string.Empty;
            txtQrToken.Text = string.Empty;
            txtQrUrl.Text = string.Empty;
            UpdateUIState();
        }

        private void UpdateUIState()
        {
            bool isSelected = _selectedTable != null;

            btnCreate.IsEnabled = !isSelected;
            btnUpdate.IsEnabled = isSelected;
        }
    }
}
