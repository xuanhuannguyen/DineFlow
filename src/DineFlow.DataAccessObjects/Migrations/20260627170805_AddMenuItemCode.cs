using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DineFlow.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuItemCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemCode",
                table: "MenuItems",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE \"MenuItems\" SET \"ItemCode\" = 'M' || \"MenuItemId\"::text WHERE \"ItemCode\" IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "MenuItems",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ItemCode",
                table: "MenuItems",
                column: "ItemCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MenuItems_ItemCode",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "ItemCode",
                table: "MenuItems");
        }
    }
}
