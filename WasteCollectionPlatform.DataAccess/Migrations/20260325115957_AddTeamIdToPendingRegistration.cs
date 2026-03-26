using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteCollectionPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamIdToPendingRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "PendingRegistrations",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "PendingRegistrations");
        }
    }
}
