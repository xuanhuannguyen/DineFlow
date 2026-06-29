using DineFlow.BusinessObjects.Auth;
using DineFlow.Services.Auth;
using System.Windows;

namespace DineFlow.WPFApp.Views;

public partial class LoginWindow : Window
{
    private readonly IAuthService _authService = new AuthService();

    public LoginWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => txtUsername.Focus();
    }

    private void btnLogin_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var request = new LoginRequestDto
            {
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Password
            };

            var currentUser = _authService.Login(request);
            var mainWindow = new MainWindow(currentUser);
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(GetLoginErrorMessage(ex), "Login failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private static string GetLoginErrorMessage(Exception exception)
    {
        var rootCause = exception;
        while (rootCause.InnerException != null)
        {
            rootCause = rootCause.InnerException;
        }

        if (rootCause.Message.Contains("actively refused", StringComparison.OrdinalIgnoreCase))
        {
            return "Khong ket noi duoc PostgreSQL local o localhost:5432."
                + Environment.NewLine
                + "Hay chay scripts\\Start-LocalPostgres.ps1 truoc khi dang nhap."
                + Environment.NewLine
                + Environment.NewLine
                + $"Chi tiet: {rootCause.Message}";
        }

        if (rootCause != exception)
        {
            return $"{exception.Message}{Environment.NewLine}{Environment.NewLine}Chi tiet: {rootCause.Message}";
        }

        return exception.Message;
    }
}
