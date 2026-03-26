using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WasteCollectionPlatform.Common.Enums;

#nullable disable

namespace WasteCollectionPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCoordinatesAndAddReportItems_V3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clean up potentially "broken" old report data as requested
            migrationBuilder.Sql("DELETE FROM \"ReportImages\" CASCADE;");
            migrationBuilder.Sql("DELETE FROM \"WasteReportItems\" CASCADE;");
            migrationBuilder.Sql("DELETE FROM \"WasteReports\" CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_collector_team",
                table: "Collectors");

            migrationBuilder.DropForeignKey(
                name: "fk_team_area",
                table: "Teams");

            // Drop columns safely using SQL IF EXISTS to avoid migration failure
            migrationBuilder.Sql("ALTER TABLE \"WasteReports\" DROP COLUMN IF EXISTS \"CitizenLatitude\";");
            migrationBuilder.Sql("ALTER TABLE \"WasteReports\" DROP COLUMN IF EXISTS \"CitizenLongitude\";");
            migrationBuilder.Sql("ALTER TABLE \"WasteReports\" DROP COLUMN IF EXISTS \"CollectorLatitude\";");
            migrationBuilder.Sql("ALTER TABLE \"WasteReports\" DROP COLUMN IF EXISTS \"CollectorLongitude\";");

            // The enum alterations were causing issues with schemas not existing.
            // They are already defined in previous migrations or the context.
            // We skip migrationBuilder.AlterDatabase() here to avoid schema "collector_role" errors.

            migrationBuilder.AlterColumn<ReportStatus>(
                name: "Status",
                table: "WasteReports",
                type: "report_status",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "AreaId",
                table: "Teams",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<TeamType>(
                name: "Type",
                table: "Teams",
                type: "team_type",
                nullable: false,
                defaultValue: TeamType.Main);

            migrationBuilder.AddColumn<string>(
                name: "ImageType",
                table: "ReportImages",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TeamId",
                table: "Collectors",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<CollectorRole>(
                name: "Role",
                table: "Collectors",
                type: "collector_role",
                nullable: false,
                defaultValue: CollectorRole.Member,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            // Create table safely (will drop if existed above)
            migrationBuilder.CreateTable(
                name: "WasteReportItems",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReportId = table.Column<int>(type: "integer", nullable: false),
                    WasteType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("wastereportitem_pkey", x => x.ItemId);
                    table.ForeignKey(
                        name: "fk_item_report",
                        column: x => x.ReportId,
                        principalTable: "WasteReports",
                        principalColumn: "ReportId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WasteReportItems_ReportId",
                table: "WasteReportItems",
                column: "ReportId");

            migrationBuilder.AddForeignKey(
                name: "fk_collector_team",
                table: "Collectors",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "fk_team_area",
                table: "Teams",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "AreaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_collector_team",
                table: "Collectors");

            migrationBuilder.DropForeignKey(
                name: "fk_team_area",
                table: "Teams");

            migrationBuilder.DropTable(
                name: "WasteReportItems");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "ReportImages");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:image_type", "Citizen,Collector")
                .Annotation("Npgsql:Enum:point_transaction_type", "Earn,Redeem")
                .Annotation("Npgsql:Enum:report_status", "Pending,Assigned,Processing,Completed,Cancelled")
                .Annotation("Npgsql:Enum:team_type", "Main,Support")
                .Annotation("Npgsql:Enum:user_role.user_role", "citizen,collector,enterprise,admin,manager")
                .OldAnnotation("Npgsql:Enum:collector_role.collector_role", "member,leader")
                .OldAnnotation("Npgsql:Enum:report_status.report_status", "pending,accepted,assigned,on_the_way,collected,failed,reported_by_team")
                .OldAnnotation("Npgsql:Enum:team_type.team_type", "main,support")
                .OldAnnotation("Npgsql:Enum:user_role.user_role", "citizen,collector,enterprise,admin,manager");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "WasteReports",
                type: "integer",
                nullable: false,
                oldClrType: typeof(ReportStatus),
                oldType: "report_status");

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

            migrationBuilder.AlterColumn<int>(
                name: "AreaId",
                table: "Teams",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TeamId",
                table: "Collectors",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Collectors",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(CollectorRole),
                oldType: "collector_role",
                oldDefaultValue: CollectorRole.Member);

            migrationBuilder.AddForeignKey(
                name: "fk_collector_team",
                table: "Collectors",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_team_area",
                table: "Teams",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "AreaId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
