using Microsoft.EntityFrameworkCore.Migrations;

namespace MajorInteractiveBot.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommandChannels",
                columns: table => new
                {
                    ChannelId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandChannels", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(nullable: false),
                    CommandPrefix = table.Column<string>(nullable: true),
                    GreetUser = table.Column<bool>(nullable: false),
                    GreetingChannel = table.Column<decimal>(nullable: false),
                    GreetingMessage = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    ModuleName = table.Column<string>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => new { x.GuildId, x.ModuleName });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommandChannels");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "Modules");
        }
    }
}
