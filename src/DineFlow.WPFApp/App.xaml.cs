using System.Windows;
using DineFlow.DataAccessObjects.DbContexts;
using DineFlow.DataAccessObjects.Tables;
using DineFlow.Repositories.Tables;
using DineFlow.Services.Tables;
using DineFlow.WPFApp.Features.Management.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DineFlow.WPFApp
{
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; } = null!;

        public App()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Register DbContext
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(context.Configuration.GetConnectionString("DefaultConnection")));

                    // Register DAO & Repositories
                    services.AddScoped<AreaDAO>();
                    services.AddScoped<DiningTableDAO>();
                    services.AddScoped<IAreaRepository, AreaRepository>();
                    services.AddScoped<IDiningTableRepository, DiningTableRepository>();

                    // Register Services
                    services.AddScoped<IAreaService, AreaService>();
                    services.AddScoped<ITableService, TableService>();
                    services.AddScoped<ITableQrService, TableQrService>();
                    services.AddScoped<ITableReadService, TableReadService>();

                    // Register UI Components
                    services.AddTransient<AreaManagementControl>();
                    services.AddTransient<TableManagementControl>();
                    services.AddTransient<TableOverviewControl>();
                    services.AddTransient<MainWindow>();
                })
                .Build();
        }

        private async void OnStartup(object sender, StartupEventArgs e)
        {
            await AppHost.StartAsync();

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private async void OnExit(object sender, ExitEventArgs e)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();
        }
    }
}
