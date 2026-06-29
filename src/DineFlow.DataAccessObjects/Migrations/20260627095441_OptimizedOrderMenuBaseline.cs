using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DineFlow.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class OptimizedOrderMenuBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChoiceGroups",
                columns: table => new
                {
                    ChoiceGroupId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChoiceGroups", x => x.ChoiceGroupId);
                });

            migrationBuilder.CreateTable(
                name: "DiningTables",
                columns: table => new
                {
                    TableId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Area = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    QrToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiningTables", x => x.TableId);
                });

            migrationBuilder.CreateTable(
                name: "MenuCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuCategories", x => x.CategoryId);
                    table.CheckConstraint("CK_MenuCategories_DisplayOrder", "\"DisplayOrder\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "SalesChannels",
                columns: table => new
                {
                    SalesChannelId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChannelName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesChannels", x => x.SalesChannelId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    MenuItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BasePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CanOrderStandalone = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ItemType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VisibilityStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TrackStock = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AvailableQuantity = table.Column<int>(type: "integer", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.MenuItemId);
                    table.CheckConstraint("CK_MenuItems_AddonOnlyNotStandalone", "\"ItemType\" <> 'AddonOnly' OR \"CanOrderStandalone\" = FALSE");
                    table.CheckConstraint("CK_MenuItems_BasePrice", "\"BasePrice\" >= 0");
                    table.CheckConstraint("CK_MenuItems_DisplayOrder", "\"DisplayOrder\" >= 0");
                    table.CheckConstraint("CK_MenuItems_StockNonNegative", "\"TrackStock\" = FALSE OR \"AvailableQuantity\" >= 0");
                    table.CheckConstraint("CK_MenuItems_TrackedStock", "\"TrackStock\" = FALSE OR \"AvailableQuantity\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_MenuItems_MenuCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "MenuCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TableSessions",
                columns: table => new
                {
                    TableSessionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableId = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OpenedBy = table.Column<int>(type: "integer", nullable: true),
                    ClosedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSessions", x => x.TableSessionId);
                    table.ForeignKey(
                        name: "FK_TableSessions_DiningTables_TableId",
                        column: x => x.TableId,
                        principalTable: "DiningTables",
                        principalColumn: "TableId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TableSessions_Users_ClosedBy",
                        column: x => x.ClosedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TableSessions_Users_OpenedBy",
                        column: x => x.OpenedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChoiceItems",
                columns: table => new
                {
                    ChoiceItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChoiceGroupId = table.Column<int>(type: "integer", nullable: false),
                    ChoiceName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ExtraPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LinkedMenuItemId = table.Column<int>(type: "integer", nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChoiceItems", x => x.ChoiceItemId);
                    table.CheckConstraint("CK_ChoiceItems_DisplayOrder", "\"DisplayOrder\" >= 0");
                    table.CheckConstraint("CK_ChoiceItems_ExtraPrice", "\"ExtraPrice\" >= 0");
                    table.ForeignKey(
                        name: "FK_ChoiceItems_ChoiceGroups_ChoiceGroupId",
                        column: x => x.ChoiceGroupId,
                        principalTable: "ChoiceGroups",
                        principalColumn: "ChoiceGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChoiceItems_MenuItems_LinkedMenuItemId",
                        column: x => x.LinkedMenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "MenuItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemChannelPrices",
                columns: table => new
                {
                    MenuItemId = table.Column<int>(type: "integer", nullable: false),
                    SalesChannelId = table.Column<int>(type: "integer", nullable: false),
                    ChannelExtraPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemChannelPrices", x => new { x.MenuItemId, x.SalesChannelId });
                    table.CheckConstraint("CK_MenuItemChannelPrices_ExtraPrice", "\"ChannelExtraPrice\" >= 0");
                    table.ForeignKey(
                        name: "FK_MenuItemChannelPrices_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "MenuItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuItemChannelPrices_SalesChannels_SalesChannelId",
                        column: x => x.SalesChannelId,
                        principalTable: "SalesChannels",
                        principalColumn: "SalesChannelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemChoiceGroups",
                columns: table => new
                {
                    MenuItemId = table.Column<int>(type: "integer", nullable: false),
                    ChoiceGroupId = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    MaxSelect = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemChoiceGroups", x => new { x.MenuItemId, x.ChoiceGroupId });
                    table.CheckConstraint("CK_MenuItemChoiceGroups_DisplayOrder", "\"DisplayOrder\" >= 0");
                    table.CheckConstraint("CK_MenuItemChoiceGroups_MaxSelect", "\"MaxSelect\" >= 1");
                    table.ForeignKey(
                        name: "FK_MenuItemChoiceGroups_ChoiceGroups_ChoiceGroupId",
                        column: x => x.ChoiceGroupId,
                        principalTable: "ChoiceGroups",
                        principalColumn: "ChoiceGroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuItemChoiceGroups_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "MenuItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bills",
                columns: table => new
                {
                    BillId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableSessionId = table.Column<int>(type: "integer", nullable: false),
                    BillCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BillNo = table.Column<int>(type: "integer", nullable: false),
                    BillName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CancelledBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bills", x => x.BillId);
                    table.ForeignKey(
                        name: "FK_Bills_TableSessions_TableSessionId",
                        column: x => x.TableSessionId,
                        principalTable: "TableSessions",
                        principalColumn: "TableSessionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bills_Users_CancelledBy",
                        column: x => x.CancelledBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bills_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TableSessionCustomers",
                columns: table => new
                {
                    SessionCustomerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableSessionId = table.Column<int>(type: "integer", nullable: false),
                    ClientToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSessionCustomers", x => x.SessionCustomerId);
                    table.ForeignKey(
                        name: "FK_TableSessionCustomers_TableSessions_TableSessionId",
                        column: x => x.TableSessionId,
                        principalTable: "TableSessions",
                        principalColumn: "TableSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChoiceItemChannelPrices",
                columns: table => new
                {
                    ChoiceItemId = table.Column<int>(type: "integer", nullable: false),
                    SalesChannelId = table.Column<int>(type: "integer", nullable: false),
                    ChannelExtraPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChoiceItemChannelPrices", x => new { x.ChoiceItemId, x.SalesChannelId });
                    table.CheckConstraint("CK_ChoiceItemChannelPrices_ExtraPrice", "\"ChannelExtraPrice\" >= 0");
                    table.ForeignKey(
                        name: "FK_ChoiceItemChannelPrices_ChoiceItems_ChoiceItemId",
                        column: x => x.ChoiceItemId,
                        principalTable: "ChoiceItems",
                        principalColumn: "ChoiceItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChoiceItemChannelPrices_SalesChannels_SalesChannelId",
                        column: x => x.SalesChannelId,
                        principalTable: "SalesChannels",
                        principalColumn: "SalesChannelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillId = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    ChangeReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "BillId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Users_ConfirmedBy",
                        column: x => x.ConfirmedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesChannelId = table.Column<int>(type: "integer", nullable: false),
                    ExternalOrderCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TableSessionId = table.Column<int>(type: "integer", nullable: false),
                    SessionCustomerId = table.Column<int>(type: "integer", nullable: true),
                    OrderCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OrderSource = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ClientToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PrintStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CustomerNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SystemNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PrintedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PrintError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PrintRetryCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CancelledBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_SalesChannels_SalesChannelId",
                        column: x => x.SalesChannelId,
                        principalTable: "SalesChannels",
                        principalColumn: "SalesChannelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_TableSessionCustomers_SessionCustomerId",
                        column: x => x.SessionCustomerId,
                        principalTable: "TableSessionCustomers",
                        principalColumn: "SessionCustomerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_TableSessions_TableSessionId",
                        column: x => x.TableSessionId,
                        principalTable: "TableSessions",
                        principalColumn: "TableSessionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Users_CancelledBy",
                        column: x => x.CancelledBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableSessionId = table.Column<int>(type: "integer", nullable: false),
                    SessionCustomerId = table.Column<int>(type: "integer", nullable: true),
                    ClientToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RequestType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConfirmedBy = table.Column<int>(type: "integer", nullable: true),
                    CompletedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_TableSessionCustomers_SessionCustomerId",
                        column: x => x.SessionCustomerId,
                        principalTable: "TableSessionCustomers",
                        principalColumn: "SessionCustomerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_TableSessions_TableSessionId",
                        column: x => x.TableSessionId,
                        principalTable: "TableSessions",
                        principalColumn: "TableSessionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Users_CompletedBy",
                        column: x => x.CompletedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Users_ConfirmedBy",
                        column: x => x.ConfirmedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    OrderItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    MenuItemId = table.Column<int>(type: "integer", nullable: false),
                    SessionCustomerId = table.Column<int>(type: "integer", nullable: true),
                    MenuItemNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    BasePriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ChannelExtraPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalUnitPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.OrderItemId);
                    table.CheckConstraint("CK_OrderItems_Prices", "\"BasePriceSnapshot\" >= 0 AND \"ChannelExtraPriceSnapshot\" >= 0 AND \"FinalUnitPriceSnapshot\" >= 0");
                    table.CheckConstraint("CK_OrderItems_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_OrderItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "MenuItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_TableSessionCustomers_SessionCustomerId",
                        column: x => x.SessionCustomerId,
                        principalTable: "TableSessionCustomers",
                        principalColumn: "SessionCustomerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BillDetails",
                columns: table => new
                {
                    BillDetailId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillId = table.Column<int>(type: "integer", nullable: false),
                    OrderItemId = table.Column<int>(type: "integer", nullable: false),
                    MenuItemId = table.Column<int>(type: "integer", nullable: false),
                    SessionCustomerId = table.Column<int>(type: "integer", nullable: true),
                    CustomerDisplayName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillDetails", x => x.BillDetailId);
                    table.ForeignKey(
                        name: "FK_BillDetails_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "BillId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillDetails_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "MenuItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillDetails_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "OrderItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillDetails_TableSessionCustomers_SessionCustomerId",
                        column: x => x.SessionCustomerId,
                        principalTable: "TableSessionCustomers",
                        principalColumn: "SessionCustomerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemSelectedChoices",
                columns: table => new
                {
                    OrderItemSelectedChoiceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderItemId = table.Column<int>(type: "integer", nullable: false),
                    ChoiceGroupId = table.Column<int>(type: "integer", nullable: false),
                    ChoiceItemId = table.Column<int>(type: "integer", nullable: false),
                    GroupNameSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ChoiceNameSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ExtraPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ChannelExtraPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalExtraPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemSelectedChoices", x => x.OrderItemSelectedChoiceId);
                    table.CheckConstraint("CK_OrderItemSelectedChoices_Prices", "\"ExtraPriceSnapshot\" >= 0 AND \"ChannelExtraPriceSnapshot\" >= 0 AND \"FinalExtraPriceSnapshot\" >= 0");
                    table.CheckConstraint("CK_OrderItemSelectedChoices_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_OrderItemSelectedChoices_ChoiceGroups_ChoiceGroupId",
                        column: x => x.ChoiceGroupId,
                        principalTable: "ChoiceGroups",
                        principalColumn: "ChoiceGroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemSelectedChoices_ChoiceItems_ChoiceItemId",
                        column: x => x.ChoiceItemId,
                        principalTable: "ChoiceItems",
                        principalColumn: "ChoiceItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemSelectedChoices_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "OrderItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillDetails_BillId_SessionCustomerId",
                table: "BillDetails",
                columns: new[] { "BillId", "SessionCustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_BillDetails_MenuItemId",
                table: "BillDetails",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BillDetails_OrderItemId",
                table: "BillDetails",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BillDetails_SessionCustomerId",
                table: "BillDetails",
                column: "SessionCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_BillCode",
                table: "Bills",
                column: "BillCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bills_CancelledBy",
                table: "Bills",
                column: "CancelledBy");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_CreatedBy",
                table: "Bills",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "UX_Bills_OneDefaultUnpaidBillPerSession",
                table: "Bills",
                column: "TableSessionId",
                unique: true,
                filter: "\"Status\" = 'Unpaid' AND \"IsDefault\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "UX_Bills_TableSessionId_BillNo_Active",
                table: "Bills",
                columns: new[] { "TableSessionId", "BillNo" },
                unique: true,
                filter: "\"Status\" <> 'Cancelled'");

            migrationBuilder.CreateIndex(
                name: "IX_ChoiceGroups_GroupName",
                table: "ChoiceGroups",
                column: "GroupName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChoiceItemChannelPrices_SalesChannelId",
                table: "ChoiceItemChannelPrices",
                column: "SalesChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoiceItems_ChoiceGroupId_ChoiceName",
                table: "ChoiceItems",
                columns: new[] { "ChoiceGroupId", "ChoiceName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChoiceItems_ChoiceGroupId_DisplayOrder",
                table: "ChoiceItems",
                columns: new[] { "ChoiceGroupId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ChoiceItems_LinkedMenuItemId",
                table: "ChoiceItems",
                column: "LinkedMenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DiningTables_QrToken",
                table: "DiningTables",
                column: "QrToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuCategories_CategoryName",
                table: "MenuCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuCategories_IsActive_DisplayOrder",
                table: "MenuCategories",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemChannelPrices_SalesChannelId",
                table: "MenuItemChannelPrices",
                column: "SalesChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemChoiceGroups_ChoiceGroupId",
                table: "MenuItemChoiceGroups",
                column: "ChoiceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemChoiceGroups_MenuItemId_DisplayOrder",
                table: "MenuItemChoiceGroups",
                columns: new[] { "MenuItemId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_CategoryId_Status_VisibilityStatus_DisplayOrder",
                table: "MenuItems",
                columns: new[] { "CategoryId", "Status", "VisibilityStatus", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MenuItemId",
                table: "OrderItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_SessionCustomerId",
                table: "OrderItems",
                column: "SessionCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemSelectedChoices_ChoiceGroupId",
                table: "OrderItemSelectedChoices",
                column: "ChoiceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemSelectedChoices_ChoiceItemId",
                table: "OrderItemSelectedChoices",
                column: "ChoiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemSelectedChoices_OrderItemId",
                table: "OrderItemSelectedChoices",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CancelledBy",
                table: "Orders",
                column: "CancelledBy");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedBy",
                table: "Orders",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ExternalOrderCode",
                table: "Orders",
                column: "ExternalOrderCode",
                filter: "\"ExternalOrderCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SalesChannelId",
                table: "Orders",
                column: "SalesChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SessionCustomerId",
                table: "Orders",
                column: "SessionCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TableSessionId",
                table: "Orders",
                column: "TableSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BillId",
                table: "Payments",
                column: "BillId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ConfirmedBy",
                table: "Payments",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UpdatedBy",
                table: "Payments",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SalesChannels_ChannelCode",
                table: "SalesChannels",
                column: "ChannelCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_CompletedBy",
                table: "ServiceRequests",
                column: "CompletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ConfirmedBy",
                table: "ServiceRequests",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_RequestType_Status",
                table: "ServiceRequests",
                columns: new[] { "RequestType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_SessionCustomerId",
                table: "ServiceRequests",
                column: "SessionCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_TableSessionId_Status",
                table: "ServiceRequests",
                columns: new[] { "TableSessionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TableSessionCustomers_TableSessionId_ClientToken",
                table: "TableSessionCustomers",
                columns: new[] { "TableSessionId", "ClientToken" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSessions_ClosedBy",
                table: "TableSessions",
                column: "ClosedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TableSessions_OpenedBy",
                table: "TableSessions",
                column: "OpenedBy");

            migrationBuilder.CreateIndex(
                name: "UX_TableSessions_OneOpenSessionPerTable",
                table: "TableSessions",
                column: "TableId",
                unique: true,
                filter: "\"Status\" IN ('Open', 'WaitingPayment')");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillDetails");

            migrationBuilder.DropTable(
                name: "ChoiceItemChannelPrices");

            migrationBuilder.DropTable(
                name: "MenuItemChannelPrices");

            migrationBuilder.DropTable(
                name: "MenuItemChoiceGroups");

            migrationBuilder.DropTable(
                name: "OrderItemSelectedChoices");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "ChoiceItems");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Bills");

            migrationBuilder.DropTable(
                name: "ChoiceGroups");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "MenuCategories");

            migrationBuilder.DropTable(
                name: "SalesChannels");

            migrationBuilder.DropTable(
                name: "TableSessionCustomers");

            migrationBuilder.DropTable(
                name: "TableSessions");

            migrationBuilder.DropTable(
                name: "DiningTables");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
