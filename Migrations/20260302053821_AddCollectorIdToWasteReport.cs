using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WasteReportApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectorIdToWasteReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CollectorId",
                table: "WasteReports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollectorImageUrl",
                table: "WasteReports",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AreaId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TeamType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.TeamId);
                    table.ForeignKey(
                        name: "FK_Teams_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "AreaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Collectors",
                columns: table => new
                {
                    CollectorId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<bool>(type: "boolean", nullable: false),
                    CurrentTaskCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collectors", x => x.CollectorId);
                    table.ForeignKey(
                        name: "FK_Collectors_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WasteReports_CollectorId",
                table: "WasteReports",
                column: "CollectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Collectors_TeamId",
                table: "Collectors",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_AreaId",
                table: "Teams",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_WasteReports_Collectors_CollectorId",
                table: "WasteReports",
                column: "CollectorId",
                principalTable: "Collectors",
                principalColumn: "CollectorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WasteReports_Collectors_CollectorId",
                table: "WasteReports");

            migrationBuilder.DropTable(
                name: "Collectors");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_WasteReports_CollectorId",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "CollectorId",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "CollectorImageUrl",
                table: "WasteReports");
        }
    }
}
