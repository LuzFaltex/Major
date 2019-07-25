using Microsoft.EntityFrameworkCore.Migrations;

namespace MajorInteractiveBot.Data.Migrations
{
    public partial class RestructureGuild : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommandChannels_Guilds_GuildId",
                table: "CommandChannels");

            migrationBuilder.DropForeignKey(
                name: "FK_Modules_Guilds_GuildId",
                table: "Modules");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Modules",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Modules_GuildId",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_CommandChannels_GuildId",
                table: "CommandChannels");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Modules",
                table: "Modules",
                columns: new[] { "GuildId", "ModuleName" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Modules",
                table: "Modules");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Modules",
                table: "Modules",
                column: "ModuleName");

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    GuildId = table.Column<ulong>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Modules_GuildId",
                table: "Modules",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_CommandChannels_GuildId",
                table: "CommandChannels",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_GuildId",
                table: "Tags",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommandChannels_Guilds_GuildId",
                table: "CommandChannels",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_Guilds_GuildId",
                table: "Modules",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
