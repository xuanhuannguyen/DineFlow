using DineFlow.BusinessObjects.Tables;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.DbContexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ── DbSets ────────────────────────────────────────────────────────────
        public DbSet<Area> Areas => Set<Area>();
        public DbSet<DiningTable> DiningTables => Set<DiningTable>();

        // TODO: Other members add their DbSets here following the same pattern.
        // e.g. Member 4: public DbSet<TableSession> TableSessions => Set<TableSession>();

        // ── Model configuration (Fluent API — NO Data Annotations on entities) ─
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureArea(modelBuilder);
            ConfigureDiningTable(modelBuilder);
        }

        // ── Areas ─────────────────────────────────────────────────────────────
        private static void ConfigureArea(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Area>(entity =>
            {
                entity.ToTable("Areas");

                entity.HasKey(e => e.AreaId);

                entity.Property(e => e.AreaId)
                      .UseIdentityByDefaultColumn();

                entity.Property(e => e.AreaName)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.Description)
                      .HasMaxLength(200)
                      .IsRequired(false);

                entity.Property(e => e.IsActive)
                      .IsRequired()
                      .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("NOW()");

                entity.Property(e => e.UpdatedAt)
                      .IsRequired(false);

                // UNIQUE: AreaName
                entity.HasIndex(e => e.AreaName)
                      .IsUnique()
                      .HasDatabaseName("IX_Areas_AreaName");

                entity.HasIndex(e => e.IsActive)
                      .HasDatabaseName("IX_Areas_IsActive");
            });
        }

        // ── DiningTables ──────────────────────────────────────────────────────
        private static void ConfigureDiningTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DiningTable>(entity =>
            {
                entity.ToTable("DiningTables");

                entity.HasKey(e => e.TableId);

                entity.Property(e => e.TableId)
                      .UseIdentityByDefaultColumn();

                entity.Property(e => e.TableName)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.AreaId)
                      .IsRequired();

                entity.Property(e => e.QrToken)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.Status)
                      .HasMaxLength(30)
                      .IsRequired()
                      .HasDefaultValue("Available");

                entity.Property(e => e.IsActive)
                      .IsRequired()
                      .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("NOW()");

                entity.Property(e => e.UpdatedAt)
                      .IsRequired(false);

                // FK: DiningTables.AreaId → Areas.AreaId (RESTRICT)
                entity.HasOne(e => e.Area)
                      .WithMany(a => a.Tables)
                      .HasForeignKey(e => e.AreaId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_DiningTables_Areas");

                // UNIQUE: QrToken
                entity.HasIndex(e => e.QrToken)
                      .IsUnique()
                      .HasDatabaseName("IX_DiningTables_QrToken");

                // UNIQUE: AreaId + TableName (không trùng tên bàn trong cùng khu vực)
                entity.HasIndex(e => new { e.AreaId, e.TableName })
                      .IsUnique()
                      .HasDatabaseName("IX_DiningTables_AreaId_TableName");

                // Performance indexes
                entity.HasIndex(e => e.Status)
                      .HasDatabaseName("IX_DiningTables_Status");

                entity.HasIndex(e => e.IsActive)
                      .HasDatabaseName("IX_DiningTables_IsActive");

                entity.HasIndex(e => new { e.AreaId, e.Status })
                      .HasDatabaseName("IX_DiningTables_AreaId_Status");

                // CHECK: Status IN ('Available', 'Occupied', 'WaitingPayment')
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_DiningTables_Status",
                    "\"Status\" IN ('Available', 'Occupied', 'WaitingPayment')"));
            });
        }
    }
}
