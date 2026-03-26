using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteCollectionPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteToWasteReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WasteReportItems_WasteReports_ReportId",
                table: "WasteReportItems");

            migrationBuilder.DropColumn(
                name: "CitizenLatitude",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "CitizenLongitude",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "CollectorLatitude",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "CollectorLongitude",
                table: "WasteReports");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "WasteReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_item_report",
                table: "WasteReportItems",
                column: "ReportId",
                principalTable: "WasteReports",
                principalColumn: "ReportId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_item_report",
                table: "WasteReportItems");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "WasteReports");

            migrationBuilder.AddColumn<decimal>(
                name: "CitizenLatitude",
                table: "WasteReports",
                type: "numeric(10,8)",
                precision: 10,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CitizenLongitude",
                table: "WasteReports",
                type: "numeric(11,8)",
                precision: 11,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CollectorLatitude",
                table: "WasteReports",
                type: "numeric(10,8)",
                precision: 10,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CollectorLongitude",
                table: "WasteReports",
                type: "numeric(11,8)",
                precision: 11,
                scale: 8,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WasteReportItems_WasteReports_ReportId",
                table: "WasteReportItems",
                column: "ReportId",
                principalTable: "WasteReports",
                principalColumn: "ReportId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
