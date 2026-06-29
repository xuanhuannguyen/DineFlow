using DineFlow.DataAccessObjects.DbContexts;
using DineFlow.Repositories.Auth;
using DineFlow.Repositories.Menu;
using DineFlow.Api.Security;
using DineFlow.Services.Auth;
using DineFlow.Services.Menu;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CustomerWeb", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Port=5432;Database=DineFlowDb;Username=postgres;Password=123";
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
    });
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();
builder.Services.AddScoped<IMenuAddonRepository, MenuAddonRepository>();
builder.Services.AddScoped<IMenuAddonService, MenuAddonService>();
builder.Services.AddScoped<IChoiceRepository, ChoiceRepository>();
builder.Services.AddScoped<IChoiceService, ChoiceService>();
builder.Services.AddScoped<IChannelPricingService, ChannelPricingService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<ICustomerMenuService, CustomerMenuService>();
builder.Services.AddSingleton<IApiTokenService, ApiTokenService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CustomerWeb");
app.UseAuthorization();
app.MapControllers();
app.MapHub<DineFlow.Api.Hubs.StaffHub>("/hubs/staff");
app.MapHub<DineFlow.Api.Hubs.CustomerHub>("/hubs/customer");

app.Run();
