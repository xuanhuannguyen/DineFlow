using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;
using DineFlow.Services.Menu;
using DineFlow.WPFApp.Helpers;
using Microsoft.Win32;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MenuItemEntity = DineFlow.BusinessObjects.Menu.MenuItem;

namespace DineFlow.WPFApp.Views;

public enum MenuWorkspaceSection
{
    Overview,
    Items,
    Categories,
    Choices,
    Stock,
    Pricing
}

public partial class MenuItemManagementWindow : UserControl
{
    private static readonly HashSet<string> SupportedImageExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".gif" };
    private static readonly CompareInfo SearchCompareInfo = CultureInfo.GetCultureInfo("vi-VN").CompareInfo;
    private static readonly HttpClient ImageHttpClient = CreateImageHttpClient();

    private readonly CurrentUserDto _currentUser;
    private readonly ICategoryService _categoryService = new CategoryService();
    private readonly IMenuItemService _menuItemService = new MenuItemService();
    private readonly IChoiceService _choiceService = new ChoiceService();
    private readonly IChannelPricingService _channelPricingService = new ChannelPricingService();
    private readonly DispatcherTimer _summaryRefreshTimer = new()
    {
        Interval = TimeSpan.FromSeconds(10)
    };
    private Category? _selectedCategory;
    private MenuItemEntity? _selectedMenuItem;
    private MenuItemEntity? _selectedStockItem;
    private MenuItemAddonGroup? _selectedAddonGroupMapping;
    private MenuAddonGroup? _selectedAddonGroup;
    private AddonGroupOption? _selectedAddonOption;
    private ChoiceGroup? _selectedGlobalChoiceGroup;
    private ChoiceItem? _selectedGlobalChoiceItem;
    private MenuItemChoiceGroup? _selectedGlobalMapping;
    private int? _editingChoiceItemId;
    private bool _editingGlobalGroup;
    private ScreenMode _currentScreen = ScreenMode.Dashboard;
    private bool _loadingDashboardFilters;
    private bool _loadingPricingWorkspace;
    private bool _loadingStockFilters;
    private int _imagePreviewVersion;
    private List<PricingWorkspaceMenuRow> _pricingWorkspaceRows = [];
    private List<PricingWorkspaceChoiceGroup> _pricingWorkspaceChoiceGroups = [];
    private List<SalesChannel> _pricingWorkspaceChannels = [];
    private PricingWorkspaceMenuRow? _selectedPricingWorkspaceRow;

    public MenuItemManagementWindow(CurrentUserDto currentUser)
    {
        InitializeComponent();
        _currentUser = currentUser;
        ClearCategoryForm();
        ClearForm();
        LoadAllData();
        ApplyPermission();
        ShowScreen(_currentUser.Role == UserRole.Staff ? ScreenMode.Stock : ScreenMode.Dashboard);
        _summaryRefreshTimer.Tick += SummaryRefreshTimer_Tick;
        Loaded += MenuItemManagementWindow_Loaded;
        Unloaded += MenuItemManagementWindow_Unloaded;
    }

    public void NavigateTo(MenuWorkspaceSection section)
    {
        ShowScreen(section switch
        {
            MenuWorkspaceSection.Items => ScreenMode.MenuItems,
            MenuWorkspaceSection.Categories => ScreenMode.Categories,
            MenuWorkspaceSection.Choices => ScreenMode.Addons,
            MenuWorkspaceSection.Stock => ScreenMode.Stock,
            MenuWorkspaceSection.Pricing => ScreenMode.Pricing,
            _ => ScreenMode.Dashboard
        });
    }

    private void ApplyPermission()
    {
        if (_currentUser.Role == UserRole.Admin)
        {
            return;
        }

        btnFlowCategories.Visibility = Visibility.Collapsed;
        btnFlowItems.Visibility = Visibility.Collapsed;
        btnFlowChoices.Visibility = Visibility.Collapsed;
        // Keep Pricing visible for all users so Manage Channels panel can be accessed from main page

        txtPrice.IsReadOnly = true;
        txtItemCode.IsReadOnly = true;
        txtItemName.IsReadOnly = true;
        txtDescription.IsReadOnly = true;
        txtImageUrl.IsReadOnly = true;
        cboCategory.IsEnabled = false;
        btnSaveItem.IsEnabled = false;
        btnSaveAndConfigureChoices.IsEnabled = false;
        btnHideItem.IsEnabled = false;

        txtCategoryName.IsReadOnly = true;
        txtCategoryDescription.IsReadOnly = true;
        txtCategoryDisplayOrder.IsReadOnly = true;
        btnCreateCategory.IsEnabled = false;
        btnUpdateCategory.IsEnabled = false;
        btnHideCategory.IsEnabled = false;
        btnNewCategory.IsEnabled = false;

        cboAddonParentMenuItem.IsEnabled = true;
        txtAddonGroupName.IsReadOnly = true;
        txtAddonMinSelect.IsReadOnly = true;
        txtAddonMaxSelect.IsReadOnly = true;
        txtAddonGroupOrder.IsReadOnly = true;
        txtExtraPriceOverride.IsReadOnly = true;
        txtAddonOptionOrder.IsReadOnly = true;
        txtCustomChoiceName.IsReadOnly = true;
        rbCustomChoice.IsEnabled = false;
        rbLinkedChoice.IsEnabled = false;
        chkAddonGroupRequired.IsEnabled = false;
        chkAddonGroupActive.IsEnabled = false;
        chkAddonOptionDefault.IsEnabled = false;
        lstAddonMenuItems.IsEnabled = false;
        btnAddAddonGroup.IsEnabled = false;
        btnUpdateAddonGroup.IsEnabled = false;
        btnNewChoiceVisible.IsEnabled = false;
        btnUpdateAddonOption.IsEnabled = false;
        btnHideAddonOption.IsEnabled = false;
        btnDeleteGlobalGroup.IsEnabled = false;
    }

    private void LoadAllData()
    {
        LoadCategories();
        LoadData();
        LoadStock();
        LoadPricingData();
        UpdateSummaryCards();
    }

    private void UpdateSummaryCards()
    {
        var categories = _categoryService.GetActiveCategories();
        var menuItems = _menuItemService.GetAll()
            .Where(item => item.Status != MenuItemStatus.Deleted
                && item.Category is { IsActive: true })
            .ToList();

        txtStatCategories.Text = categories.Count.ToString(CultureInfo.InvariantCulture);
        txtStatMenuItems.Text = menuItems.Count.ToString(CultureInfo.InvariantCulture);
        txtStatAvailable.Text = menuItems.Count(x =>
            x.Status == MenuItemStatus.Active
            && x.VisibilityStatus == VisibilityStatus.Visible
            && x.IsAvailable
            && (!x.TrackStock || (x.AvailableQuantity ?? 0) > 0)).ToString(CultureInfo.InvariantCulture);
        txtStatSoldOut.Text = menuItems.Count(x =>
            x.Status == MenuItemStatus.Active
            && x.VisibilityStatus == VisibilityStatus.Visible
            && x.TrackStock
            && (x.AvailableQuantity ?? 0) is > 0 and <= 10).ToString(CultureInfo.InvariantCulture);
    }

    private void MenuItemManagementWindow_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateSummaryCards();
        _summaryRefreshTimer.Start();
    }

    private void MenuItemManagementWindow_Unloaded(object sender, RoutedEventArgs e) => _summaryRefreshTimer.Stop();

    private void SummaryRefreshTimer_Tick(object? sender, EventArgs e)
    {
        if (!IsVisible)
        {
            return;
        }

        try
        {
            UpdateSummaryCards();
        }
        catch
        {
            // Keep the last valid snapshot during a transient database interruption.
        }
    }

    private void ShowCategories_Click(object sender, RoutedEventArgs e) => ShowScreen(ScreenMode.Categories);
    private void ShowMenuItems_Click(object sender, RoutedEventArgs e) => ShowScreen(ScreenMode.MenuItems);
    private void ShowAddons_Click(object sender, RoutedEventArgs e) => ShowScreen(ScreenMode.Addons);
    private void ShowStock_Click(object sender, RoutedEventArgs e) => ShowScreen(ScreenMode.Stock);
    private void ShowPricing_Click(object sender, RoutedEventArgs e) => ShowScreen(ScreenMode.Pricing);

    private void ShowScreen(ScreenMode screen)
    {
        _currentScreen = screen;
        pnlDashboard.Visibility = screen == ScreenMode.Dashboard ? Visibility.Visible : Visibility.Collapsed;
        pnlCategories.Visibility = screen == ScreenMode.Categories ? Visibility.Visible : Visibility.Collapsed;
        pnlMenuItems.Visibility = screen == ScreenMode.MenuItems ? Visibility.Visible : Visibility.Collapsed;
        pnlAddons.Visibility = screen == ScreenMode.Addons ? Visibility.Visible : Visibility.Collapsed;
        pnlStock.Visibility = screen == ScreenMode.Stock ? Visibility.Visible : Visibility.Collapsed;
        pnlPricing.Visibility = screen == ScreenMode.Pricing ? Visibility.Visible : Visibility.Collapsed;

        HideAllEditors();
        switch (screen)
        {
            case ScreenMode.Dashboard:
                _selectedMenuItem = null;
                dgMenuItems.SelectedItem = null;
                lstDashboardMenuItems.SelectedItem = null;
                break;
            case ScreenMode.Categories:
                _selectedCategory = null;
                dgCategories.SelectedItem = null;
                break;
            case ScreenMode.MenuItems:
                _selectedMenuItem = null;
                dgMenuItems.SelectedItem = null;
                break;
            case ScreenMode.Addons:
                LoadGlobalChoiceLibrary();
                break;
            case ScreenMode.Stock:
                // Stock screen uses an inline stock management panel inside pnlStock.
                break;
            case ScreenMode.Pricing:
                break;
        }

        txtScreenTitle.Text = screen switch
        {
            ScreenMode.Categories => "Danh mục món ăn",
            ScreenMode.MenuItems => "Món ăn",
            ScreenMode.Addons => "Thư viện lựa chọn",
            ScreenMode.Stock => "Tồn kho",
            ScreenMode.Pricing => "Giá theo kênh",
            _ => "Tổng quan menu"
        };

        txtScreenSubtitle.Text = screen switch
        {
            ScreenMode.Categories => "Bước 01 · Tạo nhóm phân loại trước khi thêm món.",
            ScreenMode.MenuItems => "Bước 02 · Nhập thông tin, giá tại quán, trạng thái bán và tồn kho.",
            ScreenMode.Addons => "Chỉnh sửa nhóm, option và giá cộng thêm dùng chung cho toàn bộ menu.",
            ScreenMode.Stock => "Bước 04 · Kiểm tra số lượng còn lại và mở hoặc tạm ngưng bán.",
            ScreenMode.Pricing => "Cấu hình phần giá cộng thêm cho món và lựa chọn trên từng kênh bán.",
            _ => "Theo dõi toàn bộ menu và đi theo luồng cấu hình từ trái sang phải."
        };

        txtDetailHint.Text = screen switch
        {
            ScreenMode.Categories => "Nhấn + Danh mục để tạo mới, hoặc chọn một dòng để chỉnh sửa.",
            ScreenMode.MenuItems => "Nhấn + Món mới để tạo, hoặc chọn một món để chỉnh sửa và cập nhật trạng thái.",
            ScreenMode.Addons => "Chọn một nhóm trong thư viện để chỉnh sửa các lựa chọn bên trong.",
            ScreenMode.Stock => "Chọn thẻ món để cập nhật nhanh số lượng và trạng thái bán.",
            ScreenMode.Pricing => "Chọn đối tượng, kênh bán và nhập phần giá cộng thêm.",
            _ => "Chọn một món trong danh sách để xem và chỉnh sửa."
        };

        if (screen == ScreenMode.Pricing)
        {
            LoadPricingData();
        }

        UpdateSummaryCards();
        ApplyNavSelection(screen);
    }

    private void HideAllEditors()
    {
        pnlItemEditor.Visibility = Visibility.Collapsed;
        pnlCategoryEditor.Visibility = Visibility.Collapsed;
        pnlAddonEditor.Visibility = Visibility.Collapsed;
        pnlDetailPane.Visibility = Visibility.Collapsed;
        detailColumn.Width = new GridLength(0);
    }

    private void ShowDetailPane(double width = 480)
    {
        detailColumn.Width = new GridLength(width);
        pnlDetailPane.Visibility = Visibility.Visible;
    }

    private void btnCloseDetail_Click(object sender, RoutedEventArgs e) => HideAllEditors();

    private void ShowAddonGroupEditor(bool isCreate)
    {
        HideAllEditors();
        ShowDetailPane();
        pnlAddonEditor.Visibility = Visibility.Visible;
        pnlAddonGroupEditor.Visibility = Visibility.Visible;
        pnlAddonChoiceEditor.Visibility = Visibility.Collapsed;
        btnAddAddonGroup.Visibility = isCreate ? Visibility.Visible : Visibility.Collapsed;
        btnUpdateAddonGroup.Visibility = isCreate ? Visibility.Collapsed : Visibility.Visible;
        txtDetailHint.Text = isCreate ? "Tạo nhóm lựa chọn mới" : "Chỉnh sửa nhóm đã chọn";

        var appliedItems = _selectedGlobalChoiceGroup?.MenuItems
            .Where(mapping => mapping.MenuItem is not null)
            .Select(mapping => AppliedMenuItemRow.From(mapping, _currentUser.Role == UserRole.Admin))
            .OrderBy(item => item.ItemName)
            .ToList() ?? [];
        lstGlobalAppliedItems.ItemsSource = appliedItems;
        txtGlobalAppliedCount.Text = $"{appliedItems.Count} món";
        txtGlobalAppliedEmpty.Visibility = appliedItems.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        if (!isCreate)
        {
            return;
        }

        txtAddonGroupName.Clear();
        txtAddonMinSelect.Text = "0";
        txtAddonMaxSelect.Text = "1";
        txtAddonGroupOrder.Text = (_choiceService.GetMappings(_selectedMenuItem?.MenuItemId ?? 0).Count + 1)
            .ToString(CultureInfo.InvariantCulture);
        chkAddonGroupRequired.IsChecked = false;
        chkAddonGroupActive.IsChecked = true;
        txtAddonGroupName.Focus();
    }

    private void ShowAddonChoiceEditor()
    {
        HideAllEditors();
        ShowDetailPane();
        pnlAddonEditor.Visibility = Visibility.Visible;
        pnlAddonGroupEditor.Visibility = Visibility.Collapsed;
        pnlAddonChoiceEditor.Visibility = Visibility.Visible;
        txtDetailHint.Text = _editingChoiceItemId.HasValue ? "Chỉnh sửa lựa chọn" : "Thêm lựa chọn mới";
    }

    private void ShowItemEditor(EditorMode mode)
    {
        HideAllEditors();
        ShowDetailPane(640);
        pnlItemEditor.Visibility = Visibility.Visible;
        var isCreate = mode == EditorMode.Create;

        btnSaveItem.Content = isCreate ? "Tạo món" : "Lưu thay đổi";
        btnItemReset.Visibility = isCreate ? Visibility.Visible : Visibility.Collapsed;

        if (isCreate)
        {
            btnReopenItem.Visibility = Visibility.Collapsed;
            btnSoldOutItem.Visibility = Visibility.Collapsed;
            btnHideItem.Visibility = Visibility.Collapsed;
            pnlItemActionGrid.Columns = 1;
            txtDetailHint.Text = "Nhập thông tin món mới. Các thao tác trạng thái chỉ xuất hiện sau khi món được tạo.";
        }
        else
        {
            pnlItemActionGrid.Columns = 2;
            txtDetailHint.Text = "Đang chỉnh sửa món đã chọn. Bạn có thể lưu thay đổi hoặc cập nhật trạng thái bán.";

            btnHideItem.Visibility = Visibility.Visible;
            if (_selectedMenuItem is not null)
            {
                if (_selectedMenuItem.IsAvailable)
                {
                    btnSoldOutItem.Visibility = Visibility.Visible;
                    btnReopenItem.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btnSoldOutItem.Visibility = Visibility.Collapsed;
                    btnReopenItem.Visibility = Visibility.Visible;
                }
            }
            else
            {
                btnReopenItem.Visibility = Visibility.Visible;
                btnSoldOutItem.Visibility = Visibility.Visible;
                btnHideItem.Visibility = Visibility.Visible;
            }
        }
    }

    private void ShowCategoryEditor(EditorMode mode)
    {
        HideAllEditors();
        ShowDetailPane();
        pnlCategoryEditor.Visibility = Visibility.Visible;
        var isCreate = mode == EditorMode.Create;

        btnCreateCategory.Visibility = isCreate ? Visibility.Visible : Visibility.Collapsed;
        btnUpdateCategory.Visibility = isCreate ? Visibility.Collapsed : Visibility.Visible;
        btnHideCategory.Visibility = !isCreate
            ? Visibility.Visible
            : Visibility.Collapsed;
        btnCategoryReset.Visibility = isCreate ? Visibility.Visible : Visibility.Collapsed;
        pnlCategoryActionGrid.Columns = isCreate ? 1 : 2;
        txtDetailHint.Text = isCreate
            ? "Nhập thông tin để tạo danh mục mới."
            : "Đang chỉnh sửa danh mục đã chọn.";
    }

    private void LoadPricingData() => LoadPricingWorkspace(_selectedPricingWorkspaceRow?.Source.MenuItemId);

    private void LoadPricingWorkspace(int? selectedMenuItemId = null)
    {
        if (lstPricingMenuRows is null || cboPricingWorkspaceCategory is null)
        {
            return;
        }

        _loadingPricingWorkspace = true;
        try
        {
            selectedMenuItemId ??= _selectedPricingWorkspaceRow?.Source.MenuItemId;
            _pricingWorkspaceChannels = _channelPricingService.GetChannels()
                .Where(channel => channel.IsActive)
                .OrderByDescending(channel => channel.ChannelCode == "DINE_IN")
                .ThenBy(channel => channel.SalesChannelId)
                .ToList();

            var selectedCategoryId = (cboPricingWorkspaceCategory.SelectedItem as PricingCategoryFilter)?.CategoryId;
            var menuItems = _menuItemService.GetAll()
                .OrderBy(item => item.Category?.DisplayOrder ?? int.MaxValue)
                .ThenBy(item => item.ItemName)
                .ToList();

            _pricingWorkspaceRows = menuItems
                .Select(item => PricingWorkspaceMenuRow.From(item, _pricingWorkspaceChannels, _channelPricingService))
                .ToList();

            var categoryFilters = new List<PricingCategoryFilter> { new("Tất cả danh mục", null) };
            categoryFilters.AddRange(menuItems
                .Where(item => item.Category is not null)
                .GroupBy(item => item.CategoryId)
                .Select(group => new PricingCategoryFilter(group.First().Category!.CategoryName, group.Key))
                .OrderBy(filter => filter.Label));
            cboPricingWorkspaceCategory.ItemsSource = categoryFilters;
            cboPricingWorkspaceCategory.SelectedItem = categoryFilters.FirstOrDefault(filter => filter.CategoryId == selectedCategoryId)
                ?? categoryFilters[0];

            ApplyPricingWorkspaceFilters(selectedMenuItemId);
        }
        finally
        {
            _loadingPricingWorkspace = false;
        }
    }

    private void PricingWorkspaceFilter_Changed(object sender, RoutedEventArgs e)
    {
        if (!_loadingPricingWorkspace)
        {
            ApplyPricingWorkspaceFilters(_selectedPricingWorkspaceRow?.Source.MenuItemId);
        }
    }

    private void ApplyPricingWorkspaceFilters(int? selectedMenuItemId = null)
    {
        if (lstPricingMenuRows is null)
        {
            return;
        }

        var keyword = txtPricingWorkspaceSearch?.Text.Trim() ?? string.Empty;
        var categoryId = (cboPricingWorkspaceCategory?.SelectedItem as PricingCategoryFilter)?.CategoryId;
        var filtered = _pricingWorkspaceRows
            .Where(row => !categoryId.HasValue || row.Source.CategoryId == categoryId.Value)
            .Where(row => string.IsNullOrWhiteSpace(keyword)
                || new[] { row.ItemName, row.ItemCode, row.CategoryName }.Any(value =>
                    SearchCompareInfo.IndexOf(value, keyword,
                        CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0))
            .ToList();

        lstPricingMenuRows.ItemsSource = filtered;
        txtPricingWorkspaceSummary.Text = $"Hiển thị {filtered.Count} / {_pricingWorkspaceRows.Count} món";
        lstPricingMenuRows.SelectedItem = filtered.FirstOrDefault(row => row.Source.MenuItemId == selectedMenuItemId)
            ?? filtered.FirstOrDefault();
    }

    private void lstPricingMenuRows_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (lstPricingMenuRows.SelectedItem is not PricingWorkspaceMenuRow row)
        {
            _selectedPricingWorkspaceRow = null;
            itemsPricingWorkspaceChannels.ItemsSource = null;
            itemsPricingWorkspaceChoiceGroups.ItemsSource = null;
            return;
        }

        _selectedPricingWorkspaceRow = row;
        txtPricingWorkspaceSelectedName.Text = row.ItemName;
        txtPricingWorkspaceSelectedMeta.Text = $"Mã món: {row.ItemCode}  ·  Danh mục: {row.CategoryName}";
        txtPricingWorkspaceBasePrice.Text = row.BasePriceText;
        itemsPricingWorkspaceChannels.ItemsSource = row.EditableChannels;

        _pricingWorkspaceChoiceGroups = _choiceService.GetMappings(row.Source.MenuItemId)
            .Where(mapping => mapping.ChoiceGroup is not null)
            .OrderBy(mapping => mapping.DisplayOrder)
            .Select(mapping => PricingWorkspaceChoiceGroup.From(
                mapping.ChoiceGroup!, _pricingWorkspaceChannels, _channelPricingService))
            .ToList();
        itemsPricingWorkspaceChoiceGroups.ItemsSource = _pricingWorkspaceChoiceGroups;
        txtPricingWorkspaceChoiceEmpty.Visibility = _pricingWorkspaceChoiceGroups.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void btnResetPricingWorkspace_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedPricingWorkspaceRow is not null)
        {
            LoadPricingWorkspace(_selectedPricingWorkspaceRow.Source.MenuItemId);
        }
    }

    private void btnSavePricingWorkspace_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedPricingWorkspaceRow is null)
        {
            MessageBoxHelper.ShowError("Chọn món cần cấu hình giá.");
            return;
        }

        var allPrices = _selectedPricingWorkspaceRow.EditableChannels
            .Concat(_pricingWorkspaceChoiceGroups.SelectMany(group => group.Choices).SelectMany(choice => choice.ChannelPrices))
            .Where(price => price.IsEditable)
            .ToList();
        if (allPrices.Any(price => !TryParseCurrencyInput(price.ExtraPriceInput, out var value) || value < 0))
        {
            MessageBoxHelper.ShowError("Giá cộng thêm phải là số không âm.");
            return;
        }

        try
        {
            foreach (var price in _selectedPricingWorkspaceRow.EditableChannels.Where(price => price.IsEditable))
            {
                TryParseCurrencyInput(price.ExtraPriceInput, out var value);
                _channelPricingService.SetMenuItemExtraPrice(
                    _selectedPricingWorkspaceRow.Source.MenuItemId, price.Source.SalesChannelId, value, _currentUser.Role);
            }

            foreach (var choice in _pricingWorkspaceChoiceGroups.SelectMany(group => group.Choices))
            {
                foreach (var price in choice.ChannelPrices.Where(price => price.IsEditable))
                {
                    TryParseCurrencyInput(price.ExtraPriceInput, out var value);
                    _channelPricingService.SetChoiceItemExtraPrice(
                        choice.Source.ChoiceItemId, price.Source.SalesChannelId, value, _currentUser.Role);
                }
            }

            var selectedId = _selectedPricingWorkspaceRow.Source.MenuItemId;
            LoadPricingWorkspace(selectedId);
            MessageBoxHelper.ShowSuccess("Đã lưu cấu hình giá theo kênh.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void btnCreateNewChannel_Click(object sender, RoutedEventArgs e)
    {
        var parentWindow = Window.GetWindow(this);
        var dialog = new CreateChannelWindow(_channelPricingService, _currentUser.Role)
        {
            Owner = parentWindow
        };

        if (dialog.ShowDialog() == true && dialog.CreatedChannel is not null)
        {
            MessageBoxHelper.ShowSuccess($"Đã tạo kênh bán mới: {dialog.CreatedChannel.ChannelName}");
            LoadPricingData(); // Refresh pricing lists and channel dropdowns
        }
    }

    private void btnManageChannels_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ManageChannelsWindow(_channelPricingService, _currentUser.Role)
        {
            Owner = Window.GetWindow(this)
        };

        dialog.ShowDialog();
        LoadPricingData();
    }

    private void ApplyNavSelection(ScreenMode screen)
    {
        var navButtons = new Dictionary<ScreenMode, Button>
        {
            [ScreenMode.Categories] = btnFlowCategories,
            [ScreenMode.MenuItems] = btnFlowItems,
            [ScreenMode.Addons] = btnFlowChoices,
            [ScreenMode.Stock] = btnFlowStock,
            [ScreenMode.Pricing] = btnFlowPricing
        };

        foreach (var (mode, button) in navButtons)
        {
            var isActive = mode == screen;
            button.Background = BrushFrom(isActive ? "#0866E5" : "#FFFFFF");
            button.BorderBrush = BrushFrom(isActive ? "#0866E5" : "#E2E8F0");
            button.Foreground = BrushFrom(isActive ? "#FFFFFF" : "#475569");
        }
    }

    private void LoadCategories() => ApplyCategoryFilters();

    private void LoadData()
    {
        cboCategory.ItemsSource = _categoryService.GetActiveCategories();
        var menuItems = _menuItemService.GetAll();
        LoadItemCategoryFilter();
        PopulateDashboardCategoryFilter();
        cboAddonParentMenuItem.ItemsSource = menuItems;
        if (cboAddonParentMenuItem.SelectedIndex < 0 && menuItems.Count > 0)
        {
            cboAddonParentMenuItem.SelectedIndex = 0;
        }
        lstAddonMenuItems.ItemsSource = GetAddonCandidateRows(menuItems);
        ApplyMenuItemFilters();
        LoadDashboardMenuItems();
    }

    private void LoadItemCategoryFilter()
    {
        if (cboItemCategoryFilter is null)
        {
            return;
        }

        var selectedCategoryId = cboItemCategoryFilter.SelectedItem is ComboBoxItem { Tag: int id }
            ? id
            : (int?)null;

        cboItemCategoryFilter.Items.Clear();
        var allCategoriesItem = new ComboBoxItem { Content = "Tất cả danh mục" };
        cboItemCategoryFilter.Items.Add(allCategoriesItem);

        ComboBoxItem? itemToSelect = null;
        foreach (var category in _categoryService.GetActiveCategories()
                     .OrderBy(x => x.DisplayOrder)
                     .ThenBy(x => x.CategoryName))
        {
            var item = new ComboBoxItem
            {
                Content = category.CategoryName,
                Tag = category.CategoryId
            };
            cboItemCategoryFilter.Items.Add(item);
            if (category.CategoryId == selectedCategoryId)
            {
                itemToSelect = item;
            }
        }

        cboItemCategoryFilter.SelectedItem = itemToSelect ?? allCategoriesItem;
    }

    private void LoadStock()
    {
        PopulateStockCategoryFilter();
        ApplyStockFilters();
        LoadDashboardMenuItems();
    }

    private void PopulateDashboardCategoryFilter()
    {
        if (cboDashboardCategoryFilter is null)
        {
            return;
        }

        var selectedCategoryId = cboDashboardCategoryFilter.SelectedItem is ComboBoxItem { Tag: int id }
            ? id
            : (int?)null;
        _loadingDashboardFilters = true;
        try
        {
            cboDashboardCategoryFilter.Items.Clear();
            var allItem = new ComboBoxItem { Content = "Tất cả danh mục", Tag = "All" };
            cboDashboardCategoryFilter.Items.Add(allItem);

            ComboBoxItem? selectedItem = null;
            foreach (var category in _categoryService.GetActiveCategories()
                         .OrderBy(item => item.DisplayOrder)
                         .ThenBy(item => item.CategoryName))
            {
                var item = new ComboBoxItem { Content = category.CategoryName, Tag = category.CategoryId };
                cboDashboardCategoryFilter.Items.Add(item);
                if (category.CategoryId == selectedCategoryId)
                {
                    selectedItem = item;
                }
            }

            cboDashboardCategoryFilter.SelectedItem = selectedItem ?? allItem;
        }
        finally
        {
            _loadingDashboardFilters = false;
        }
    }

    private void PopulateStockCategoryFilter()
    {
        if (cboStockCategoryFilter is null)
        {
            return;
        }

        var selectedCategoryId = (cboStockCategoryFilter.SelectedItem as ComboBoxItem)?.Tag as int?;
        _loadingStockFilters = true;
        try
        {
            cboStockCategoryFilter.Items.Clear();
            var allItem = new ComboBoxItem { Content = "Tất cả danh mục", Tag = "All" };
            cboStockCategoryFilter.Items.Add(allItem);

            ComboBoxItem? selectedItem = null;
            foreach (var category in _categoryService.GetAll().OrderBy(item => item.DisplayOrder).ThenBy(item => item.CategoryName))
            {
                var item = new ComboBoxItem { Content = category.CategoryName, Tag = category.CategoryId };
                cboStockCategoryFilter.Items.Add(item);
                if (category.CategoryId == selectedCategoryId)
                {
                    selectedItem = item;
                }
            }

            cboStockCategoryFilter.SelectedItem = selectedItem ?? allItem;
        }
        finally
        {
            _loadingStockFilters = false;
        }
    }

    private void ApplyCategoryFilters()
    {
        if (dgCategories is null || txtCategorySearch is null)
        {
            return;
        }

        var keyword = txtCategorySearch.Text.Trim();
        var allItems = _menuItemService.GetAll();
        var categories = _categoryService.GetActiveCategories().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            categories = categories.Where(c =>
                c.CategoryName.Contains(keyword, StringComparison.InvariantCultureIgnoreCase)
                || (c.Description?.Contains(keyword, StringComparison.InvariantCultureIgnoreCase) == true));
        }

        dgCategories.ItemsSource = categories
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.CategoryName)
            .Select(c => CategoryRow.From(c, allItems.Count(i => i.CategoryId == c.CategoryId)))
            .ToList();
    }

    private void ApplyMenuItemFilters()
    {
        if (dgMenuItems is null || txtItemListSummary is null || txtItemSearch is null
            || cboItemCategoryFilter is null || pnlMenuItemsEmpty is null)
        {
            return;
        }

        var keyword = txtItemSearch.Text.Trim();
        var allMenuItems = _menuItemService.GetAll();
        var menuItems = allMenuItems.AsEnumerable();

        if (cboItemCategoryFilter.SelectedItem is ComboBoxItem { Tag: int selectedCategoryId })
        {
            menuItems = menuItems.Where(x => x.CategoryId == selectedCategoryId);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            menuItems = menuItems.Where(x => MatchesMenuItemSearch(x, keyword));
        }

        menuItems = GetSelectedComboIndex(cboItemStatusFilter) switch
        {
            0 => menuItems.Where(x => x.Status == MenuItemStatus.Active && x.VisibilityStatus == VisibilityStatus.Visible && x.IsAvailable && x.Category is { IsActive: true }),
            1 => menuItems.Where(x => x.Status == MenuItemStatus.Active && x.VisibilityStatus == VisibilityStatus.Visible && !x.IsAvailable && x.Category is { IsActive: true }),
            2 => menuItems.Where(x => x.Status == MenuItemStatus.Active && x.VisibilityStatus == VisibilityStatus.Visible && x.TrackStock && (x.AvailableQuantity ?? 0) > 0 && (x.AvailableQuantity ?? 0) <= 5 && x.Category is { IsActive: true }),
            _ => menuItems
        };

        var rows = menuItems
            .OrderBy(x => x.Category?.DisplayOrder ?? int.MaxValue)
            .ThenBy(x => x.ItemName)
            .Select(MenuItemRow.From)
            .ToList();

        dgMenuItems.ItemsSource = rows;
        var activeFilterCount = 0;
        if (!string.IsNullOrWhiteSpace(keyword)) activeFilterCount++;
        if (cboItemCategoryFilter.SelectedItem is ComboBoxItem { Tag: int }) activeFilterCount++;
        if (GetSelectedComboIndex(cboItemStatusFilter) != 3) activeFilterCount++;

        txtItemListSummary.Text = activeFilterCount == 0
            ? $"Hiển thị {rows.Count} món"
            : $"Tìm thấy {rows.Count}/{allMenuItems.Count} món  ·  {activeFilterCount} điều kiện lọc";
        pnlMenuItemsEmpty.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private static bool MatchesMenuItemSearch(MenuItemEntity item, string keyword)
    {
        var searchableValues = new[]
        {
            item.ItemCode,
            item.ItemName,
            item.Description,
            item.Category?.CategoryName
        };

        return keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .All(term => searchableValues.Any(value =>
                SearchCompareInfo.IndexOf(value ?? string.Empty, term, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0));
    }

    private void ApplyStockFilters()
    {
        if (dgStockItems is null || txtStockSearch is null)
        {
            return;
        }

        var allTrackedItems = _menuItemService.GetAll()
            .Where(item => item.TrackStock)
            .ToList();
        var keyword = txtStockSearch.Text.Trim();
        var typeTag = (cboStockTypeFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
        var categoryTag = (cboStockCategoryFilter.SelectedItem as ComboBoxItem)?.Tag;
        var selectedItemId = _selectedStockItem?.MenuItemId;

        var rows = allTrackedItems
            .Where(item => string.IsNullOrWhiteSpace(keyword) || MatchesMenuItemSearch(item, keyword))
            .Where(item => typeTag == "All" || item.ItemType.ToString() == typeTag)
            .Where(item => categoryTag is not int categoryId || item.CategoryId == categoryId)
            .Select(StockCardRow.From)
            .Where(row => GetSelectedComboIndex(cboStockFilter) switch
            {
                0 => true,
                1 => row.Quantity is > 0 and <= 10,
                2 => row.Quantity == 0,
                3 => row.Quantity > 10,
                _ => true
            })
            .OrderBy(x => x.Quantity)
            .ThenBy(x => x.ItemName)
            .ToList();

        dgStockItems.ItemsSource = rows;
        pnlStockEmpty.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        txtStockResultSummary.Text = $"Hiển thị {rows.Count} / {allTrackedItems.Count} món đang theo dõi tồn";
        txtStockKpiTotal.Text = allTrackedItems.Count.ToString(CultureInfo.InvariantCulture);
        txtStockKpiGood.Text = allTrackedItems.Count(item => (item.AvailableQuantity ?? 0) > 10).ToString(CultureInfo.InvariantCulture);
        txtStockKpiLow.Text = allTrackedItems.Count(item => (item.AvailableQuantity ?? 0) is > 0 and <= 10).ToString(CultureInfo.InvariantCulture);
        txtStockKpiOut.Text = allTrackedItems.Count(item => (item.AvailableQuantity ?? 0) == 0).ToString(CultureInfo.InvariantCulture);

        var selectedRow = rows.FirstOrDefault(row => row.Source.MenuItemId == selectedItemId);
        if (selectedRow is not null)
        {
            dgStockItems.SelectedItem = selectedRow;
            dgStockItems.ScrollIntoView(selectedRow);
        }
        else
        {
            ClearStockEditor();
        }
    }

    private void LoadDashboardMenuItems()
    {
        if (lstDashboardMenuItems is null || txtDashboardSearch is null
            || cboDashboardCategoryFilter is null || cboDashboardTypeFilter is null
            || cboDashboardStatusFilter is null)
        {
            return;
        }

        var keyword = txtDashboardSearch.Text.Trim();
        var allMenuItems = _menuItemService.GetAll()
            .Where(item => item.Status != MenuItemStatus.Deleted
                && item.Category is { IsActive: true })
            .ToList();
        var menuItems = allMenuItems.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            menuItems = menuItems.Where(item => MatchesMenuItemSearch(item, keyword));
        }

        if (cboDashboardCategoryFilter.SelectedItem is ComboBoxItem { Tag: int categoryId })
        {
            menuItems = menuItems.Where(item => item.CategoryId == categoryId);
        }

        var typeTag = (cboDashboardTypeFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
        if (typeTag != "All")
        {
            menuItems = menuItems.Where(item => item.ItemType.ToString() == typeTag);
        }

        menuItems = cboDashboardStatusFilter.SelectedIndex switch
        {
            1 => menuItems.Where(item => item.Status == MenuItemStatus.Active
                && item.VisibilityStatus == VisibilityStatus.Visible
                && item.IsAvailable
                && (!item.TrackStock || (item.AvailableQuantity ?? 0) > 0)),
            2 => menuItems.Where(item => item.Status == MenuItemStatus.Active
                && item.TrackStock
                && (item.AvailableQuantity ?? 0) is > 0 and <= 10),
            3 => menuItems.Where(item => item.TrackStock && (item.AvailableQuantity ?? 0) == 0),
            4 => menuItems.Where(item => item.Status != MenuItemStatus.Active
                || item.VisibilityStatus == VisibilityStatus.Hidden
                || !item.IsAvailable),
            _ => menuItems
        };

        var rows = menuItems
            .OrderBy(x => x.Category?.DisplayOrder ?? int.MaxValue)
            .ThenBy(x => x.ItemName)
            .Select(MenuItemRow.From)
            .ToList();

        lstDashboardMenuItems.ItemsSource = rows;
        var activeFilterCount = 0;
        if (!string.IsNullOrWhiteSpace(keyword)) activeFilterCount++;
        if (cboDashboardCategoryFilter.SelectedItem is ComboBoxItem { Tag: int }) activeFilterCount++;
        if (typeTag != "All") activeFilterCount++;
        if (cboDashboardStatusFilter.SelectedIndex != 0) activeFilterCount++;
        txtDashboardMenuSummary.Text = activeFilterCount == 0
            ? $"{rows.Count} món trong menu"
            : $"Tìm thấy {rows.Count}/{allMenuItems.Count} món · {activeFilterCount} điều kiện lọc";
        pnlDashboardEmpty.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private static int GetSelectedComboIndex(ComboBox? comboBox) => comboBox?.SelectedIndex ?? 0;

    private void txtCategorySearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyCategoryFilters();
    private void DashboardFilter_Changed(object sender, RoutedEventArgs e)
    {
        if (!_loadingDashboardFilters)
        {
            LoadDashboardMenuItems();
        }
    }

    private void btnClearDashboardFilters_Click(object sender, RoutedEventArgs e)
    {
        _loadingDashboardFilters = true;
        try
        {
            txtDashboardSearch.Clear();
            cboDashboardCategoryFilter.SelectedIndex = 0;
            cboDashboardTypeFilter.SelectedIndex = 0;
            cboDashboardStatusFilter.SelectedIndex = 0;
        }
        finally
        {
            _loadingDashboardFilters = false;
        }

        LoadDashboardMenuItems();
        txtDashboardSearch.Focus();
    }
    private void txtItemSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyMenuItemFilters();
    private void txtGlobalGroupSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadGlobalChoiceLibrary();
    private void cboItemFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyMenuItemFilters();
    private void btnClearItemFilters_Click(object sender, RoutedEventArgs e)
    {
        txtItemSearch.Clear();
        cboItemCategoryFilter.SelectedIndex = 0;
        cboItemStatusFilter.SelectedIndex = 3;
        ApplyMenuItemFilters();
        txtItemSearch.Focus();
    }
    private void txtStockSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_loadingStockFilters)
        {
            ApplyStockFilters();
        }
    }
    private void StockFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_loadingStockFilters)
        {
            ApplyStockFilters();
        }
    }

    private void btnClearStockFilters_Click(object sender, RoutedEventArgs e)
    {
        _loadingStockFilters = true;
        try
        {
            txtStockSearch.Clear();
            cboStockTypeFilter.SelectedIndex = 0;
            cboStockCategoryFilter.SelectedIndex = 0;
            cboStockFilter.SelectedIndex = 0;
        }
        finally
        {
            _loadingStockFilters = false;
        }

        ApplyStockFilters();
        txtStockSearch.Focus();
    }

    private void RefreshAfterMenuChange()
    {
        LoadAllData();
        if (_selectedMenuItem is not null)
        {
            LoadAddonGroups();
        }
    }

    private void dgCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedCategory = (dgCategories.SelectedItem as CategoryRow)?.Source;
        if (_selectedCategory is null)
        {
            return;
        }

        txtCategoryName.Text = _selectedCategory.CategoryName;
        txtCategoryDescription.Text = _selectedCategory.Description;
        txtCategoryDisplayOrder.Text = _selectedCategory.DisplayOrder.ToString(CultureInfo.InvariantCulture);
        ShowCategoryEditor(EditorMode.Edit);
        ApplyMenuItemFilters();
    }

    private void btnNewCategory_Click(object sender, RoutedEventArgs e)
    {
        ClearCategoryForm();
        ShowCategoryEditor(EditorMode.Create);
        txtCategoryName.Focus();
    }

    private void btnCategoryCreate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _categoryService.Create(ReadCategoryForm(), _currentUser.Role);
            LoadAllData();
            ClearCategoryForm();
            HideAllEditors();
            MessageBoxHelper.ShowSuccess("Đã tạo danh mục.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void btnCategoryUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCategory is null)
        {
            MessageBoxHelper.ShowError("Hãy chọn một danh mục trước.");
            return;
        }

        try
        {
            var category = ReadCategoryForm();
            category.CategoryId = _selectedCategory.CategoryId;
            _categoryService.Update(category, _currentUser.Role);
            LoadAllData();
            ClearCategoryForm();
            HideAllEditors();
            MessageBoxHelper.ShowSuccess("Đã cập nhật danh mục.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void btnCategorySoftDelete_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCategory is null)
        {
            MessageBoxHelper.ShowError("Hãy chọn một danh mục trước.");
            return;
        }

        if (!MessageBoxHelper.ShowConfirm($"Xóa danh mục “{_selectedCategory.CategoryName}” khỏi giao diện?"))
        {
            return;
        }

        try
        {
            _categoryService.SoftDelete(_selectedCategory.CategoryId, _currentUser.Role);
            LoadAllData();
            ClearCategoryForm();
            HideAllEditors();
            MessageBoxHelper.ShowSuccess("Đã xóa danh mục khỏi giao diện.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void btnCategoryClear_Click(object sender, RoutedEventArgs e)
    {
        ClearCategoryForm();
        ShowCategoryEditor(EditorMode.Create);
        txtCategoryName.Focus();
    }

    private Category ReadCategoryForm()
    {
        return new Category
        {
            CategoryName = txtCategoryName.Text,
            Description = txtCategoryDescription.Text,
            DisplayOrder = int.TryParse(txtCategoryDisplayOrder.Text, out var displayOrder) ? displayOrder : -1,
            IsActive = true
        };
    }

    private void ClearCategoryForm()
    {
        _selectedCategory = null;
        dgCategories.SelectedItem = null;
        txtCategoryName.Clear();
        txtCategoryDescription.Clear();
        txtCategoryDisplayOrder.Text = "0";
        ApplyMenuItemFilters();
    }

    private void dgMenuItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedMenuItem = (dgMenuItems.SelectedItem as MenuItemRow)?.Source;
        if (_selectedMenuItem is null)
        {
            return;
        }

        FillItemEditor(_selectedMenuItem);
        ShowItemEditor(EditorMode.Edit);
        LoadAddonGroups();
    }

    private void lstDashboardMenuItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedMenuItem = (lstDashboardMenuItems.SelectedItem as MenuItemRow)?.Source;
        if (_selectedMenuItem is null)
        {
            return;
        }

        FillItemEditor(_selectedMenuItem);
        ShowItemEditor(EditorMode.Edit);
        LoadAddonGroups();
    }

    private void btnNewItem_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
        ShowItemEditor(EditorMode.Create);
        txtItemCode.Focus();
    }

    private void FillItemEditor(MenuItemEntity item)
    {
        txtItemCode.Text = item.ItemCode;
        cboCategory.SelectedValue = item.CategoryId;
        txtItemName.Text = item.ItemName;
        txtPrice.Text = item.Price.ToString(CultureInfo.InvariantCulture);
        txtImageUrl.Text = item.ImageUrl;
        UpdateImagePreview(item.ImageUrl);
        txtDescription.Text = item.Description;
        chkIsAvailable.IsChecked = item.IsAvailable;
        chkTrackStock.IsChecked = item.TrackStock;
        txtAvailableQuantity.Text = item.AvailableQuantity?.ToString() ?? string.Empty;
        txtItemSoldOutReason.Text = item.SoldOutReason ?? string.Empty;
        txtItemStaffNote.Text = item.StaffNote ?? string.Empty;
        cboAddonParentMenuItem.SelectedValue = item.MenuItemId;
        ApplyTrackStockUi();
    }

    private void btnChooseImage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Chọn ảnh món ăn",
            Filter = "Ảnh món ăn|*.jpg;*.jpeg;*.png;*.webp;*.bmp;*.gif|Tất cả tệp|*.*"
        };

        if (dialog.ShowDialog(Window.GetWindow(this)) != true)
        {
            return;
        }

        SetImageFromLocalFile(dialog.FileName);
    }

    private void btnClearImage_Click(object sender, RoutedEventArgs e)
    {
        txtImageUrl.Clear();
        UpdateImagePreview(null);
    }

    private async void btnPreviewImageUrl_Click(object sender, RoutedEventArgs e)
    {
        var imageReference = txtImageUrl.Text.Trim();
        if (string.IsNullOrWhiteSpace(imageReference))
        {
            MessageBoxHelper.ShowError("Hãy nhập URL ảnh trước khi xem thử.");
            txtImageUrl.Focus();
            return;
        }

        await LoadImagePreviewAsync(imageReference);
    }

    private void ImageDropZone_PreviewDragOver(object sender, DragEventArgs e)
    {
        e.Effects = TryGetSupportedDroppedImage(e.Data, out _) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void ImageDropZone_Drop(object sender, DragEventArgs e)
    {
        if (TryGetSupportedDroppedImage(e.Data, out var imagePath))
        {
            SetImageFromLocalFile(imagePath);
        }
    }

    private void SetImageFromLocalFile(string sourcePath)
    {
        try
        {
            if (!SupportedImageExtensions.Contains(Path.GetExtension(sourcePath)))
            {
                MessageBoxHelper.ShowError("Chỉ hỗ trợ ảnh JPG, PNG, WEBP, BMP hoặc GIF.");
                return;
            }

            txtImageUrl.Text = CopyImageToAppFolder(sourcePath);
            UpdateImagePreview(txtImageUrl.Text);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private static bool TryGetSupportedDroppedImage(IDataObject data, out string imagePath)
    {
        imagePath = string.Empty;
        if (!data.GetDataPresent(DataFormats.FileDrop))
        {
            return false;
        }

        var files = data.GetData(DataFormats.FileDrop) as string[];
        if (files is not { Length: > 0 })
        {
            return false;
        }

        imagePath = files[0];
        return File.Exists(imagePath) && SupportedImageExtensions.Contains(Path.GetExtension(imagePath));
    }

    private static string CopyImageToAppFolder(string sourcePath)
    {
        var imageDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DineFlow",
            "MenuImages");

        Directory.CreateDirectory(imageDirectory);
        var extension = Path.GetExtension(sourcePath);
        var safeBaseName = string.Concat(Path.GetFileNameWithoutExtension(sourcePath)
            .Where(character => !Path.GetInvalidFileNameChars().Contains(character)));
        var fileName = $"{safeBaseName}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var targetPath = Path.Combine(imageDirectory, fileName);
        File.Copy(sourcePath, targetPath, overwrite: false);
        return targetPath;
    }

    private void UpdateImagePreview(string? imagePath) => _ = LoadImagePreviewAsync(imagePath);

    private async Task<bool> LoadImagePreviewAsync(string? imagePath)
    {
        var previewVersion = ++_imagePreviewVersion;
        imgPreview.Source = null;
        txtImageEmpty.Visibility = Visibility.Visible;

        if (string.IsNullOrWhiteSpace(imagePath))
        {
            SetImageStatus("Chưa có ảnh cho món này.", "#64748B");
            return false;
        }

        try
        {
            byte[] imageBytes;
            var isExternalUrl = false;
            if (Uri.TryCreate(imagePath, UriKind.Absolute, out var absoluteUri)
                && absoluteUri.Scheme is "http" or "https")
            {
                isExternalUrl = true;
                using var response = await ImageHttpClient.GetAsync(
                    absoluteUri,
                    HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType is null || !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    throw new BusinessException("Địa chỉ này không trả về dữ liệu ảnh.");
                }

                const int maximumImageBytes = 5 * 1024 * 1024;
                if (response.Content.Headers.ContentLength is > maximumImageBytes)
                {
                    throw new BusinessException("Ảnh URL vượt quá giới hạn 5 MB.");
                }

                imageBytes = await response.Content.ReadAsByteArrayAsync();
                if (imageBytes.Length > maximumImageBytes)
                {
                    throw new BusinessException("Ảnh URL vượt quá giới hạn 5 MB.");
                }
            }
            else
            {
                if (!File.Exists(imagePath))
                {
                    SetImageStatus("Không tìm thấy ảnh. Hãy dùng URL http/https đầy đủ hoặc chọn lại tệp ảnh.", "#BE123C");
                    return false;
                }

                imageBytes = await File.ReadAllBytesAsync(Path.GetFullPath(imagePath));
            }

            if (previewVersion != _imagePreviewVersion)
            {
                return false;
            }

            using var imageStream = new MemoryStream(imageBytes, writable: false);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = imageStream;
            bitmap.EndInit();
            bitmap.Freeze();

            imgPreview.Source = bitmap;
            txtImageEmpty.Visibility = Visibility.Collapsed;
            SetImageStatus(isExternalUrl ? "Đã tải ảnh từ URL bên ngoài." : "Đã chọn ảnh từ máy.", "#15803D");
            return true;
        }
        catch (Exception ex)
        {
            if (previewVersion != _imagePreviewVersion)
            {
                return false;
            }

            imgPreview.Source = null;
            txtImageEmpty.Visibility = Visibility.Visible;
            SetImageStatus($"Không tải được ảnh: {ex.Message}", "#BE123C");
            return false;
        }
    }

    private static HttpClient CreateImageHttpClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DineFlow", "1.0"));
        return client;
    }

    private void SetImageStatus(string message, string color)
    {
        if (txtImageStatus is null)
        {
            return;
        }

        txtImageStatus.Text = message;
        txtImageStatus.Foreground = BrushFrom(color);
    }

    private void btnSaveItem_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedMenuItem is null)
        {
            btnCreate_Click(sender, e);
            return;
        }

        btnUpdate_Click(sender, e);
    }

    private void btnSaveAndConfigureChoices_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            MenuItemEntity savedItem;
            if (_selectedMenuItem is null)
            {
                savedItem = _menuItemService.Create(ReadForm(), _currentUser.Role);
            }
            else
            {
                var item = ReadForm();
                item.MenuItemId = _selectedMenuItem.MenuItemId;
                item.ItemType = _selectedMenuItem.ItemType;
                item.RowVersion = _selectedMenuItem.RowVersion;
                _menuItemService.Update(item, _currentUser.Role);
                savedItem = _menuItemService.GetById(item.MenuItemId) ?? item;
            }

            RefreshAfterMenuChange();
            _selectedMenuItem = savedItem;
            FillItemEditor(savedItem);
            ShowItemEditor(EditorMode.Edit);
            LoadAddonGroups();
            MessageBoxHelper.ShowSuccess("Đã lưu món. Thiết lập nhóm lựa chọn ngay bên dưới.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void btnCreate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _menuItemService.Create(ReadForm(), _currentUser.Role);
            RefreshAfterMenuChange();
            ClearForm();
            HideAllEditors();
            MessageBoxHelper.ShowSuccess("Menu item created.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void btnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedMenuItem is null)
        {
            MessageBoxHelper.ShowError("Hãy chọn một món ăn trước.");
            return;
        }

        try
        {
            var item = ReadForm();
            item.MenuItemId = _selectedMenuItem.MenuItemId;
            item.ItemType = _selectedMenuItem.ItemType;
            item.RowVersion = _selectedMenuItem.RowVersion;
            _menuItemService.Update(item, _currentUser.Role);
            RefreshAfterMenuChange();
            ClearForm();
            HideAllEditors();
            MessageBoxHelper.ShowSuccess("Menu item updated.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void btnSoftDelete_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedMenuItem is null)
        {
            MessageBoxHelper.ShowError("Hãy chọn món cần xóa.");
            return;
        }

        var confirmation = MessageBox.Show(
            $"Xóa vĩnh viễn món '{_selectedMenuItem.ItemName}' khỏi database?\n\nThao tác này không thể hoàn tác.",
            "Xác nhận xóa món",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);
        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            _menuItemService.SoftDelete(_selectedMenuItem.MenuItemId, _currentUser.Role);
            RefreshAfterMenuChange();
            ClearForm();
            HideAllEditors();
            MessageBoxHelper.ShowSuccess("Đã xóa món khỏi database.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void btnSelectedItemMarkSoldOut_Click(object sender, RoutedEventArgs e) => SetSelectedMenuItemAvailability(false);
    private void btnSelectedItemEnableSale_Click(object sender, RoutedEventArgs e) => SetSelectedMenuItemAvailability(true);

    private void SetSelectedMenuItemAvailability(bool isAvailable)
    {
        if (_selectedMenuItem is null)
        {
            MessageBoxHelper.ShowError("Hãy chọn một món ăn trước.");
            return;
        }

        try
        {
            _menuItemService.SetAvailability(_selectedMenuItem.MenuItemId, isAvailable, _currentUser.Role);
            RefreshAfterMenuChange();
            LoadStock();
            ClearForm();
            HideAllEditors();
            MessageBoxHelper.ShowSuccess(isAvailable ? "Đã mở bán lại món ăn." : "Đã tạm ngưng bán món ăn.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void btnClear_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
        ShowItemEditor(EditorMode.Create);
        txtItemCode.Focus();
    }

    private MenuItemEntity ReadForm()
    {
        var trackStock = chkTrackStock.IsChecked == true;
        const bool isActive = true;
        var isAvailable = chkIsAvailable.IsChecked == true;
        var quantity = trackStock && int.TryParse(txtAvailableQuantity.Text, out var parsedQuantity) ? parsedQuantity : (int?)null;
        var imageReference = string.IsNullOrWhiteSpace(txtImageUrl.Text) ? null : txtImageUrl.Text.Trim();
        ValidateImageReferenceForSave(imageReference);
        return new MenuItemEntity
        {
            ItemCode = txtItemCode.Text,
            CategoryId = cboCategory.SelectedValue is int categoryId ? categoryId : 0,
            ItemName = txtItemName.Text,
            Description = txtDescription.Text,
            Price = decimal.TryParse(txtPrice.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) ? price : -1,
            ImageUrl = imageReference,
            IsActive = isActive,
            Status = isActive ? MenuItemStatus.Active : MenuItemStatus.Inactive,
            VisibilityStatus = isActive ? VisibilityStatus.Visible : VisibilityStatus.Hidden,
            IsAvailable = isActive && isAvailable,
            AvailabilityStatus = !isActive
                ? AvailabilityStatus.TemporarilyUnavailable
                : trackStock && (quantity ?? 0) <= 0
                    ? AvailabilityStatus.SoldOut
                    : isAvailable
                        ? AvailabilityStatus.Available
                        : AvailabilityStatus.TemporarilyUnavailable,
            TrackStock = trackStock,
            CanOrderStandalone = true,
            AvailableQuantity = quantity
        };
    }

    private static void ValidateImageReferenceForSave(string? imageReference)
    {
        if (string.IsNullOrWhiteSpace(imageReference))
        {
            return;
        }

        if (imageReference.Length > 500)
        {
            throw new BusinessException("URL ảnh không được dài quá 500 ký tự.");
        }

        if (Uri.TryCreate(imageReference, UriKind.Absolute, out var uri)
            && uri.Scheme is "http" or "https")
        {
            return;
        }

        if (File.Exists(imageReference) || imageReference.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        throw new BusinessException("Ảnh phải là URL đầy đủ bắt đầu bằng http:// hoặc https://, hoặc là tệp ảnh đã chọn từ máy.");
    }

    private void ClearForm()
    {
        _selectedMenuItem = null;
        dgMenuItems.SelectedItem = null;
        cboCategory.SelectedIndex = cboCategory.Items.Count > 0 ? 0 : -1;
        txtItemName.Clear();
        txtItemCode.Clear();
        txtPrice.Text = "0";
        txtImageUrl.Clear();
        UpdateImagePreview(null);
        txtDescription.Clear();
        chkIsAvailable.IsChecked = true;
        chkTrackStock.IsChecked = false;
        txtAvailableQuantity.Clear();
        txtItemSoldOutReason.Clear();
        txtItemStaffNote.Clear();
        ApplyTrackStockUi();
        ClearAddonForm();
        dgAddonGroups.ItemsSource = null;
        dgAddonOptions.ItemsSource = null;
    }

    private void chkTrackStock_Changed(object sender, RoutedEventArgs e) => ApplyTrackStockUi();

    private void ApplyTrackStockUi()
    {
        if (txtAvailableQuantity is null || txtStockSuggestion is null)
        {
            return;
        }

        var trackStock = chkTrackStock.IsChecked == true;
        txtAvailableQuantity.IsEnabled = trackStock && _currentUser.Role == UserRole.Admin;
        txtStockSuggestion.Visibility = trackStock && txtAvailableQuantity.Text.Trim() == "0"
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void LoadAddonGroups()
    {
        if (_selectedMenuItem is null)
        {
            dgAddonGroups.ItemsSource = null;
            dgAddonOptions.ItemsSource = null;
            lstChoiceGroupLibrary.ItemsSource = null;
            lstItemChoiceGroups.ItemsSource = null;
            return;
        }

        var groupMappings = _menuItemService.GetAddonGroupMappings(_selectedMenuItem.MenuItemId);
        dgAddonGroups.ItemsSource = groupMappings;
        var assignedIds = groupMappings.Select(x => x.MenuAddonGroupId).ToHashSet();
        var displayOrderByGroup = groupMappings.ToDictionary(x => x.MenuAddonGroupId, x => x.DisplayOrder);
        var choiceRows = _choiceService.GetGroups()
            .Select(group => ChoiceGroupAssignmentRow.From(
                group,
                assignedIds.Contains(group.ChoiceGroupId),
                _currentUser.Role == UserRole.Admin))
            .OrderByDescending(x => x.IsAssigned)
            .ThenBy(x => displayOrderByGroup.GetValueOrDefault(x.Source.ChoiceGroupId, int.MaxValue))
            .ThenBy(x => x.GroupName)
            .ToList();
        lstChoiceGroupLibrary.ItemsSource = choiceRows;
        lstItemChoiceGroups.ItemsSource = choiceRows;
        dgAddonOptions.ItemsSource = null;
        _selectedAddonGroupMapping = null;
        _selectedAddonGroup = null;
        _selectedAddonOption = null;
        ApplyLinkedItemFilter();
    }

    private void LoadGlobalChoiceLibrary(int? selectedGroupId = null, int? selectedChoiceItemId = null)
    {
        var keyword = txtGlobalGroupSearch?.Text.Trim() ?? string.Empty;
        var groups = _choiceService.GetGroups()
            .Where(group => string.IsNullOrWhiteSpace(keyword)
                || SearchCompareInfo.IndexOf(group.GroupName, keyword,
                    CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0
                || SearchCompareInfo.IndexOf(
                    group.DefaultMinSelect > 0 ? "Bắt buộc" : "Không bắt buộc",
                    keyword, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)
            .OrderBy(x => x.GroupName)
            .Select(GlobalChoiceGroupRow.From)
            .ToList();
        lstGlobalChoiceGroups.ItemsSource = groups;

        var targetGroupId = selectedGroupId ?? _selectedGlobalChoiceGroup?.ChoiceGroupId;
        var selectedRow = groups.FirstOrDefault(x => x.Source.ChoiceGroupId == targetGroupId)
            ?? groups.FirstOrDefault();
        lstGlobalChoiceGroups.SelectedItem = selectedRow;

        if (selectedRow is null)
        {
            _selectedGlobalChoiceGroup = null;
            _selectedGlobalChoiceItem = null;
            txtGlobalGroupName.Text = "Chưa có nhóm lựa chọn";
            txtGlobalGroupRule.Text = "Tạo nhóm đầu tiên để thêm size, topping hoặc khẩu phần.";
            dgGlobalChoiceItems.ItemsSource = null;
            btnEditGlobalGroup.IsEnabled = false;
            btnCreateGlobalChoice.IsEnabled = false;
            btnDeleteGlobalGroup.IsEnabled = false;
            dgGlobalAppliedItems.ItemsSource = null;
            txtGlobalUsageCount.Text = "0 món";
            pnlGlobalAppliedEmpty.Visibility = Visibility.Visible;
            pnlGlobalMappingEditor.Visibility = Visibility.Collapsed;
            return;
        }

        if (selectedChoiceItemId.HasValue)
        {
            var choiceRow = dgGlobalChoiceItems.Items.OfType<GlobalChoiceItemRow>()
                .FirstOrDefault(x => x.Source.ChoiceItemId == selectedChoiceItemId.Value);
            if (choiceRow is not null)
            {
                dgGlobalChoiceItems.SelectedItem = choiceRow;
                dgGlobalChoiceItems.ScrollIntoView(choiceRow);
            }
        }
    }

    private void lstGlobalChoiceGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (lstGlobalChoiceGroups.SelectedItem is not GlobalChoiceGroupRow row)
        {
            return;
        }

        _selectedGlobalChoiceGroup = row.Source;
        _selectedGlobalChoiceItem = null;
        _selectedAddonGroup = AdaptChoiceGroup(row.Source);
        _selectedAddonOption = null;
        txtGlobalGroupName.Text = row.GroupName;
        txtGlobalGroupRule.Text = row.RuleText;
        btnEditGlobalGroup.IsEnabled = _currentUser.Role == UserRole.Admin;
        btnCreateGlobalChoice.IsEnabled = _currentUser.Role == UserRole.Admin;
        btnDeleteGlobalGroup.IsEnabled = _currentUser.Role == UserRole.Admin;
        dgGlobalChoiceItems.ItemsSource = row.Source.ChoiceItems
            .Where(x => x.IsAvailable)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.ChoiceName)
            .Select(item => GlobalChoiceItemRow.From(item, row.AppliedItemCount))
            .ToList();
        LoadGlobalAppliedMappings(row.Source);
    }

    private void LoadGlobalAppliedMappings(ChoiceGroup group)
    {
        _selectedGlobalMapping = null;
        pnlGlobalMappingEditor.Visibility = Visibility.Collapsed;
        var rows = group.MenuItems
            .Where(mapping => mapping.MenuItem is not null)
            .OrderBy(mapping => mapping.DisplayOrder)
            .ThenBy(mapping => mapping.MenuItem!.ItemName)
            .Select(mapping => AppliedMenuItemRow.From(mapping, _currentUser.Role == UserRole.Admin))
            .ToList();

        dgGlobalAppliedItems.ItemsSource = rows;
        txtGlobalUsageCount.Text = $"{rows.Count} món";
        pnlGlobalAppliedEmpty.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void dgGlobalChoiceItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgGlobalChoiceItems.SelectedItem is not GlobalChoiceItemRow row
            || _selectedGlobalChoiceGroup is null)
        {
            return;
        }

        _selectedGlobalChoiceItem = row.Source;
        _selectedAddonOption = AdaptChoiceItem(row.Source);
        _editingChoiceItemId = row.Source.ChoiceItemId;
        ShowAddonChoiceEditor();
        txtChoiceEditorMode.Text = $"Cập nhật: {row.Source.ChoiceName}";
        btnUpdateAddonOption.Content = "Lưu thay đổi";
        btnHideAddonOption.Visibility = Visibility.Visible;
        btnHideAddonOption.IsEnabled = _currentUser.Role == UserRole.Admin;
        btnNewChoiceVisible.Visibility = Visibility.Visible;

        txtLinkedItemSearch.Clear();
        lstAddonMenuItems.SelectedItem = null;
        rbLinkedChoice.IsChecked = row.Source.LinkedMenuItemId.HasValue;
        rbCustomChoice.IsChecked = !row.Source.LinkedMenuItemId.HasValue;
        txtCustomChoiceName.Text = row.Source.LinkedMenuItemId.HasValue ? string.Empty : row.Source.ChoiceName;
        foreach (var candidate in lstAddonMenuItems.Items.OfType<AddonCandidateRow>())
        {
            if (candidate.Source.MenuItemId == row.Source.LinkedMenuItemId)
            {
                lstAddonMenuItems.SelectedItem = candidate;
                break;
            }
        }

        txtExtraPriceOverride.Text = row.Source.ExtraPrice.ToString(CultureInfo.InvariantCulture);
        txtAddonOptionOrder.Text = row.Source.DisplayOrder.ToString(CultureInfo.InvariantCulture);
    }

    private void btnCreateGlobalGroup_Click(object sender, RoutedEventArgs e)
    {
        _editingGlobalGroup = true;
        _selectedGlobalChoiceGroup = null;
        ShowAddonGroupEditor(true);
        txtAddonGroupOrder.Text = "0";
    }

    private void btnEditGlobalGroup_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGlobalChoiceGroup is null)
        {
            MessageBoxHelper.ShowError("Chọn nhóm lựa chọn cần sửa.");
            return;
        }

        _editingGlobalGroup = true;
        txtAddonGroupName.Text = _selectedGlobalChoiceGroup.GroupName;
        txtAddonMinSelect.Text = _selectedGlobalChoiceGroup.DefaultMinSelect.ToString(CultureInfo.InvariantCulture);
        txtAddonMaxSelect.Text = _selectedGlobalChoiceGroup.DefaultMaxSelect.ToString(CultureInfo.InvariantCulture);
        txtAddonGroupOrder.Text = "0";
        chkAddonGroupRequired.IsChecked = _selectedGlobalChoiceGroup.DefaultMinSelect > 0;
        chkAddonGroupActive.IsChecked = _selectedGlobalChoiceGroup.IsAvailable;
        ShowAddonGroupEditor(false);
    }

    private void btnDeleteGlobalGroup_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGlobalChoiceGroup is null)
        {
            MessageBoxHelper.ShowError("Chọn nhóm lựa chọn cần xóa.");
            return;
        }

        var group = _selectedGlobalChoiceGroup;
        if (!MessageBoxHelper.ShowConfirm(
                $"Xóa vĩnh viễn nhóm “{group.GroupName}”? Nhóm sẽ được gỡ khỏi {group.MenuItems.Count} món và {group.ChoiceItems.Count} lựa chọn bên trong cũng bị xóa."))
        {
            return;
        }

        try
        {
            _choiceService.DeleteGroup(group.ChoiceGroupId, _currentUser.Role);
            _selectedGlobalChoiceGroup = null;
            _selectedGlobalChoiceItem = null;
            LoadGlobalChoiceLibrary();
            MessageBoxHelper.ShowSuccess($"Đã xóa nhóm {group.GroupName}.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void EditGlobalMapping_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not AppliedMenuItemRow row || !row.CanManage)
        {
            return;
        }

        _selectedGlobalMapping = row.Source;
        txtGlobalMappingItemName.Text = $"Áp dụng cho: {row.ItemName}";
        chkGlobalMappingRequired.IsChecked = row.Source.IsRequired;
        txtGlobalMappingMin.Text = row.Source.MinSelect.ToString(CultureInfo.InvariantCulture);
        txtGlobalMappingMax.Text = row.Source.MaxSelect.ToString(CultureInfo.InvariantCulture);
        txtGlobalMappingOrder.Text = row.Source.DisplayOrder.ToString(CultureInfo.InvariantCulture);
        pnlGlobalMappingEditor.Visibility = Visibility.Visible;
        txtGlobalMappingMin.Focus();
        txtGlobalMappingMin.SelectAll();
    }

    private void CancelGlobalMappingEdit_Click(object sender, RoutedEventArgs e)
    {
        _selectedGlobalMapping = null;
        pnlGlobalMappingEditor.Visibility = Visibility.Collapsed;
    }

    private void SaveGlobalMapping_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGlobalMapping is null || _selectedGlobalChoiceGroup is null)
        {
            return;
        }

        if (!int.TryParse(txtGlobalMappingMin.Text, out var min)
            || !int.TryParse(txtGlobalMappingMax.Text, out var max)
            || !int.TryParse(txtGlobalMappingOrder.Text, out var displayOrder)
            || min < 0 || max < 1 || min > max || displayOrder < 0)
        {
            MessageBoxHelper.ShowError("Min, max và thứ tự không hợp lệ. Yêu cầu: 0 ≤ min ≤ max, max ≥ 1 và thứ tự ≥ 0.");
            return;
        }

        var isRequired = chkGlobalMappingRequired.IsChecked == true;
        if (isRequired && min < 1)
        {
            MessageBoxHelper.ShowError("Nhóm bắt buộc phải có số lượng tối thiểu từ 1.");
            return;
        }

        try
        {
            var mapping = new MenuItemChoiceGroup
            {
                MenuItemId = _selectedGlobalMapping.MenuItemId,
                ChoiceGroupId = _selectedGlobalMapping.ChoiceGroupId,
                IsRequired = isRequired,
                MinSelect = min,
                MaxSelect = max,
                DisplayOrder = displayOrder
            };
            _choiceService.AssignGroup(mapping, _currentUser.Role);
            pnlGlobalMappingEditor.Visibility = Visibility.Collapsed;
            LoadGlobalChoiceLibrary(mapping.ChoiceGroupId);
            MessageBoxHelper.ShowSuccess("Đã cập nhật quy tắc áp dụng cho món.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void RemoveGlobalMapping_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not AppliedMenuItemRow row || !row.CanManage)
        {
            return;
        }

        if (!MessageBoxHelper.ShowConfirm($"Gỡ nhóm “{_selectedGlobalChoiceGroup?.GroupName}” khỏi món “{row.ItemName}”?"))
        {
            return;
        }

        try
        {
            _choiceService.RemoveGroup(row.Source.MenuItemId, row.Source.ChoiceGroupId, _currentUser.Role);
            LoadGlobalChoiceLibrary(row.Source.ChoiceGroupId);
            MessageBoxHelper.ShowSuccess($"Đã gỡ nhóm khỏi {row.ItemName}.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void btnCreateGlobalChoice_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGlobalChoiceGroup is null)
        {
            MessageBoxHelper.ShowError("Chọn nhóm trước khi thêm lựa chọn.");
            return;
        }

        _selectedAddonGroup = AdaptChoiceGroup(_selectedGlobalChoiceGroup);
        StartNewChoiceEditor();
        ShowAddonChoiceEditor();
        txtCustomChoiceName.Focus();
    }

    private void OpenChoiceGroup_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not ChoiceGroup group)
        {
            return;
        }

        ShowScreen(ScreenMode.Addons);
        LoadGlobalChoiceLibrary(group.ChoiceGroupId);
    }

    private void OpenNewChoiceGroup_Click(object sender, RoutedEventArgs e)
    {
        ShowScreen(ScreenMode.Addons);
        btnCreateGlobalGroup_Click(sender, e);
    }

    private static MenuAddonGroup AdaptChoiceGroup(ChoiceGroup group)
    {
        return new MenuAddonGroup
        {
            MenuAddonGroupId = group.ChoiceGroupId,
            GroupName = group.GroupName,
            IsActive = group.IsAvailable,
            Options = group.ChoiceItems.Where(x => x.IsAvailable).Select(AdaptChoiceItem).ToList()
        };
    }

    private static AddonGroupOption AdaptChoiceItem(ChoiceItem item)
    {
        return new AddonGroupOption
        {
            AddonGroupOptionId = item.ChoiceItemId,
            MenuAddonGroupId = item.ChoiceGroupId,
            MenuAddonOptionId = item.ChoiceItemId,
            ExtraPrice = item.ExtraPrice,
            DisplayOrder = item.DisplayOrder,
            IsActive = item.IsAvailable,
            MenuAddonOption = new MenuAddonOption
            {
                MenuAddonOptionId = item.ChoiceItemId,
                OptionName = item.ChoiceName,
                LinkedMenuItemId = item.LinkedMenuItemId,
                LinkedMenuItem = item.LinkedMenuItem,
                IsActive = item.IsAvailable
            }
        };
    }

    private List<MenuItemEntity> GetAddonCandidates(List<MenuItemEntity> menuItems)
    {
        return _selectedMenuItem is null || _currentScreen == ScreenMode.Addons
            ? menuItems
            : menuItems.Where(x => x.MenuItemId != _selectedMenuItem.MenuItemId).ToList();
    }

    private List<AddonCandidateRow> GetAddonCandidateRows(List<MenuItemEntity> menuItems)
    {
        return GetAddonCandidates(menuItems)
            .OrderBy(x => x.Category?.DisplayOrder ?? int.MaxValue)
            .ThenBy(x => x.ItemName)
            .Select(AddonCandidateRow.From)
            .ToList();
    }

    private void cboAddonParentMenuItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cboAddonParentMenuItem.SelectedItem is not MenuItemEntity selectedParent)
        {
            return;
        }

        if (_selectedMenuItem?.MenuItemId == selectedParent.MenuItemId)
        {
            return;
        }

        _selectedMenuItem = selectedParent;
        FillItemEditor(selectedParent);
        LoadAddonGroups();
    }

    private void dgAddonGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedAddonGroupMapping = dgAddonGroups.SelectedItem as MenuItemAddonGroup;
        _selectedAddonGroup = _selectedAddonGroupMapping?.MenuAddonGroup;
        _selectedAddonOption = null;
        if (_selectedAddonGroup is null || _selectedAddonGroupMapping is null)
        {
            dgAddonOptions.ItemsSource = null;
            return;
        }

        txtAddonGroupName.Text = _selectedAddonGroup.GroupName;
        txtAddonMinSelect.Text = _selectedAddonGroupMapping.MinSelect.ToString(CultureInfo.InvariantCulture);
        txtAddonMaxSelect.Text = _selectedAddonGroupMapping.MaxSelect.ToString(CultureInfo.InvariantCulture);
        txtAddonGroupOrder.Text = _selectedAddonGroupMapping.DisplayOrder.ToString(CultureInfo.InvariantCulture);
        chkAddonGroupActive.IsChecked = _selectedAddonGroupMapping.IsActive;
        chkAddonGroupRequired.IsChecked = _selectedAddonGroupMapping.IsRequired;
        dgAddonOptions.ItemsSource = _selectedAddonGroup.Options
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .Select(AddonOptionRow.From)
            .ToList();
        HideAllEditors();
    }

    private void ChoiceGroupAssignment_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox { Tag: ChoiceGroupAssignmentRow row } checkBox
            || _selectedMenuItem is null)
        {
            return;
        }

        try
        {
            if (checkBox.IsChecked == true)
            {
                var nextOrder = _choiceService.GetMappings(_selectedMenuItem.MenuItemId).Count + 1;
                _choiceService.AssignGroup(new MenuItemChoiceGroup
                {
                    MenuItemId = _selectedMenuItem.MenuItemId,
                    ChoiceGroupId = row.Source.ChoiceGroupId,
                    IsRequired = row.Source.DefaultMinSelect > 0,
                    MinSelect = row.Source.DefaultMinSelect,
                    MaxSelect = row.Source.DefaultMaxSelect,
                    DisplayOrder = nextOrder
                }, _currentUser.Role);
            }
            else
            {
                _choiceService.RemoveGroup(
                    _selectedMenuItem.MenuItemId,
                    row.Source.ChoiceGroupId,
                    _currentUser.Role);
            }

            LoadAddonGroups();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
            LoadAddonGroups();
        }
    }

    private void MoveChoiceGroupUp_Click(object sender, RoutedEventArgs e) => MoveChoiceGroup(sender, -1);

    private void MoveChoiceGroupDown_Click(object sender, RoutedEventArgs e) => MoveChoiceGroup(sender, 1);

    private void MoveChoiceGroup(object sender, int direction)
    {
        if ((sender as Button)?.Tag is not ChoiceGroupAssignmentRow row
            || _selectedMenuItem is null
            || !row.IsAssigned)
        {
            return;
        }

        try
        {
            var mappings = _choiceService.GetMappings(_selectedMenuItem.MenuItemId)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.ChoiceGroupId)
                .ToList();
            var currentIndex = mappings.FindIndex(x => x.ChoiceGroupId == row.Source.ChoiceGroupId);
            var targetIndex = currentIndex + direction;
            if (currentIndex < 0 || targetIndex < 0 || targetIndex >= mappings.Count)
            {
                return;
            }

            var current = mappings[currentIndex];
            var target = mappings[targetIndex];
            var currentOrder = current.DisplayOrder;
            current.DisplayOrder = target.DisplayOrder;
            target.DisplayOrder = currentOrder;
            _choiceService.AssignGroup(current, _currentUser.Role);
            _choiceService.AssignGroup(target, _currentUser.Role);
            LoadAddonGroups();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void dgAddonOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedAddonOption = (dgAddonOptions.SelectedItem as AddonOptionRow)?.Mapping;
        if (_selectedAddonOption is null)
        {
            return;
        }

        _editingChoiceItemId = _selectedAddonOption.AddonGroupOptionId;
        ShowAddonChoiceEditor();
        txtChoiceEditorMode.Text = $"Cập nhật: {_selectedAddonOption.MenuAddonOption?.OptionName}";
        btnUpdateAddonOption.Content = "Lưu thay đổi";
        btnHideAddonOption.Visibility = Visibility.Visible;
        btnHideAddonOption.IsEnabled = _currentUser.Role == UserRole.Admin;
        btnNewChoiceVisible.Visibility = Visibility.Visible;

        txtLinkedItemSearch.Clear();
        lstAddonMenuItems.SelectedItem = null;
        var linkedMenuItemId = _selectedAddonOption.MenuAddonOption?.LinkedMenuItemId;
        rbLinkedChoice.IsChecked = linkedMenuItemId.HasValue;
        rbCustomChoice.IsChecked = !linkedMenuItemId.HasValue;
        txtCustomChoiceName.Text = linkedMenuItemId.HasValue
            ? string.Empty
            : _selectedAddonOption.MenuAddonOption?.OptionName ?? string.Empty;
        foreach (var row in lstAddonMenuItems.Items.OfType<AddonCandidateRow>())
        {
            if (row.Source.MenuItemId == linkedMenuItemId)
            {
                lstAddonMenuItems.SelectedItem = row;
                break;
            }
        }

        txtExtraPriceOverride.Text = _selectedAddonOption.ExtraPrice?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        txtAddonOptionOrder.Text = _selectedAddonOption.DisplayOrder.ToString(CultureInfo.InvariantCulture);
        chkAddonOptionDefault.IsChecked = _selectedAddonOption.IsDefault;
    }

    private void btnNewAddonGroupEditor_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedMenuItem is null)
        {
            MessageBoxHelper.ShowError("Chọn món chính trước khi tạo nhóm lựa chọn.");
            return;
        }

        ShowAddonGroupEditor(true);
    }

    private void btnEditAddonGroupEditor_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedAddonGroup is null || _selectedAddonGroupMapping is null)
        {
            MessageBoxHelper.ShowError("Chọn một nhóm đã áp dụng để chỉnh sửa.");
            return;
        }

        ShowAddonGroupEditor(false);
    }

    private void btnAddAddonGroup_Click(object sender, RoutedEventArgs e)
    {
        if (_editingGlobalGroup)
        {
            try
            {
                var form = ReadAddonGroupMapping();
                var group = _choiceService.CreateGroup(new ChoiceGroup
                {
                    GroupName = txtAddonGroupName.Text,
                    DefaultMinSelect = form.MinSelect,
                    DefaultMaxSelect = form.MaxSelect,
                    IsAvailable = chkAddonGroupActive.IsChecked == true
                }, _currentUser.Role);
                _editingGlobalGroup = false;
                HideAllEditors();
                LoadGlobalChoiceLibrary(group.ChoiceGroupId);
                MessageBoxHelper.ShowSuccess("Đã tạo nhóm lựa chọn.");
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError(ex.Message);
            }
            return;
        }

        if (_selectedMenuItem is null)
        {
            MessageBoxHelper.ShowError("Hãy chọn một món ăn trước.");
            return;
        }

        try
        {
            var groupMappingForm = ReadAddonGroupMapping();
            var groupName = txtAddonGroupName.Text.Trim();
            var group = _choiceService.GetGroups()
                .FirstOrDefault(x => string.Equals(x.GroupName, groupName, StringComparison.OrdinalIgnoreCase))
                ?? _choiceService.CreateGroup(new ChoiceGroup
                {
                    GroupName = groupName,
                    DefaultMinSelect = groupMappingForm.MinSelect,
                    DefaultMaxSelect = groupMappingForm.MaxSelect,
                    IsAvailable = true
                }, _currentUser.Role);

            _choiceService.AssignGroup(new MenuItemChoiceGroup
            {
                MenuItemId = _selectedMenuItem.MenuItemId,
                ChoiceGroupId = group.ChoiceGroupId,
                MinSelect = groupMappingForm.MinSelect,
                MaxSelect = groupMappingForm.MaxSelect,
                IsRequired = groupMappingForm.IsRequired,
                DisplayOrder = groupMappingForm.DisplayOrder
            }, _currentUser.Role);
            LoadAddonGroups();
            HideAllEditors();
            MessageBoxHelper.ShowSuccess("Đã tạo và gắn nhóm lựa chọn.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void btnUpdateAddonGroup_Click(object sender, RoutedEventArgs e)
    {
        if (_editingGlobalGroup)
        {
            if (_selectedGlobalChoiceGroup is null)
            {
                MessageBoxHelper.ShowError("Chọn nhóm lựa chọn cần sửa.");
                return;
            }

            try
            {
                var form = ReadAddonGroupMapping();
                var groupId = _selectedGlobalChoiceGroup.ChoiceGroupId;
                _choiceService.UpdateGroup(new ChoiceGroup
                {
                    ChoiceGroupId = groupId,
                    GroupName = txtAddonGroupName.Text,
                    DefaultMinSelect = form.MinSelect,
                    DefaultMaxSelect = form.MaxSelect,
                    IsAvailable = chkAddonGroupActive.IsChecked == true
                }, _currentUser.Role);
                _editingGlobalGroup = false;
                HideAllEditors();
                LoadGlobalChoiceLibrary(groupId);
                MessageBoxHelper.ShowSuccess("Đã cập nhật nhóm lựa chọn.");
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError(ex.Message);
            }
            return;
        }

        if (_selectedAddonGroup is null || _selectedAddonGroupMapping is null)
        {
            MessageBoxHelper.ShowError("Hãy chọn một nhóm lựa chọn trước.");
            return;
        }

        try
        {
            var mapping = ReadAddonGroupMapping();
            _choiceService.UpdateGroup(new ChoiceGroup
            {
                ChoiceGroupId = _selectedAddonGroup.MenuAddonGroupId,
                GroupName = txtAddonGroupName.Text,
                DefaultMinSelect = mapping.MinSelect,
                DefaultMaxSelect = mapping.MaxSelect,
                IsAvailable = chkAddonGroupActive.IsChecked == true
            }, _currentUser.Role);
            _choiceService.AssignGroup(new MenuItemChoiceGroup
            {
                MenuItemId = _selectedAddonGroupMapping.MenuItemId,
                ChoiceGroupId = _selectedAddonGroupMapping.MenuAddonGroupId,
                IsRequired = mapping.IsRequired,
                MinSelect = mapping.MinSelect,
                MaxSelect = mapping.MaxSelect,
                DisplayOrder = mapping.DisplayOrder
            }, _currentUser.Role);
            LoadAddonGroups();
            HideAllEditors();
            MessageBoxHelper.ShowSuccess("Đã cập nhật nhóm lựa chọn.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void btnNewChoice_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedAddonGroup is null)
        {
            MessageBoxHelper.ShowError("Chọn một nhóm lựa chọn trước khi tạo option.");
            return;
        }

        StartNewChoiceEditor();
        ShowAddonChoiceEditor();
        txtCustomChoiceName.Focus();
    }

    private void btnSaveChoice_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedAddonGroup is null)
        {
            MessageBoxHelper.ShowError("Chọn một nhóm lựa chọn trước khi lưu option.");
            return;
        }
        if (!TryReadChoiceForm(out var choice))
        {
            return;
        }

        try
        {
            int choiceItemId;
            if (_editingChoiceItemId is int editingId)
            {
                choice.ChoiceItemId = editingId;
                _choiceService.UpdateChoiceItem(choice, _currentUser.Role);
                choiceItemId = editingId;
                MessageBoxHelper.ShowSuccess("Đã cập nhật lựa chọn.");
            }
            else
            {
                choiceItemId = _choiceService.CreateChoiceItem(choice, _currentUser.Role).ChoiceItemId;
                MessageBoxHelper.ShowSuccess("Đã thêm lựa chọn mới.");
            }

            var groupId = _selectedAddonGroup.MenuAddonGroupId;
            ReloadChoiceSelection(groupId, choiceItemId);
            LoadPricingData();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void btnHideAddonOption_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedAddonOption is null)
        {
            MessageBoxHelper.ShowError("Chọn lựa chọn cần xóa.");
            return;
        }

        var optionName = _selectedAddonOption.MenuAddonOption?.OptionName ?? "lựa chọn này";
        var confirmation = MessageBox.Show(
            $"Xóa vĩnh viễn '{optionName}' khỏi database?\n\nThao tác này không thể hoàn tác.",
            "Xác nhận xóa lựa chọn",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);
        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var groupId = _selectedAddonOption.MenuAddonGroupId;
            var choiceItemId = _selectedAddonOption.AddonGroupOptionId;
            _choiceService.DeleteChoiceItem(choiceItemId, _currentUser.Role);
            _selectedAddonOption = null;
            _selectedGlobalChoiceItem = null;
            ReloadChoiceSelection(groupId, -1);
            MessageBoxHelper.ShowSuccess("Đã xóa lựa chọn khỏi database.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private MenuItemAddonGroup ReadAddonGroupMapping()
    {
        return new MenuItemAddonGroup
        {
            MinSelect = int.TryParse(txtAddonMinSelect.Text, out var minSelect) ? minSelect : -1,
            MaxSelect = int.TryParse(txtAddonMaxSelect.Text, out var maxSelect) ? maxSelect : -1,
            DisplayOrder = int.TryParse(txtAddonGroupOrder.Text, out var displayOrder) ? displayOrder : 0,
            IsRequired = chkAddonGroupRequired.IsChecked == true,
            IsActive = chkAddonGroupActive.IsChecked == true
        };
    }

    private void AddonRequired_Changed(object sender, RoutedEventArgs e)
    {
        if (txtAddonMinSelect is null)
        {
            return;
        }

        if (chkAddonGroupRequired.IsChecked == true)
        {
            if (!int.TryParse(txtAddonMinSelect.Text, out var minSelect) || minSelect < 1)
            {
                txtAddonMinSelect.Text = "1";
            }
        }
        else
        {
            txtAddonMinSelect.Text = "0";
        }
    }

    private bool TryReadChoiceForm(out ChoiceItem choice)
    {
        choice = null!;
        if (_selectedAddonGroup is null)
        {
            MessageBoxHelper.ShowError("Chọn nhóm lựa chọn trước khi nhập option.");
            return false;
        }
        if (!TryParseCurrencyInput(txtExtraPriceOverride.Text, out var extraPrice))
        {
            MessageBoxHelper.ShowError("Giá cộng thêm phải là số không âm, ví dụ 10000 hoặc 10.000.");
            return false;
        }
        if (!int.TryParse(txtAddonOptionOrder.Text, out var displayOrder) || displayOrder < 0)
        {
            MessageBoxHelper.ShowError("Thứ tự hiển thị phải là số nguyên không âm.");
            return false;
        }

        MenuItemEntity? linkedItem = null;
        string choiceName;
        if (rbLinkedChoice.IsChecked == true)
        {
            linkedItem = (lstAddonMenuItems.SelectedItem as AddonCandidateRow)?.Source;
            if (linkedItem is null)
            {
                MessageBoxHelper.ShowError("Chọn một món có sẵn để dùng chung stock và trạng thái.");
                return false;
            }
            choiceName = linkedItem.ItemName;
        }
        else
        {
            choiceName = txtCustomChoiceName.Text.Trim();
            if (string.IsNullOrWhiteSpace(choiceName))
            {
                MessageBoxHelper.ShowError("Nhập tên lựa chọn, ví dụ: Sốt phô mai, Cay vừa hoặc Phần 3 người.");
                return false;
            }
        }

        choice = new ChoiceItem
        {
            ChoiceGroupId = _selectedAddonGroup.MenuAddonGroupId,
            ChoiceName = choiceName,
            LinkedMenuItemId = linkedItem?.MenuItemId,
            ExtraPrice = extraPrice,
            DisplayOrder = displayOrder,
            IsAvailable = true
        };
        return true;
    }

    private static bool TryParseCurrencyInput(string input, out decimal value)
    {
        var normalized = input.Trim()
            .Replace("₫", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("đ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(".", string.Empty, StringComparison.Ordinal)
            .Replace(",", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        return decimal.TryParse(normalized, NumberStyles.None, CultureInfo.InvariantCulture, out value)
               && value >= 0;
    }

    private void StartNewChoiceEditor()
    {
        _editingChoiceItemId = null;
        _selectedAddonOption = null;
        dgAddonOptions.SelectedItem = null;
        txtChoiceEditorMode.Text = _selectedAddonGroup is null
            ? "Chọn nhóm trước khi thêm lựa chọn"
            : $"Tạo lựa chọn mới trong nhóm: {_selectedAddonGroup.GroupName}";
        btnUpdateAddonOption.Content = "Lưu lựa chọn mới";
        btnHideAddonOption.Visibility = Visibility.Collapsed;
        btnNewChoiceVisible.Visibility = Visibility.Collapsed;
        rbCustomChoice.IsChecked = true;
        txtCustomChoiceName.Clear();
        txtLinkedItemSearch.Clear();
        lstAddonMenuItems.SelectedItem = null;
        txtExtraPriceOverride.Text = "0";
        txtAddonOptionOrder.Text = ((_selectedAddonGroup?.Options.Count ?? 0) + 1).ToString(CultureInfo.InvariantCulture);
    }

    private void ReloadChoiceSelection(int groupId, int choiceItemId)
    {
        if (_currentScreen == ScreenMode.Addons)
        {
            HideAllEditors();
            LoadGlobalChoiceLibrary(groupId, choiceItemId);
            return;
        }

        LoadAddonGroups();
        var groupMapping = dgAddonGroups.Items.OfType<MenuItemAddonGroup>()
            .FirstOrDefault(x => x.MenuAddonGroupId == groupId);
        if (groupMapping is null)
        {
            return;
        }

        dgAddonGroups.SelectedItem = groupMapping;
        var choiceRow = dgAddonOptions.Items.OfType<AddonOptionRow>()
            .FirstOrDefault(x => x.Mapping.AddonGroupOptionId == choiceItemId);
        if (choiceRow is not null)
        {
            dgAddonOptions.SelectedItem = choiceRow;
            dgAddonOptions.ScrollIntoView(choiceRow);
        }
    }

    private void ChoiceMode_Changed(object sender, RoutedEventArgs e)
    {
        if (pnlCustomChoice is null || pnlLinkedChoice is null)
        {
            return;
        }

        var useLinkedItem = rbLinkedChoice.IsChecked == true;
        pnlCustomChoice.Visibility = useLinkedItem ? Visibility.Collapsed : Visibility.Visible;
        pnlLinkedChoice.Visibility = useLinkedItem ? Visibility.Visible : Visibility.Collapsed;
        if (useLinkedItem && txtLinkedItemSearch is not null)
        {
            txtLinkedItemSearch.Focus();
        }
    }

    private void txtLinkedItemSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyLinkedItemFilter();

    private void ApplyLinkedItemFilter()
    {
        if (lstAddonMenuItems is null || txtLinkedItemSearch is null)
        {
            return;
        }

        var selectedMenuItemId = (lstAddonMenuItems.SelectedItem as AddonCandidateRow)?.Source.MenuItemId;
        var keyword = txtLinkedItemSearch.Text.Trim();
        var rows = GetAddonCandidateRows(_menuItemService.GetAll());
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var terms = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            rows = rows.Where(row => terms.All(term => new[] { row.ItemCode, row.ItemName, row.CategoryName }
                    .Any(value => SearchCompareInfo.IndexOf(value, term, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)))
                .ToList();
        }

        lstAddonMenuItems.ItemsSource = rows;
        if (selectedMenuItemId.HasValue)
        {
            lstAddonMenuItems.SelectedItem = rows.FirstOrDefault(x => x.Source.MenuItemId == selectedMenuItemId.Value);
        }
        UpdateLinkedItemSelectionHint();
    }

    private void lstAddonMenuItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateLinkedItemSelectionHint();
        if (_editingChoiceItemId is null
            && rbLinkedChoice.IsChecked == true
            && lstAddonMenuItems.SelectedItem is AddonCandidateRow row)
        {
            txtExtraPriceOverride.Text = row.Price.ToString("0", CultureInfo.InvariantCulture);
        }
    }

    private void UpdateLinkedItemSelectionHint()
    {
        if (txtLinkedItemSelection is null)
        {
            return;
        }

        txtLinkedItemSelection.Text = lstAddonMenuItems.SelectedItem is AddonCandidateRow row
            ? $"Đã chọn: {row.ItemCode} · {row.ItemName}"
            : "Chưa chọn món liên kết";
        txtLinkedItemSelection.Foreground = BrushFrom(
            lstAddonMenuItems.SelectedItem is null ? "#7F8995" : "#50C99A");
    }

    private void ClearAddonForm()
    {
        _selectedAddonGroup = null;
        _selectedAddonGroupMapping = null;
        _selectedAddonOption = null;
        _editingChoiceItemId = null;
        txtChoiceEditorMode.Text = "Chọn nhóm trước khi thêm lựa chọn";
        btnUpdateAddonOption.Content = "Lưu lựa chọn mới";
        btnHideAddonOption.Visibility = Visibility.Collapsed;
        btnNewChoiceVisible.Visibility = Visibility.Collapsed;
        txtAddonGroupName.Clear();
        rbCustomChoice.IsChecked = true;
        txtCustomChoiceName.Clear();
        txtAddonMinSelect.Text = "0";
        txtAddonMaxSelect.Text = "1";
        txtAddonGroupOrder.Text = "0";
        chkAddonGroupActive.IsChecked = true;
        chkAddonGroupRequired.IsChecked = false;
        lstAddonMenuItems.SelectedItem = null;
        txtExtraPriceOverride.Clear();
        txtAddonOptionOrder.Text = "0";
        chkAddonOptionDefault.IsChecked = false;
    }

    private void SelectStockItemForQuickAction(MenuItemEntity item)
    {
        _selectedStockItem = item;
        pnlStockEditor.IsEnabled = true;
        txtStockSelectedItem.Text = item.ItemName;
        txtStockSelectedMeta.Text = $"{item.ItemCode} · {GetItemTypeLabel(item.ItemType)}";
        txtStockSelectedStatus.Text = item.IsAvailable ? "Đang mở bán" : "Đang tạm ngưng";
        txtStockSelectedStatus.Foreground = BrushFrom(item.IsAvailable ? "#15803D" : "#BE123C");
        txtStockSelectedQuantity.Text = (item.AvailableQuantity ?? 0).ToString(CultureInfo.InvariantCulture);
        txtStockSelectedCategory.Text = item.Category?.CategoryName ?? "Không xác định";
        txtStockAvailableQuantity.Text = item.AvailableQuantity?.ToString() ?? string.Empty;
        txtStockSoldOutReason.Text = item.SoldOutReason ?? string.Empty;
        txtStockStaffNote.Text = item.StaffNote ?? string.Empty;
        btnStockMarkSoldOut.IsEnabled = item.IsAvailable;
        btnStockEnableSale.IsEnabled = !item.IsAvailable;
    }

    private void ClearStockEditor()
    {
        _selectedStockItem = null;
        if (pnlStockEditor is null)
        {
            return;
        }

        pnlStockEditor.IsEnabled = false;
        txtStockSelectedItem.Text = "Chọn một món trong bảng";
        txtStockSelectedMeta.Text = "Thông tin cập nhật sẽ hiển thị tại đây.";
        txtStockSelectedStatus.Text = "Chưa chọn";
        txtStockSelectedStatus.Foreground = BrushFrom("#64748B");
        txtStockSelectedQuantity.Text = "—";
        txtStockSelectedCategory.Text = "—";
        txtStockAvailableQuantity.Clear();
        txtStockSoldOutReason.Clear();
        txtStockStaffNote.Clear();
    }

    private void dgStockItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgStockItems.SelectedItem is StockCardRow row)
        {
            SelectStockItemForQuickAction(row.Source);
        }
    }

    private void btnStockQuickAdjust_Click(object sender, RoutedEventArgs e)
    {
        if (txtStockAvailableQuantity is null || sender is not Button button)
        {
            return;
        }

        if (int.TryParse(button.Tag?.ToString(), out var delta) &&
            int.TryParse(txtStockAvailableQuantity.Text, out var currentQuantity))
        {
            txtStockAvailableQuantity.Text = Math.Max(0, currentQuantity + delta).ToString(CultureInfo.InvariantCulture);
        }
    }

    private void btnStockUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelectedStockItem(out var item))
        {
            return;
        }

        if (!int.TryParse(txtStockAvailableQuantity.Text.Trim(), out var quantity) || quantity < 0)
        {
            MessageBoxHelper.ShowError("Số lượng tồn phải là số nguyên không âm.");
            txtStockAvailableQuantity.Focus();
            txtStockAvailableQuantity.SelectAll();
            return;
        }

        try
        {
            _menuItemService.UpdateStock(item.MenuItemId, quantity, txtStockStaffNote.Text, _currentUser.Role);
            LoadAllData();
            MessageBoxHelper.ShowSuccess($"Đã cập nhật tồn kho của {item.ItemName}.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void btnStockMarkSoldOut_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelectedStockItem(out var item))
        {
            return;
        }

        if (!MessageBoxHelper.ShowConfirm($"Tạm ngưng bán “{item.ItemName}”? Khách hàng sẽ không thể đặt món này."))
        {
            return;
        }

        try
        {
            _menuItemService.SetAvailability(
                item.MenuItemId,
                false,
                txtStockSoldOutReason.Text,
                txtStockStaffNote.Text,
                _currentUser.Role);
            LoadAllData();
            MessageBoxHelper.ShowSuccess($"Đã tạm ngưng bán {item.ItemName}.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private void btnStockEnableSale_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelectedStockItem(out var item))
        {
            return;
        }

        try
        {
            _menuItemService.SetAvailability(
                item.MenuItemId,
                true,
                soldOutReason: null,
                txtStockStaffNote.Text,
                _currentUser.Role);
            LoadAllData();
            MessageBoxHelper.ShowSuccess($"Đã mở bán lại {item.ItemName}.");
        }
        catch (Exception ex)
        {
            MessageBoxHelper.ShowError(ex.Message);
        }
    }

    private bool TryGetSelectedStockItem(out MenuItemEntity item)
    {
        if (_selectedStockItem is null)
        {
            MessageBoxHelper.ShowError("Hãy chọn một món trong bảng tồn kho trước.");
            item = null!;
            return false;
        }

        item = _selectedStockItem;
        return true;
    }

    private enum ScreenMode
    {
        Dashboard,
        Categories,
        MenuItems,
        Addons,
        Stock,
        Pricing
    }

    private enum EditorMode
    {
        Create,
        Edit
    }

    private sealed record BadgeRow(string BadgeText, Brush BadgeBackground, Brush BadgeForeground);

    private sealed record CategoryRow(Category Source, string CategoryName, int DisplayOrder, int ItemCount)
    {
        public static CategoryRow From(Category category, int itemCount)
        {
            return new CategoryRow(
                category,
                category.CategoryName,
                category.DisplayOrder,
                itemCount);
        }
    }

    private sealed record MenuItemRow(
        MenuItemEntity Source,
        string? ImageUrl,
        string ItemCode,
        string ItemName,
        string CategoryName,
        string PriceText,
        string StockText,
        BadgeRow StatusBadge)
    {
        public static MenuItemRow From(MenuItemEntity item)
        {
            var status = item.Status == MenuItemStatus.Deleted
                ? Badge("Đã xóa", "#E5E7EB", "#4B5563")
                : item.Status != MenuItemStatus.Active
                    ? Badge(GetMenuItemStatusLabel(item.Status), "#E5E7EB", "#4B5563")
                : item.VisibilityStatus == VisibilityStatus.Hidden || !item.IsActive
                ? Badge("Đang ẩn", "#E5E7EB", "#4B5563")
                : item.Category is { IsActive: false }
                    ? Badge("Danh mục đang ẩn", "#FEE2E2", "#B91C1C")
                    : item.AvailabilityStatus == AvailabilityStatus.SoldOut
                        ? Badge("Hết hàng", "#FFEDD5", "#C2410C")
                    : item.IsAvailable
                        ? Badge("Đang bán", "#DCFCE7", "#15803D")
                        : Badge("Tạm ngưng", "#FFEDD5", "#C2410C");

            return new MenuItemRow(
                item,
                item.ImageUrl,
                item.ItemCode,
                item.ItemName,
                item.Category?.CategoryName ?? "Chưa phân loại",
                item.Price.ToString("N0", CultureInfo.InvariantCulture),
                item.TrackStock ? (item.AvailableQuantity ?? 0).ToString(CultureInfo.InvariantCulture) : "Không theo dõi",
                status);
        }
    }

    private sealed record StockCardRow(
        MenuItemEntity Source,
        string ItemCode,
        string ItemName,
        MenuItemType ItemType,
        string TypeLabel,
        string CategoryName,
        int Quantity,
        string QuantityText,
        Brush StockForeground,
        BadgeRow StatusBadge)
    {
        public static StockCardRow From(MenuItemEntity item)
        {
            var quantity = item.AvailableQuantity ?? 0;
            var foreground = BrushFrom(quantity == 0 ? "#DC2626" : quantity <= 10 ? "#EA580C" : "#16A34A");
            return new StockCardRow(
                item,
                item.ItemCode,
                item.ItemName,
                item.ItemType,
                GetItemTypeLabel(item.ItemType),
                item.Category?.CategoryName ?? string.Empty,
                quantity,
                quantity.ToString(CultureInfo.InvariantCulture),
                foreground,
                !item.IsAvailable
                    ? Badge("Tạm ngưng", "#FEE2E2", "#B91C1C")
                    : quantity == 0
                    ? Badge("Hết hàng", "#FEE2E2", "#B91C1C")
                    : quantity <= 10
                        ? Badge("Sắp hết", "#FFEDD5", "#C2410C")
                        : Badge("Ổn định", "#DCFCE7", "#15803D"));
        }
    }

    private static string GetItemTypeLabel(MenuItemType itemType) => itemType switch
    {
        MenuItemType.Single => "Món lẻ",
        MenuItemType.Combo => "Combo",
        MenuItemType.AddonOnly => "Món thêm",
        MenuItemType.Drink => "Đồ uống",
        MenuItemType.SideDish => "Món phụ",
        _ => itemType.ToString()
    };

    private static string GetMenuItemStatusLabel(MenuItemStatus status) => status switch
    {
        MenuItemStatus.Draft => "Bản nháp",
        MenuItemStatus.Active => "Đang hoạt động",
        MenuItemStatus.Inactive => "Tạm ngưng",
        MenuItemStatus.Deleted => "Đã xóa",
        _ => status.ToString()
    };

    private sealed record AddonCandidateRow(MenuItemEntity Source, string ItemCode, string ItemName, string CategoryName, decimal Price, string? ImageUrl)
    {
        public static AddonCandidateRow From(MenuItemEntity item)
        {
            return new AddonCandidateRow(
                item,
                item.ItemCode,
                item.ItemName,
                item.Category?.CategoryName ?? string.Empty,
                item.Price,
                item.ImageUrl);
        }
    }

    private sealed record ChoiceGroupAssignmentRow(
        ChoiceGroup Source,
        string GroupName,
        string RuleText,
        int OptionCount,
        bool IsAssigned,
        bool CanManage)
    {
        public static ChoiceGroupAssignmentRow From(ChoiceGroup group, bool isAssigned, bool canManage)
        {
            var requiredText = group.DefaultMinSelect > 0 ? "Bắt buộc" : "Không bắt buộc";
            return new ChoiceGroupAssignmentRow(
                group,
                group.GroupName,
                $"{requiredText} · Min {group.DefaultMinSelect} · Max {group.DefaultMaxSelect}",
                group.ChoiceItems.Count(x => x.IsAvailable),
                isAssigned,
                canManage);
        }
    }

    private sealed record GlobalChoiceGroupRow(
        ChoiceGroup Source,
        string GroupName,
        string RuleText,
        int OptionCount,
        int AppliedItemCount)
    {
        public static GlobalChoiceGroupRow From(ChoiceGroup group)
        {
            var requiredText = group.DefaultMinSelect > 0 ? "Bắt buộc" : "Không bắt buộc";
            return new GlobalChoiceGroupRow(
                group,
                group.GroupName,
                $"{requiredText} · Chọn {group.DefaultMinSelect}–{group.DefaultMaxSelect}",
                group.ChoiceItems.Count(x => x.IsAvailable),
                group.MenuItems.Count);
        }
    }

    private sealed record AppliedMenuItemRow(
        MenuItemChoiceGroup Source,
        string ItemName,
        string ItemMeta,
        string CategoryName,
        string RuleText,
        bool CanManage)
    {
        public static AppliedMenuItemRow From(MenuItemChoiceGroup mapping, bool canManage)
        {
            var menuItem = mapping.MenuItem!;
            var requiredText = mapping.IsRequired ? "Bắt buộc" : "Không bắt buộc";
            return new AppliedMenuItemRow(
                mapping,
                menuItem.ItemName,
                $"{menuItem.ItemCode} · {menuItem.Category?.CategoryName ?? "Chưa phân loại"}",
                menuItem.Category?.CategoryName ?? "Chưa phân loại",
                $"{requiredText} · {mapping.MinSelect}–{mapping.MaxSelect}",
                canManage);
        }
    }

    private sealed record GlobalChoiceItemRow(
        ChoiceItem Source,
        string ChoiceName,
        decimal ExtraPrice,
        string LinkedItemName,
        int DisplayOrder,
        int AppliedItemCount,
        string StatusText)
    {
        public static GlobalChoiceItemRow From(ChoiceItem item, int appliedItemCount)
        {
            return new GlobalChoiceItemRow(
                item,
                item.ChoiceName,
                item.ExtraPrice,
                item.LinkedMenuItem?.ItemName ?? "—",
                item.DisplayOrder,
                appliedItemCount,
                item.IsAvailable ? "Đang dùng" : "Tạm ẩn");
        }
    }

    private sealed record AddonOptionRow(AddonGroupOption Mapping, string OptionName, decimal? ExtraPrice, bool IsActive, string StatusText, string? ImageUrl)
    {
        public static AddonOptionRow From(AddonGroupOption mapping)
        {
            return new AddonOptionRow(
                mapping,
                mapping.MenuAddonOption?.OptionName ?? string.Empty,
                mapping.ExtraPrice,
                mapping.IsActive,
                mapping.IsActive ? "Đang dùng" : "Đã ẩn",
                mapping.MenuAddonOption?.LinkedMenuItem?.ImageUrl);
        }
    }


    private sealed record PricingCategoryFilter(string Label, int? CategoryId);

    private sealed class PricingWorkspaceMenuRow
    {
        public required MenuItemEntity Source { get; init; }
        public required string ItemName { get; init; }
        public required string ItemCode { get; init; }
        public required string CategoryName { get; init; }
        public required string BasePriceText { get; init; }
        public string? ImageUrl { get; init; }
        public required List<PricingEditableChannel> EditableChannels { get; init; }
        public List<PricingEditableChannel> ChannelCells => EditableChannels;

        public static PricingWorkspaceMenuRow From(
            MenuItemEntity item,
            List<SalesChannel> channels,
            IChannelPricingService pricingService) => new()
            {
                Source = item,
                ItemName = item.ItemName,
                ItemCode = item.ItemCode,
                CategoryName = item.Category?.CategoryName ?? "Chưa phân loại",
                BasePriceText = $"{item.Price:N0} ₫",
                ImageUrl = item.ImageUrl,
                EditableChannels = channels.Select(channel => PricingEditableChannel.From(
                    channel,
                    item.Price,
                    channel.ChannelCode == "DINE_IN"
                        ? 0
                        : pricingService.GetMenuItemExtraPrice(item.MenuItemId, channel.SalesChannelId))).ToList()
            };
    }

    private sealed class PricingWorkspaceChoiceGroup
    {
        public required string GroupName { get; init; }
        public required List<PricingWorkspaceChoice> Choices { get; init; }

        public static PricingWorkspaceChoiceGroup From(
            ChoiceGroup group,
            List<SalesChannel> channels,
            IChannelPricingService pricingService) => new()
            {
                GroupName = group.GroupName,
                Choices = group.ChoiceItems
                .Where(choice => choice.IsAvailable)
                .OrderBy(choice => choice.DisplayOrder)
                .ThenBy(choice => choice.ChoiceName)
                .Select(choice => new PricingWorkspaceChoice
                {
                    Source = choice,
                    ChoiceName = choice.ChoiceName,
                    ChannelPrices = channels.Select(channel => PricingEditableChannel.From(
                        channel,
                        choice.ExtraPrice,
                        channel.ChannelCode == "DINE_IN"
                            ? 0
                            : pricingService.GetChoiceItemExtraPrice(choice.ChoiceItemId, channel.SalesChannelId))).ToList()
                })
                .ToList()
            };
    }

    private sealed class PricingWorkspaceChoice
    {
        public required ChoiceItem Source { get; init; }
        public required string ChoiceName { get; init; }
        public required List<PricingEditableChannel> ChannelPrices { get; init; }
    }

    private sealed class PricingEditableChannel : INotifyPropertyChanged
    {
        private string _extraPriceInput = "0";

        public required SalesChannel Source { get; init; }
        public required string ChannelName { get; init; }
        public required decimal BasePrice { get; init; }
        public required bool IsEditable { get; init; }

        public string ExtraPriceInput
        {
            get => _extraPriceInput;
            set
            {
                if (_extraPriceInput == value)
                {
                    return;
                }

                _extraPriceInput = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExtraPriceInput)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExtraPriceText)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FinalPriceText)));
            }
        }

        public string ExtraPriceText => TryParseCurrencyInput(ExtraPriceInput, out var value)
            ? value == 0 ? "+0" : $"+{value:N0}"
            : "—";

        public string FinalPriceText => TryParseCurrencyInput(ExtraPriceInput, out var value)
            ? $"Cuối: {BasePrice + value:N0} ₫"
            : "Giá chưa hợp lệ";

        public event PropertyChangedEventHandler? PropertyChanged;

        public static PricingEditableChannel From(SalesChannel channel, decimal basePrice, decimal extraPrice) => new()
        {
            Source = channel,
            ChannelName = channel.ChannelName,
            BasePrice = basePrice,
            IsEditable = channel.ChannelCode != "DINE_IN",
            ExtraPriceInput = extraPrice.ToString(CultureInfo.InvariantCulture)
        };
    }

    private static BadgeRow Badge(string text, string background, string foreground)
    {
        return new BadgeRow(text, BrushFrom(background), BrushFrom(foreground));
    }

    private static Brush BrushFrom(string color)
    {
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
    }
}
