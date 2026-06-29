using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DineFlow.DataAccessObjects.DesignTime;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.example.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=DineFlowDb;Username=postgres;Password=123";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
        });

        return new AppDbContext(optionsBuilder.Options);
    }
}
