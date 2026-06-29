using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DineFlow.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddChoiceGroupDefaultsAndMin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinSelect",
                table: "MenuItemChoiceGroups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DefaultMaxSelect",
                table: "ChoiceGroups",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "DefaultMinSelect",
                table: "ChoiceGroups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE \"MenuItemChoiceGroups\" SET \"MinSelect\" = 1 WHERE \"IsRequired\" = TRUE;");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MenuItemChoiceGroups_MinSelect",
                table: "MenuItemChoiceGroups",
                sql: "\"MinSelect\" >= 0 AND \"MinSelect\" <= \"MaxSelect\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MenuItemChoiceGroups_RequiredMinSelect",
                table: "MenuItemChoiceGroups",
                sql: "\"IsRequired\" = FALSE OR \"MinSelect\" >= 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChoiceGroups_DefaultMaxSelect",
                table: "ChoiceGroups",
                sql: "\"DefaultMaxSelect\" >= \"DefaultMinSelect\" AND \"DefaultMaxSelect\" >= 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChoiceGroups_DefaultMinSelect",
                table: "ChoiceGroups",
                sql: "\"DefaultMinSelect\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MenuItemChoiceGroups_MinSelect",
                table: "MenuItemChoiceGroups");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MenuItemChoiceGroups_RequiredMinSelect",
                table: "MenuItemChoiceGroups");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChoiceGroups_DefaultMaxSelect",
                table: "ChoiceGroups");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChoiceGroups_DefaultMinSelect",
                table: "ChoiceGroups");

            migrationBuilder.DropColumn(
                name: "MinSelect",
                table: "MenuItemChoiceGroups");

            migrationBuilder.DropColumn(
                name: "DefaultMaxSelect",
                table: "ChoiceGroups");

            migrationBuilder.DropColumn(
                name: "DefaultMinSelect",
                table: "ChoiceGroups");
        }
    }
}
