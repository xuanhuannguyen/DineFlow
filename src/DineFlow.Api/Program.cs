using DineFlow.DataAccessObjects.DbContexts;
using DineFlow.DataAccessObjects.Tables;
using DineFlow.Repositories.Tables;
using DineFlow.Services.Tables;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection for Table Module (Member 2)
builder.Services.AddScoped<AreaDAO>();
builder.Services.AddScoped<DiningTableDAO>();

builder.Services.AddScoped<IAreaRepository, AreaRepository>();
builder.Services.AddScoped<IDiningTableRepository, DiningTableRepository>();

builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<ITableQrService, TableQrService>();
builder.Services.AddScoped<ITableReadService, TableReadService>();
builder.Services.AddScoped<ITableStatusPort, TableStatusPort>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
