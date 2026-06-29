using DineFlow.BusinessObjects.Auth;
using DineFlow.WPFApp.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DineFlow.WPFApp;

public partial class MainWindow : Window
{
    private readonly CurrentUserDto _currentUser;
    private readonly MenuItemManagementWindow _menuWorkspace;
    private readonly Dictionary<AppModule, Button> _navigationButtons;
    private readonly Dictionary<AppModule, ModulePreview> _modulePreviews;
    private readonly Dictionary<AppModule, ModulePlaceholderView> _moduleViews = [];

    public MainWindow(CurrentUserDto currentUser)
    {
        InitializeComponent();
        _currentUser = currentUser;
        _menuWorkspace = new MenuItemManagementWindow(_currentUser);
        _navigationButtons = CreateNavigationMap();
        _modulePreviews = CreateModulePreviews();

        txtCurrentUser.Text = _currentUser.FullName;
        txtCurrentRole.Text = _currentUser.Role.ToString();
        txtInitials.Text = BuildInitials(_currentUser.FullName);

        Navigate(AppModule.Menu);
    }

    private void btnOverview_Click(object sender, RoutedEventArgs e) => Navigate(AppModule.Overview);
    private void btnMenu_Click(object sender, RoutedEventArgs e) => Navigate(AppModule.Menu);
    private void btnOrders_Click(object sender, RoutedEventArgs e) => Navigate(AppModule.Orders);
    private void btnBills_Click(object sender, RoutedEventArgs e) => Navigate(AppModule.Bills);
    private void btnRequests_Click(object sender, RoutedEventArgs e) => Navigate(AppModule.Requests);
    private void btnReports_Click(object sender, RoutedEventArgs e) => Navigate(AppModule.Reports);
    private void btnTables_Click(object sender, RoutedEventArgs e) => Navigate(AppModule.Tables);
    private void btnAuth_Click(object sender, RoutedEventArgs e) => Navigate(AppModule.Auth);

    private void Navigate(AppModule module)
    {
        if (module == AppModule.Menu)
        {
            _menuWorkspace.NavigateTo(MenuWorkspaceSection.Overview);
            mainContent.Content = _menuWorkspace;
        }
        else
        {
            mainContent.Content = GetOrCreateModuleView(module);
        }

        ApplyNavigationSelection(module);
    }

    private ModulePlaceholderView GetOrCreateModuleView(AppModule module)
    {
        if (_moduleViews.TryGetValue(module, out var view))
        {
            return view;
        }

        view = new ModulePlaceholderView(_modulePreviews[module]);
        _moduleViews[module] = view;
        return view;
    }

    private void ApplyNavigationSelection(AppModule selectedModule)
    {
        foreach (var (module, button) in _navigationButtons)
        {
            var isSelected = module == selectedModule;
            button.Background = Brush(isSelected ? "#F0F7FF" : "Transparent");
            button.BorderBrush = Brush(isSelected ? "#D1E9FF" : "Transparent");
            button.Foreground = Brush(isSelected ? "#0866E5" : "#475569");
        }
    }

    private Dictionary<AppModule, Button> CreateNavigationMap() => new()
    {
        [AppModule.Overview] = btnOverview,
        [AppModule.Menu] = btnMenu,
        [AppModule.Orders] = btnOrders,
        [AppModule.Bills] = btnBills,
        [AppModule.Requests] = btnRequests,
        [AppModule.Reports] = btnReports,
        [AppModule.Tables] = btnTables,
        [AppModule.Auth] = btnAuth
    };

    private static Dictionary<AppModule, ModulePreview> CreateModulePreviews() => new()
    {
        [AppModule.Overview] = Preview(
            "Tổng quan", "Theo dõi nhanh tình hình vận hành nhà hàng trong ngày.", "\uE80F",
            "Đơn hôm nay", "Doanh thu", "Yêu cầu mới", "Hoạt động gần đây", "Làm mới",
            "Hoạt động", "Thời gian", "Trạng thái",
            "Màn tổng quan hiện chỉ có giao diện. Các chỉ số sẽ được cập nhật khi kết nối dữ liệu đơn hàng và doanh thu."),
        [AppModule.Orders] = Preview(
            "Đơn hàng", "Theo dõi đơn mới, tiến độ bếp và trạng thái phục vụ.", "\uE8A5",
            "Đơn mới", "Đang chế biến", "Đã hoàn tất", "Danh sách đơn hàng", "Tạo đơn",
            "Mã đơn / khách", "Tổng tiền", "Trạng thái",
            "Module Đơn hàng chưa nối service. Giao diện này dành cho luồng nhận đơn, chuyển bếp và hoàn tất phục vụ."),
        [AppModule.Bills] = Preview(
            "Hóa đơn", "Quản lý thanh toán, hóa đơn đã phát hành và đối soát ca.", "\uE8C7",
            "Chờ thanh toán", "Đã thanh toán", "Hoàn tiền", "Danh sách hóa đơn", "Xuất dữ liệu",
            "Mã hóa đơn", "Giá trị", "Thanh toán",
            "Module Hóa đơn chưa nối service. Dữ liệu thanh toán và chức năng xuất hóa đơn sẽ được bổ sung ở giai đoạn nghiệp vụ."),
        [AppModule.Requests] = Preview(
            "Yêu cầu", "Tiếp nhận yêu cầu từ khách và điều phối nhân viên xử lý.", "\uE8BD",
            "Mới", "Đang xử lý", "Đã xong", "Yêu cầu phục vụ", "Tạo yêu cầu",
            "Nội dung", "Bàn", "Trạng thái",
            "Module Yêu cầu chưa nối service. Sau khi kết nối, khu vực này sẽ hiển thị yêu cầu gọi món, hỗ trợ và thanh toán."),
        [AppModule.Reports] = Preview(
            "Báo cáo", "Tổng hợp doanh thu, hiệu suất món và hoạt động theo thời gian.", "\uE9D2",
            "Doanh thu", "Giá trị đơn TB", "Món bán chạy", "Báo cáo gần đây", "Xuất báo cáo",
            "Báo cáo", "Khoảng thời gian", "Cập nhật",
            "Module Báo cáo chưa nối dữ liệu. Bộ lọc thời gian, biểu đồ và chức năng xuất file sẽ được triển khai sau."),
        [AppModule.Tables] = Preview(
            "Sơ đồ bàn", "Quản lý sơ đồ bàn, khu vực và trạng thái sử dụng tại quán.", "\uE80A",
            "Tổng số bàn", "Đang sử dụng", "Còn trống", "Danh sách bàn", "Thêm bàn",
            "Tên bàn / khu vực", "Sức chứa", "Trạng thái",
            "Module Sơ đồ bàn chưa nối service. Giao diện đã chuẩn bị cho quản lý khu vực, bàn và sức chứa."),
        [AppModule.Auth] = Preview(
            "Nhân viên & Phân quyền", "Quản lý tài khoản nhân viên, vai trò và quyền truy cập.", "\uE72E",
            "Tài khoản", "Đang hoạt động", "Vai trò", "Người dùng và phân quyền", "Thêm tài khoản",
            "Người dùng", "Vai trò", "Trạng thái",
            "Module Nhân viên & Phân quyền hiện chỉ có giao diện quản trị. Việc tạo tài khoản và phân quyền chưa được kết nối tại màn hình này.")
    };

    private static ModulePreview Preview(
        string title,
        string subtitle,
        string icon,
        string metricOne,
        string metricTwo,
        string metricThree,
        string sectionTitle,
        string actionLabel,
        string columnOne,
        string columnTwo,
        string columnThree,
        string emptyMessage) =>
        new(title, subtitle, icon, metricOne, metricTwo, metricThree, sectionTitle, actionLabel,
            columnOne, columnTwo, columnThree, emptyMessage);

    private void btnLogout_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Đăng xuất khỏi DineFlow?", "DineFlow",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            return;
        }

        var loginWindow = new LoginWindow();
        Application.Current.MainWindow = loginWindow;
        loginWindow.Show();
        Close();
    }

    private static string BuildInitials(string value)
    {
        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(words.Take(2).Select(x => char.ToUpperInvariant(x[0])));
    }

    private static Brush Brush(string value) =>
        new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));

    private enum AppModule
    {
        Overview,
        Menu,
        Orders,
        Bills,
        Requests,
        Reports,
        Tables,
        Auth
    }
}
