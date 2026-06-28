using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DineFlow.BusinessObjects.Tables;
using DineFlow.Services.Tables;

namespace DineFlow.WPFApp.Features.Management.Tables
{
    public partial class TableOverviewControl : UserControl
    {
        private readonly ITableReadService _tableReadService;
        private readonly IAreaService _areaService;
        private bool _isLoaded = false;

        public TableOverviewControl(ITableReadService tableReadService, IAreaService areaService)
        {
            InitializeComponent();
            _tableReadService = tableReadService;
            _areaService = areaService;
            Loaded += TableOverviewControl_Loaded;
        }

        private void TableOverviewControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAreas();
            LoadTableStatus();
            _isLoaded = true;
        }

        private void LoadAreas()
        {
            try
            {
                var areas = _areaService.GetActiveAreas().ToList();
                areas.Insert(0, new AreaDto { AreaId = 0, AreaName = "Tất cả" });
                
                int? currentSelection = (lbAreas.SelectedItem as AreaDto)?.AreaId;
                
                bool wasLoaded = _isLoaded;
                _isLoaded = false; // Tạm thời disable event SelectionChanged

                lbAreas.ItemsSource = areas;
                
                if (currentSelection.HasValue)
                {
                    var toSelect = areas.FirstOrDefault(a => a.AreaId == currentSelection.Value);
                    lbAreas.SelectedItem = toSelect ?? areas[0];
                }
                else
                {
                    lbAreas.SelectedIndex = 0;
                }
                
                _isLoaded = wasLoaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách khu vực: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTableStatus()
        {
            try
            {
                int? areaId = null;
                if (lbAreas.SelectedItem is AreaDto selectedArea && selectedArea.AreaId > 0)
                {
                    areaId = selectedArea.AreaId;
                }

                string? keyword = txtKeyword.Text.Trim();
                if (string.IsNullOrEmpty(keyword)) keyword = null;

                // Load all tables for the area and filter locally by status since "Đang có khách" implies Occupied OR WaitingPayment
                var tables = _tableReadService.GetTableStatusOverview(areaId, null, keyword);
                
                if (rbStatusAvailable.IsChecked == true)
                {
                    tables = tables.Where(t => t.Status == TableStatus.Available).ToList();
                }
                else if (rbStatusOccupied.IsChecked == true)
                {
                    tables = tables.Where(t => t.Status == TableStatus.Occupied || t.Status == TableStatus.WaitingPayment).ToList();
                }

                icTables.ItemsSource = tables;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải trạng thái bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LbAreas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;
            LoadTableStatus();
        }

        private void FilterStatus_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            LoadTableStatus();
        }

        private void TxtKeyword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded) return;
            LoadTableStatus();
        }

        private void TableCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TableStatusSummaryDto table)
            {
                // Update Dummy Bill Header (Removed as per design)

                if (table.Status == TableStatus.Available)
                {
                    // No session
                }
                else
                {
                    // For now, it's just a dummy layout so we don't need to pop a MessageBox
                }
            }
        }
    }
}
