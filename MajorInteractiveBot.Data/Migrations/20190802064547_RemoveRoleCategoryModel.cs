using Microsoft.EntityFrameworkCore.Migrations;

namespace MajorInteractiveBot.Data.Migrations
{
    public partial class RemoveRoleCategoryModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleCategories");

            migrationBuilder.AddColumn<decimal>(
                name: "GuildId",
                table: "AssignableRoles",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "AssignableRoles",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "AssignableRoles");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "AssignableRoles");

            migrationBuilder.CreateTable(
                name: "RoleCategories",
                columns: table => new
                {
                    CategoryRole = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleCategories", x => x.CategoryRole);
                });
        }
    }
}
