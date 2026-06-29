using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Bills;
using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;
using DineFlow.BusinessObjects.Orders;
using DineFlow.BusinessObjects.Requests;
using DineFlow.BusinessObjects.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DineFlow.DataAccessObjects.DbContexts;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<DiningTable> DiningTables => Set<DiningTable>();
    public DbSet<TableSession> TableSessions => Set<TableSession>();
    public DbSet<TableSessionCustomer> TableSessionCustomers => Set<TableSessionCustomer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<ChoiceGroup> ChoiceGroups => Set<ChoiceGroup>();
    public DbSet<ChoiceItem> ChoiceItems => Set<ChoiceItem>();
    public DbSet<MenuItemChoiceGroup> MenuItemChoiceGroups => Set<MenuItemChoiceGroup>();
    public DbSet<SalesChannel> SalesChannels => Set<SalesChannel>();
    public DbSet<MenuItemChannelPrice> MenuItemChannelPrices => Set<MenuItemChannelPrice>();
    public DbSet<ChoiceItemChannelPrice> ChoiceItemChannelPrices => Set<ChoiceItemChannelPrice>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemSelectedChoice> OrderItemSelectedChoices => Set<OrderItemSelectedChoice>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillDetail> BillDetails => Set<BillDetail>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.example.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=DineFlowDb;Username=postgres;Password=123";

        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureTables(modelBuilder);
        ConfigureOptimizedMenu(modelBuilder);
        ConfigureOptimizedOrders(modelBuilder);
        ConfigureRequests(modelBuilder);
        ConfigureBills(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.UserId);
            entity.HasIndex(x => x.Username).IsUnique();
            entity.Property(x => x.Username).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        });
    }

    private static void ConfigureTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiningTable>(entity =>
        {
            entity.HasKey(x => x.TableId);
            entity.HasIndex(x => x.QrToken).IsUnique();
            entity.Property(x => x.TableName).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Area).HasMaxLength(50);
            entity.Property(x => x.QrToken).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<TableSession>(entity =>
        {
            entity.HasKey(x => x.TableSessionId);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.HasOne(x => x.Table)
                .WithMany(x => x.TableSessions)
                .HasForeignKey(x => x.TableId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OpenedByUser)
                .WithMany()
                .HasForeignKey(x => x.OpenedBy)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ClosedByUser)
                .WithMany()
                .HasForeignKey(x => x.ClosedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.TableId)
                .IsUnique()
                .HasFilter("\"Status\" IN ('Open', 'WaitingPayment')")
                .HasDatabaseName("UX_TableSessions_OneOpenSessionPerTable");
        });

        modelBuilder.Entity<TableSessionCustomer>(entity =>
        {
            entity.HasKey(x => x.SessionCustomerId);
            entity.Property(x => x.ClientToken).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(50);
            entity.HasIndex(x => new { x.TableSessionId, x.ClientToken }).IsUnique();
            entity.HasOne(x => x.TableSession)
                .WithMany(x => x.Customers)
                .HasForeignKey(x => x.TableSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureOptimizedMenu(ModelBuilder modelBuilder)
    {
        // Legacy menu entities are intentionally excluded. The optimized model uses
        // ChoiceGroup/ChoiceItem for every size, topping and side-dish selection.
        modelBuilder.Ignore<MenuItemImage>();
        modelBuilder.Ignore<MenuItemVariant>();
        modelBuilder.Ignore<MenuAddonGroup>();
        modelBuilder.Ignore<MenuAddonOption>();
        modelBuilder.Ignore<MenuItemAddonGroup>();
        modelBuilder.Ignore<AddonGroupOption>();
        modelBuilder.Ignore<ComboGroup>();
        modelBuilder.Ignore<ComboGroupItem>();
        modelBuilder.Ignore<MenuItemAvailabilitySchedule>();
        modelBuilder.Ignore<MenuItemPriceHistory>();
        modelBuilder.Ignore<KitchenStation>();
        modelBuilder.Ignore<StockTransaction>();
        modelBuilder.Ignore<MenuAuditLog>();

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("MenuCategories");
            entity.HasKey(x => x.CategoryId);
            entity.Property(x => x.CategoryName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.DisplayOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.HasIndex(x => x.CategoryName).IsUnique();
            entity.HasIndex(x => new { x.IsActive, x.DisplayOrder });
            entity.Ignore(x => x.RestaurantId);
            entity.Ignore(x => x.ImageUrl);
            entity.Ignore(x => x.CreatedBy);
            entity.Ignore(x => x.UpdatedBy);
            entity.ToTable(t => t.HasCheckConstraint("CK_MenuCategories_DisplayOrder", "\"DisplayOrder\" >= 0"));
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.ToTable("MenuItems");
            entity.HasKey(x => x.MenuItemId);
            entity.Property(x => x.ItemCode).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.BasePrice).HasPrecision(18, 2);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
            entity.Property(x => x.ItemType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.IsAvailable).HasDefaultValue(true);
            entity.Property(x => x.CanOrderStandalone).HasDefaultValue(true);
            entity.Property(x => x.TrackStock).HasDefaultValue(false);
            entity.Property(x => x.VisibilityStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.DisplayOrder).HasDefaultValue(0);
            entity.HasIndex(x => new { x.CategoryId, x.Status, x.VisibilityStatus, x.DisplayOrder });
            entity.HasIndex(x => x.ItemCode).IsUnique();
            entity.HasOne(x => x.Category).WithMany(x => x.MenuItems).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(x => x.RestaurantId);
            entity.Ignore(x => x.KitchenStationId);
            entity.Ignore(x => x.Slug);
            entity.Ignore(x => x.ShortName);
            entity.Ignore(x => x.Currency);
            entity.Ignore(x => x.IsActive);
            entity.Ignore(x => x.HiddenReason);
            entity.Ignore(x => x.HiddenAt);
            entity.Ignore(x => x.AvailabilityStatus);
            entity.Ignore(x => x.UnavailableReason);
            entity.Ignore(x => x.ReservedQuantity);
            entity.Ignore(x => x.LowStockThreshold);
            entity.Ignore(x => x.SoldOutReason);
            entity.Ignore(x => x.SoldOutAt);
            entity.Ignore(x => x.PreparationTimeMinutes);
            entity.Ignore(x => x.SpicyLevel);
            entity.Ignore(x => x.Calories);
            entity.Ignore(x => x.AllergenNote);
            entity.Ignore(x => x.IsFeatured);
            entity.Ignore(x => x.CreatedBy);
            entity.Ignore(x => x.UpdatedBy);
            entity.Ignore(x => x.StaffNote);
            entity.Ignore(x => x.RowVersion);

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_MenuItems_BasePrice", "\"BasePrice\" >= 0");
                t.HasCheckConstraint("CK_MenuItems_TrackedStock", "\"TrackStock\" = FALSE OR \"AvailableQuantity\" IS NOT NULL");
                t.HasCheckConstraint("CK_MenuItems_StockNonNegative", "\"TrackStock\" = FALSE OR \"AvailableQuantity\" >= 0");
                t.HasCheckConstraint("CK_MenuItems_AddonOnlyNotStandalone", "\"ItemType\" <> 'AddonOnly' OR \"CanOrderStandalone\" = FALSE");
                t.HasCheckConstraint("CK_MenuItems_DisplayOrder", "\"DisplayOrder\" >= 0");
            });
        });

        modelBuilder.Entity<ChoiceGroup>(entity =>
        {
            entity.ToTable("ChoiceGroups");
            entity.HasKey(x => x.ChoiceGroupId);
            entity.Property(x => x.GroupName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.DefaultMinSelect).HasDefaultValue(0);
            entity.Property(x => x.DefaultMaxSelect).HasDefaultValue(1);
            entity.Property(x => x.IsAvailable).HasDefaultValue(true);
            entity.HasIndex(x => x.GroupName).IsUnique();
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_ChoiceGroups_DefaultMinSelect", "\"DefaultMinSelect\" >= 0");
                t.HasCheckConstraint("CK_ChoiceGroups_DefaultMaxSelect", "\"DefaultMaxSelect\" >= \"DefaultMinSelect\" AND \"DefaultMaxSelect\" >= 1");
            });
        });

        modelBuilder.Entity<MenuItemChoiceGroup>(entity =>
        {
            entity.ToTable("MenuItemChoiceGroups");
            entity.HasKey(x => new { x.MenuItemId, x.ChoiceGroupId });
            entity.Property(x => x.MinSelect).HasDefaultValue(0);
            entity.Property(x => x.MaxSelect).HasDefaultValue(1);
            entity.Property(x => x.DisplayOrder).HasDefaultValue(0);
            entity.HasOne(x => x.MenuItem).WithMany(x => x.ChoiceGroups).HasForeignKey(x => x.MenuItemId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ChoiceGroup).WithMany(x => x.MenuItems).HasForeignKey(x => x.ChoiceGroupId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.MenuItemId, x.DisplayOrder });
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_MenuItemChoiceGroups_MaxSelect", "\"MaxSelect\" >= 1");
                t.HasCheckConstraint("CK_MenuItemChoiceGroups_MinSelect", "\"MinSelect\" >= 0 AND \"MinSelect\" <= \"MaxSelect\"");
                t.HasCheckConstraint("CK_MenuItemChoiceGroups_RequiredMinSelect", "\"IsRequired\" = FALSE OR \"MinSelect\" >= 1");
                t.HasCheckConstraint("CK_MenuItemChoiceGroups_DisplayOrder", "\"DisplayOrder\" >= 0");
            });
        });

        modelBuilder.Entity<ChoiceItem>(entity =>
        {
            entity.ToTable("ChoiceItems");
            entity.HasKey(x => x.ChoiceItemId);
            entity.Property(x => x.ChoiceName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ExtraPrice).HasPrecision(18, 2);
            entity.Property(x => x.IsAvailable).HasDefaultValue(true);
            entity.Property(x => x.DisplayOrder).HasDefaultValue(0);
            entity.HasIndex(x => new { x.ChoiceGroupId, x.ChoiceName }).IsUnique();
            entity.HasIndex(x => new { x.ChoiceGroupId, x.DisplayOrder });
            entity.HasOne(x => x.ChoiceGroup).WithMany(x => x.ChoiceItems).HasForeignKey(x => x.ChoiceGroupId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.LinkedMenuItem).WithMany(x => x.UsedAsLinkedChoiceItems).HasForeignKey(x => x.LinkedMenuItemId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_ChoiceItems_ExtraPrice", "\"ExtraPrice\" >= 0");
                t.HasCheckConstraint("CK_ChoiceItems_DisplayOrder", "\"DisplayOrder\" >= 0");
            });
        });

        modelBuilder.Entity<SalesChannel>(entity =>
        {
            entity.ToTable("SalesChannels");
            entity.HasKey(x => x.SalesChannelId);
            entity.Property(x => x.ChannelCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ChannelName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.HasIndex(x => x.ChannelCode).IsUnique();
        });

        modelBuilder.Entity<MenuItemChannelPrice>(entity =>
        {
            entity.ToTable("MenuItemChannelPrices");
            entity.HasKey(x => new { x.MenuItemId, x.SalesChannelId });
            entity.Property(x => x.ChannelExtraPrice).HasPrecision(18, 2);
            entity.HasOne(x => x.MenuItem).WithMany(x => x.ChannelPrices).HasForeignKey(x => x.MenuItemId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.SalesChannel).WithMany(x => x.MenuItemPrices).HasForeignKey(x => x.SalesChannelId).OnDelete(DeleteBehavior.Cascade);
            entity.ToTable(t => t.HasCheckConstraint("CK_MenuItemChannelPrices_ExtraPrice", "\"ChannelExtraPrice\" >= 0"));
        });

        modelBuilder.Entity<ChoiceItemChannelPrice>(entity =>
        {
            entity.ToTable("ChoiceItemChannelPrices");
            entity.HasKey(x => new { x.ChoiceItemId, x.SalesChannelId });
            entity.Property(x => x.ChannelExtraPrice).HasPrecision(18, 2);
            entity.HasOne(x => x.ChoiceItem).WithMany(x => x.ChannelPrices).HasForeignKey(x => x.ChoiceItemId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.SalesChannel).WithMany(x => x.ChoiceItemPrices).HasForeignKey(x => x.SalesChannelId).OnDelete(DeleteBehavior.Cascade);
            entity.ToTable(t => t.HasCheckConstraint("CK_ChoiceItemChannelPrices_ExtraPrice", "\"ChannelExtraPrice\" >= 0"));
        });
    }

    private static void ConfigureOptimizedOrders(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<OrderItemModifier>();

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(x => x.OrderId);
            entity.Property(x => x.OrderCode).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ExternalOrderCode).HasMaxLength(100);
            entity.Property(x => x.OrderSource).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.PrintStatus).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.ClientToken).HasMaxLength(100);
            entity.Property(x => x.CustomerNote).HasMaxLength(500);
            entity.Property(x => x.SystemNote).HasMaxLength(500);
            entity.Property(x => x.CancelReason).HasMaxLength(500);
            entity.Property(x => x.PrintError).HasMaxLength(1000);
            entity.HasIndex(x => x.OrderCode).IsUnique();
            entity.HasIndex(x => x.ExternalOrderCode).HasFilter("\"ExternalOrderCode\" IS NOT NULL");
            entity.HasOne(x => x.SalesChannel).WithMany().HasForeignKey(x => x.SalesChannelId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TableSession).WithMany(x => x.Orders).HasForeignKey(x => x.TableSessionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SessionCustomer).WithMany().HasForeignKey(x => x.SessionCustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CancelledByUser).WithMany().HasForeignKey(x => x.CancelledBy).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(x => x.OrderItemId);
            entity.Property(x => x.MenuItemNameSnapshot).HasMaxLength(200).IsRequired();
            entity.Property(x => x.BasePriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.ChannelExtraPriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.FinalUnitPriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.TotalPrice).HasPrecision(18, 2);
            entity.Property(x => x.Note).HasMaxLength(500);
            entity.HasOne(x => x.Order).WithMany(x => x.OrderItems).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.MenuItem).WithMany().HasForeignKey(x => x.MenuItemId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SessionCustomer).WithMany().HasForeignKey(x => x.SessionCustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_OrderItems_Quantity", "\"Quantity\" > 0");
                t.HasCheckConstraint("CK_OrderItems_Prices", "\"BasePriceSnapshot\" >= 0 AND \"ChannelExtraPriceSnapshot\" >= 0 AND \"FinalUnitPriceSnapshot\" >= 0");
            });
        });

        modelBuilder.Entity<OrderItemSelectedChoice>(entity =>
        {
            entity.ToTable("OrderItemSelectedChoices");
            entity.HasKey(x => x.OrderItemSelectedChoiceId);
            entity.Property(x => x.GroupNameSnapshot).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ChoiceNameSnapshot).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ExtraPriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.ChannelExtraPriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.FinalExtraPriceSnapshot).HasPrecision(18, 2);
            entity.HasOne(x => x.OrderItem).WithMany(x => x.SelectedChoices).HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ChoiceGroup).WithMany().HasForeignKey(x => x.ChoiceGroupId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ChoiceItem).WithMany().HasForeignKey(x => x.ChoiceItemId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_OrderItemSelectedChoices_Quantity", "\"Quantity\" > 0");
                t.HasCheckConstraint("CK_OrderItemSelectedChoices_Prices", "\"ExtraPriceSnapshot\" >= 0 AND \"ChannelExtraPriceSnapshot\" >= 0 AND \"FinalExtraPriceSnapshot\" >= 0");
            });
        });
    }

    private static void ConfigureMenu(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.CategoryId);
            entity.Property(x => x.RestaurantId).HasDefaultValue(1);
            entity.Property(x => x.CategoryName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.HasIndex(x => new { x.RestaurantId, x.CategoryName }).IsUnique();
            entity.HasIndex(x => new { x.DisplayOrder, x.IsActive });
            entity.ToTable(x => x.HasCheckConstraint("CK_Categories_DisplayOrder", "\"DisplayOrder\" >= 0"));
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(x => x.MenuItemId);
            entity.Property(x => x.RestaurantId).HasDefaultValue(1);
            entity.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(220);
            entity.Property(x => x.ShortName).HasMaxLength(100);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.Currency).HasMaxLength(10);
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
            entity.Property(x => x.ItemType).HasConversion<string>().HasMaxLength(30).HasDefaultValue(MenuItemType.Single).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).HasDefaultValue(MenuItemStatus.Active).IsRequired();
            entity.Property(x => x.VisibilityStatus).HasConversion<string>().HasMaxLength(30).HasDefaultValue(VisibilityStatus.Visible).IsRequired();
            entity.Property(x => x.HiddenReason).HasMaxLength(500);
            entity.Property(x => x.AvailabilityStatus).HasConversion<string>().HasMaxLength(30).HasDefaultValue(AvailabilityStatus.Available).IsRequired();
            entity.Property(x => x.UnavailableReason).HasMaxLength(500);
            entity.Property(x => x.SoldOutReason).HasMaxLength(500);
            entity.Property(x => x.StaffNote).HasMaxLength(500);
            entity.Property(x => x.AllergenNote).HasMaxLength(500);
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.IsAvailable).HasDefaultValue(true);
            entity.Property(x => x.TrackStock).HasDefaultValue(false);
            entity.Property(x => x.CanOrderStandalone).HasDefaultValue(true);
            entity.Property(x => x.RowVersion).HasColumnType("bytea").IsConcurrencyToken();
            entity.HasIndex(x => new { x.RestaurantId, x.ItemName }).IsUnique();
            entity.HasIndex(x => new { x.RestaurantId, x.CategoryId, x.Status, x.VisibilityStatus, x.IsAvailable });
            entity.HasIndex(x => new { x.Status, x.VisibilityStatus, x.IsAvailable, x.CanOrderStandalone });
            entity.HasIndex(x => new { x.TrackStock, x.AvailableQuantity });
            entity.HasOne(x => x.Category)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.KitchenStation)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(x => x.KitchenStationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(x =>
            {
                x.HasCheckConstraint("CK_MenuItems_Price", "\"Price\" >= 0");
                x.HasCheckConstraint("CK_MenuItems_StockRequired", "\"TrackStock\" = FALSE OR \"AvailableQuantity\" IS NOT NULL");
                x.HasCheckConstraint("CK_MenuItems_StockNonNegative", "\"TrackStock\" = FALSE OR \"AvailableQuantity\" >= 0");
                x.HasCheckConstraint("CK_MenuItems_ZeroStockUnavailable", "\"TrackStock\" = FALSE OR \"AvailableQuantity\" > 0 OR \"IsAvailable\" = FALSE");
                x.HasCheckConstraint("CK_MenuItems_AddonOnlyNotStandalone", "\"ItemType\" <> 'AddonOnly' OR \"CanOrderStandalone\" = FALSE");
                x.HasCheckConstraint("CK_MenuItems_LowStockThreshold", "\"LowStockThreshold\" IS NULL OR \"LowStockThreshold\" >= 0");
                x.HasCheckConstraint("CK_MenuItems_ReservedQuantity", "\"ReservedQuantity\" >= 0");
                x.HasCheckConstraint("CK_MenuItems_DisplayOrder", "\"DisplayOrder\" >= 0");
            });
        });

        modelBuilder.Entity<MenuAddonGroup>(entity =>
        {
            entity.HasKey(x => x.MenuAddonGroupId);
            entity.Property(x => x.RestaurantId).HasDefaultValue(1);
            entity.Property(x => x.GroupName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.HasIndex(x => new { x.RestaurantId, x.GroupName }).IsUnique();
            entity.HasIndex(x => new { x.IsActive, x.DisplayOrder });
            entity.ToTable(x =>
            {
                x.HasCheckConstraint("CK_MenuAddonGroups_DisplayOrder", "\"DisplayOrder\" >= 0");
            });
        });

        modelBuilder.Entity<MenuAddonOption>(entity =>
        {
            entity.HasKey(x => x.MenuAddonOptionId);
            entity.Property(x => x.OptionName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.PriceApplyType).HasConversion<string>().HasMaxLength(30).HasDefaultValue(PriceApplyType.PerParentItem).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.HasIndex(x => x.OptionName).IsUnique();
            entity.HasIndex(x => x.LinkedMenuItemId)
                .IsUnique()
                .HasFilter("\"LinkedMenuItemId\" IS NOT NULL");
            entity.HasOne(x => x.LinkedMenuItem)
                .WithMany(x => x.UsedAsLinkedAddonOptions)
                .HasForeignKey(x => x.LinkedMenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MenuItemAddonGroup>(entity =>
        {
            entity.HasKey(x => x.MenuItemAddonGroupId);
            entity.Property(x => x.IsRequired).HasDefaultValue(false);
            entity.Property(x => x.MinSelect).HasDefaultValue(0);
            entity.Property(x => x.MaxSelect).HasDefaultValue(1);
            entity.Property(x => x.AllowDuplicateOption).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.HasIndex(x => new { x.MenuItemId, x.MenuAddonGroupId }).IsUnique();
            entity.HasIndex(x => new { x.MenuItemId, x.IsActive, x.DisplayOrder });
            entity.HasOne(x => x.MenuItem)
                .WithMany(x => x.AddonGroups)
                .HasForeignKey(x => x.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MenuAddonGroup)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(x => x.MenuAddonGroupId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(x =>
            {
                x.HasCheckConstraint("CK_MenuItemAddonGroups_DisplayOrder", "\"DisplayOrder\" >= 0");
                x.HasCheckConstraint("CK_MenuItemAddonGroups_MinSelect", "\"MinSelect\" >= 0");
                x.HasCheckConstraint("CK_MenuItemAddonGroups_MaxSelect", "\"MaxSelect\" >= \"MinSelect\" AND \"MaxSelect\" >= 1");
                x.HasCheckConstraint("CK_MenuItemAddonGroups_RequiredMinSelect", "\"IsRequired\" = FALSE OR \"MinSelect\" >= 1");
            });
        });

        modelBuilder.Entity<AddonGroupOption>(entity =>
        {
            entity.HasKey(x => x.AddonGroupOptionId);
            entity.Property(x => x.ExtraPrice).HasPrecision(18, 2);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.AllowMultiple).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.HasIndex(x => new { x.MenuAddonGroupId, x.MenuAddonOptionId }).IsUnique();
            entity.HasIndex(x => new { x.MenuAddonGroupId, x.IsActive, x.DisplayOrder });
            entity.HasIndex(x => x.MenuAddonGroupId)
                .IsUnique()
                .HasFilter("\"IsDefault\" = TRUE AND \"IsActive\" = TRUE");
            entity.HasOne(x => x.MenuAddonGroup)
                .WithMany(x => x.Options)
                .HasForeignKey(x => x.MenuAddonGroupId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MenuAddonOption)
                .WithMany(x => x.Groups)
                .HasForeignKey(x => x.MenuAddonOptionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(x =>
            {
                x.HasCheckConstraint("CK_AddonGroupOptions_ExtraPrice", "\"ExtraPrice\" IS NULL OR \"ExtraPrice\" >= 0");
                x.HasCheckConstraint("CK_AddonGroupOptions_DisplayOrder", "\"DisplayOrder\" >= 0");
                x.HasCheckConstraint("CK_AddonGroupOptions_MaxQuantityPerOption", "\"MaxQuantityPerOption\" IS NULL OR \"MaxQuantityPerOption\" > 0");
            });
        });

        modelBuilder.Entity<MenuItemImage>(entity =>
        {
            entity.HasKey(x => x.MenuItemImageId);
            entity.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(x => x.AltText).HasMaxLength(300);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.HasIndex(x => new { x.MenuItemId, x.IsPrimary })
                .IsUnique()
                .HasFilter("\"IsPrimary\" = TRUE");
            entity.HasIndex(x => new { x.MenuItemId, x.DisplayOrder });
            entity.HasOne(x => x.MenuItem)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.ToTable(x => x.HasCheckConstraint("CK_MenuItemImages_DisplayOrder", "\"DisplayOrder\" >= 0"));
        });

        modelBuilder.Entity<MenuItemVariant>(entity =>
        {
            entity.HasKey(x => x.MenuItemVariantId);
            entity.Property(x => x.VariantName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.Property(x => x.AvailabilityStatus).HasConversion<string>().HasMaxLength(30).HasDefaultValue(AvailabilityStatus.Available).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).HasDefaultValue(MenuItemStatus.Active).IsRequired();
            entity.Property(x => x.RowVersion).HasColumnType("bytea").IsConcurrencyToken();
            entity.HasIndex(x => new { x.MenuItemId, x.VariantName }).IsUnique();
            entity.HasOne(x => x.MenuItem)
                .WithMany(x => x.Variants)
                .HasForeignKey(x => x.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.ToTable(x =>
            {
                x.HasCheckConstraint("CK_MenuItemVariants_Price", "\"Price\" >= 0");
                x.HasCheckConstraint("CK_MenuItemVariants_StockRequired", "\"TrackStock\" = FALSE OR \"AvailableQuantity\" IS NOT NULL");
                x.HasCheckConstraint("CK_MenuItemVariants_StockNonNegative", "\"TrackStock\" = FALSE OR \"AvailableQuantity\" >= 0");
                x.HasCheckConstraint("CK_MenuItemVariants_DisplayOrder", "\"DisplayOrder\" >= 0");
            });
        });

        modelBuilder.Entity<KitchenStation>(entity =>
        {
            entity.HasKey(x => x.KitchenStationId);
            entity.Property(x => x.RestaurantId).HasDefaultValue(1);
            entity.Property(x => x.StationName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.HasIndex(x => new { x.RestaurantId, x.StationName }).IsUnique();
            entity.ToTable(x => x.HasCheckConstraint("CK_KitchenStations_DisplayOrder", "\"DisplayOrder\" >= 0"));
        });

        modelBuilder.Entity<ComboGroup>(entity =>
        {
            entity.HasKey(x => x.ComboGroupId);
            entity.Property(x => x.GroupName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.HasIndex(x => new { x.ComboMenuItemId, x.GroupName }).IsUnique();
            entity.HasOne(x => x.ComboMenuItem)
                .WithMany(x => x.ComboGroups)
                .HasForeignKey(x => x.ComboMenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(x =>
            {
                x.HasCheckConstraint("CK_ComboGroups_DisplayOrder", "\"DisplayOrder\" >= 0");
                x.HasCheckConstraint("CK_ComboGroups_MaxSelect", "\"MaxSelect\" >= \"MinSelect\" AND \"MinSelect\" >= 0");
                x.HasCheckConstraint("CK_ComboGroups_RequiredMinSelect", "\"IsRequired\" = FALSE OR \"MinSelect\" >= 1");
            });
        });

        modelBuilder.Entity<ComboGroupItem>(entity =>
        {
            entity.HasKey(x => x.ComboGroupItemId);
            entity.Property(x => x.ExtraPrice).HasPrecision(18, 2);
            entity.Property(x => x.IsAvailable).HasDefaultValue(true);
            entity.HasIndex(x => new { x.ComboGroupId, x.MenuItemId, x.MenuItemVariantId }).IsUnique();
            entity.HasOne(x => x.ComboGroup)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.ComboGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.MenuItem)
                .WithMany(x => x.UsedInComboGroups)
                .HasForeignKey(x => x.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MenuItemVariant)
                .WithMany()
                .HasForeignKey(x => x.MenuItemVariantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(x =>
            {
                x.HasCheckConstraint("CK_ComboGroupItems_ExtraPrice", "\"ExtraPrice\" >= 0");
                x.HasCheckConstraint("CK_ComboGroupItems_DisplayOrder", "\"DisplayOrder\" >= 0");
            });
        });

        modelBuilder.Entity<MenuItemAvailabilitySchedule>(entity =>
        {
            entity.HasKey(x => x.MenuItemAvailabilityScheduleId);
            entity.Property(x => x.DayOfWeek).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.Note).HasMaxLength(300);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.HasIndex(x => new { x.MenuItemId, x.DayOfWeek, x.IsActive });
            entity.HasOne(x => x.MenuItem)
                .WithMany(x => x.AvailabilitySchedules)
                .HasForeignKey(x => x.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.ToTable(x => x.HasCheckConstraint("CK_MenuItemAvailabilitySchedules_Time", "\"StartTime\" < \"EndTime\""));
        });

        modelBuilder.Entity<MenuItemPriceHistory>(entity =>
        {
            entity.HasKey(x => x.MenuItemPriceHistoryId);
            entity.Property(x => x.RestaurantId).HasDefaultValue(1);
            entity.Property(x => x.OldPrice).HasPrecision(18, 2);
            entity.Property(x => x.NewPrice).HasPrecision(18, 2);
            entity.Property(x => x.ChangeType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(500);
            entity.HasIndex(x => new { x.MenuItemId, x.EffectiveFrom });
            entity.HasOne(x => x.MenuItem)
                .WithMany(x => x.PriceHistories)
                .HasForeignKey(x => x.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MenuItemVariant)
                .WithMany()
                .HasForeignKey(x => x.MenuItemVariantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(x => x.HasCheckConstraint("CK_MenuItemPriceHistories_NewPrice", "\"NewPrice\" >= 0"));
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(x => x.StockTransactionId);
            entity.Property(x => x.RestaurantId).HasDefaultValue(1);
            entity.Property(x => x.ChangeType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(500);
            entity.Property(x => x.ReferenceType).HasMaxLength(50);
            entity.HasIndex(x => new { x.MenuItemId, x.CreatedAt });
            entity.HasOne(x => x.MenuItem)
                .WithMany(x => x.StockTransactions)
                .HasForeignKey(x => x.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MenuItemVariant)
                .WithMany()
                .HasForeignKey(x => x.MenuItemVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MenuAuditLog>(entity =>
        {
            entity.HasKey(x => x.MenuAuditLogId);
            entity.Property(x => x.RestaurantId).HasDefaultValue(1);
            entity.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ActionType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(500);
            entity.HasIndex(x => new { x.RestaurantId, x.EntityName, x.EntityId, x.CreatedAt });
            entity.HasOne(x => x.MenuItem)
                .WithMany(x => x.AuditLogs)
                .HasForeignKey(x => x.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureOrders(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(x => x.OrderId);
            entity.HasIndex(x => x.OrderCode).IsUnique();
            entity.HasIndex(x => new { x.TableSessionId, x.CreatedAt });
            entity.HasIndex(x => new { x.PrintStatus, x.CreatedAt });
            entity.Property(x => x.OrderCode).HasMaxLength(30).IsRequired();
            entity.Property(x => x.OrderSource).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.PrintStatus).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.ClientToken).HasMaxLength(100);
            entity.Property(x => x.CustomerNote).HasMaxLength(500);
            entity.Property(x => x.SystemNote).HasMaxLength(500);
            entity.Property(x => x.CancelReason).HasMaxLength(500);
            entity.Property(x => x.PrintError).HasMaxLength(1000);
            entity.HasOne(x => x.TableSession)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.TableSessionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SessionCustomer)
                .WithMany()
                .HasForeignKey(x => x.SessionCustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(x => x.OrderItemId);
            entity.Property(x => x.ItemName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.TotalPrice).HasPrecision(18, 2);
            entity.Property(x => x.Note).HasMaxLength(500);
            entity.HasIndex(x => new { x.OrderId, x.SessionCustomerId });
            entity.HasOne(x => x.Order)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.MenuItem)
                .WithMany()
                .HasForeignKey(x => x.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SessionCustomer)
                .WithMany()
                .HasForeignKey(x => x.SessionCustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItemModifier>(entity =>
        {
            entity.HasKey(x => x.OrderItemModifierId);
            entity.Property(x => x.AddonGroupNameSnapshot).HasMaxLength(100).IsRequired();
            entity.Property(x => x.AddonOptionNameSnapshot).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ExtraPriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.TotalPrice).HasPrecision(18, 2);
            entity.HasIndex(x => x.OrderItemId);
            entity.HasIndex(x => x.AddonGroupOptionId);
            entity.HasIndex(x => x.LinkedMenuItemId);
            entity.HasOne(x => x.OrderItem)
                .WithMany(x => x.Modifiers)
                .HasForeignKey(x => x.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.MenuAddonGroup)
                .WithMany()
                .HasForeignKey(x => x.MenuAddonGroupId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MenuAddonOption)
                .WithMany()
                .HasForeignKey(x => x.MenuAddonOptionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AddonGroupOption)
                .WithMany()
                .HasForeignKey(x => x.AddonGroupOptionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LinkedMenuItem)
                .WithMany()
                .HasForeignKey(x => x.LinkedMenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(x =>
            {
                x.HasCheckConstraint("CK_OrderItemModifiers_ExtraPriceSnapshot", "\"ExtraPriceSnapshot\" >= 0");
                x.HasCheckConstraint("CK_OrderItemModifiers_Quantity", "\"Quantity\" > 0");
            });
        });
    }

    private static void ConfigureRequests(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(x => x.RequestId);
            entity.Property(x => x.RequestType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.ClientToken).HasMaxLength(100);
            entity.Property(x => x.Reason).HasMaxLength(255);
            entity.Property(x => x.Message).HasMaxLength(500);
            entity.HasIndex(x => new { x.TableSessionId, x.Status });
            entity.HasIndex(x => new { x.RequestType, x.Status });
            entity.HasOne(x => x.TableSession)
                .WithMany(x => x.ServiceRequests)
                .HasForeignKey(x => x.TableSessionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SessionCustomer)
                .WithMany()
                .HasForeignKey(x => x.SessionCustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ConfirmedByUser)
                .WithMany()
                .HasForeignKey(x => x.ConfirmedBy)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CompletedByUser)
                .WithMany()
                .HasForeignKey(x => x.CompletedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBills(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(x => x.BillId);
            entity.HasIndex(x => x.BillCode).IsUnique();
            entity.HasIndex(x => new { x.TableSessionId, x.BillNo })
                .IsUnique()
                .HasFilter("\"Status\" <> 'Cancelled'")
                .HasDatabaseName("UX_Bills_TableSessionId_BillNo_Active");
            entity.HasIndex(x => x.TableSessionId)
                .IsUnique()
                .HasFilter("\"Status\" = 'Unpaid' AND \"IsDefault\" = TRUE")
                .HasDatabaseName("UX_Bills_OneDefaultUnpaidBillPerSession");
            entity.Property(x => x.BillCode).HasMaxLength(30).IsRequired();
            entity.Property(x => x.BillName).HasMaxLength(100);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.SubTotal).HasPrecision(18, 2);
            entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            entity.Property(x => x.FinalAmount).HasPrecision(18, 2);
            entity.HasOne(x => x.TableSession)
                .WithMany(x => x.Bills)
                .HasForeignKey(x => x.TableSessionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BillDetail>(entity =>
        {
            entity.HasKey(x => x.BillDetailId);
            entity.Property(x => x.CustomerDisplayName).HasMaxLength(50);
            entity.Property(x => x.ItemName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.TotalPrice).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.BillId, x.SessionCustomerId });
            entity.HasIndex(x => x.OrderItemId);
            entity.HasOne(x => x.Bill)
                .WithMany(x => x.BillDetails)
                .HasForeignKey(x => x.BillId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.OrderItem)
                .WithMany()
                .HasForeignKey(x => x.OrderItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MenuItem)
                .WithMany()
                .HasForeignKey(x => x.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SessionCustomer)
                .WithMany()
                .HasForeignKey(x => x.SessionCustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(x => x.PaymentId);
            entity.HasIndex(x => x.BillId).IsUnique();
            entity.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.ChangeReason).HasMaxLength(500);
            entity.HasOne(x => x.Bill)
                .WithOne(x => x.Payment)
                .HasForeignKey<Payment>(x => x.BillId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ConfirmedByUser)
                .WithMany()
                .HasForeignKey(x => x.ConfirmedBy)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.UpdatedByUser)
                .WithMany()
                .HasForeignKey(x => x.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
