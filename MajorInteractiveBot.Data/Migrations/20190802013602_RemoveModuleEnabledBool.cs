using Microsoft.EntityFrameworkCore.Migrations;

namespace MajorInteractiveBot.Data.Migrations
{
    public partial class RemoveModuleEnabledBool : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Modules",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "Modules");

            migrationBuilder.RenameTable(
                name: "Modules",
                newName: "DisabledModules");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DisabledModules",
                table: "DisabledModules",
                columns: new[] { "GuildId", "ModuleName" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DisabledModules",
                table: "DisabledModules");

            migrationBuilder.RenameTable(
                name: "DisabledModules",
                newName: "Modules");

            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "Modules",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Modules",
                table: "Modules",
                columns: new[] { "GuildId", "ModuleName" });
        }
    }
}
