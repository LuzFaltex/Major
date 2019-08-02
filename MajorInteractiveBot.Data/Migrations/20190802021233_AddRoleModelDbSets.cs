using Microsoft.EntityFrameworkCore.Migrations;

namespace MajorInteractiveBot.Data.Migrations
{
    public partial class AddRoleModelDbSets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssignableRoles",
                columns: table => new
                {
                    RoleId = table.Column<decimal>(nullable: false),
                    Require18Plus = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignableRoles", x => x.RoleId);
                });

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignableRoles");

            migrationBuilder.DropTable(
                name: "RoleCategories");
        }
    }
}
