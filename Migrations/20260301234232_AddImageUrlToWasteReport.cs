using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteReportApp.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToWasteReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "WasteReports",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "WasteReports");
        }
    }
}
