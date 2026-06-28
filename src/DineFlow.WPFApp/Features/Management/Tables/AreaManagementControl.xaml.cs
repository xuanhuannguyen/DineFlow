using System;
using System.Windows;
using System.Windows.Controls;
using DineFlow.BusinessObjects.Tables;
using DineFlow.Services.Tables;

namespace DineFlow.WPFApp.Features.Management.Tables
{
    public partial class AreaManagementControl : UserControl
    {
        private readonly IAreaService _areaService;
        private AreaDto? _selectedArea;

        public AreaManagementControl(IAreaService areaService)
        {
            InitializeComponent();
            _areaService = areaService;
            Loaded += AreaManagementControl_Loaded;
        }

        private void AreaManagementControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAreas();
        }

        private void LoadAreas()
        {
            try
            {
                var areas = _areaService.GetAllAreas();
                dgAreas.ItemsSource = areas;
                UpdateUIState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách khu vực: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgAreas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgAreas.SelectedItem is AreaDto area)
            {
                _selectedArea = area;
                txtAreaName.Text = area.AreaName;
                txtDescription.Text = area.Description;
            }
            else
            {
                ClearForm();
            }
            UpdateUIState();
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var request = new CreateAreaRequest
                {
                    AreaName = txtAreaName.Text,
                    Description = txtDescription.Text
                };

                _areaService.CreateArea(request);
                MessageBox.Show("Thêm khu vực thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                
                ClearForm();
                LoadAreas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedArea == null) return;

            try
            {
                var request = new UpdateAreaRequest
                {
                    AreaName = txtAreaName.Text,
                    Description = txtDescription.Text
                };

                _areaService.UpdateArea(_selectedArea.AreaId, request);
                MessageBox.Show("Cập nhật khu vực thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                
                LoadAreas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnGridToggleActive_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is AreaDto area)
            {
                string action = area.IsActive ? "NGỪNG HOẠT ĐỘNG" : "BẬT HOẠT ĐỘNG";
                var result = MessageBox.Show($"Bạn có chắc muốn {action} khu vực '{area.AreaName}'?\nLưu ý: Ngừng hoạt động khu vực sẽ không hiển thị khu vực này trên màn hình chọn bàn.", 
                                             "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;

                try
                {
                    if (area.IsActive)
                        _areaService.DeactivateArea(area.AreaId);
                    else
                        _areaService.ReactivateArea(area.AreaId);

                    MessageBox.Show($"Đã {action.ToLower()} khu vực.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    LoadAreas();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            LoadAreas();
        }

        private void ClearForm()
        {
            _selectedArea = null;
            dgAreas.SelectedItem = null;
            txtAreaName.Text = string.Empty;
            txtDescription.Text = string.Empty;
            UpdateUIState();
        }

        private void UpdateUIState()
        {
            bool isSelected = _selectedArea != null;

            btnCreate.IsEnabled = !isSelected;
            btnUpdate.IsEnabled = isSelected;
        }
    }
}
