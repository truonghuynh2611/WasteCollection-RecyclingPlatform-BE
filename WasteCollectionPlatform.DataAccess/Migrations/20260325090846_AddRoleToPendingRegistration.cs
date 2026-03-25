using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WasteCollectionPlatform.Common.Enums;

#nullable disable

namespace WasteCollectionPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleToPendingRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:image_type", "Citizen,Collector")
                .Annotation("Npgsql:Enum:point_transaction_type", "Earn,Redeem")
                .Annotation("Npgsql:Enum:report_status", "Pending,Assigned,Processing,Completed,Cancelled")
                .Annotation("Npgsql:Enum:team_type", "Main,Support")
                .Annotation("Npgsql:Enum:user_role.user_role", "citizen,collector,enterprise,admin,manager")
                .OldAnnotation("Npgsql:Enum:collector_role.collector_role", "member,leader")
                .OldAnnotation("Npgsql:Enum:image_type", "Citizen,Collector")
                .OldAnnotation("Npgsql:Enum:point_transaction_type", "Earn,Redeem")
                .OldAnnotation("Npgsql:Enum:report_status", "Pending,Assigned,Processing,Completed,Cancelled")
                .OldAnnotation("Npgsql:Enum:team_type", "Main,Support")
                .OldAnnotation("Npgsql:Enum:user_role.user_role", "citizen,collector,enterprise,admin,manager");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "WasteReports",
                type: "integer",
                nullable: false,
                oldClrType: typeof(ReportStatus),
                oldType: "report_status");

            migrationBuilder.AddColumn<UserRole>(
                name: "Role",
                table: "PendingRegistrations",
                type: "user_role",
                nullable: false,
                defaultValue: UserRole.Citizen);

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Collectors",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "collector_role",
                oldDefaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "PendingRegistrations");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:collector_role.collector_role", "member,leader")
                .Annotation("Npgsql:Enum:image_type", "Citizen,Collector")
                .Annotation("Npgsql:Enum:point_transaction_type", "Earn,Redeem")
                .Annotation("Npgsql:Enum:report_status", "Pending,Assigned,Processing,Completed,Cancelled")
                .Annotation("Npgsql:Enum:team_type", "Main,Support")
                .Annotation("Npgsql:Enum:user_role.user_role", "citizen,collector,enterprise,admin,manager")
                .OldAnnotation("Npgsql:Enum:image_type", "Citizen,Collector")
                .OldAnnotation("Npgsql:Enum:point_transaction_type", "Earn,Redeem")
                .OldAnnotation("Npgsql:Enum:report_status", "Pending,Assigned,Processing,Completed,Cancelled")
                .OldAnnotation("Npgsql:Enum:team_type", "Main,Support")
                .OldAnnotation("Npgsql:Enum:user_role.user_role", "citizen,collector,enterprise,admin,manager");

            migrationBuilder.AlterColumn<ReportStatus>(
                name: "Status",
                table: "WasteReports",
                type: "report_status",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Collectors",
                type: "collector_role",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    AdminId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsSuperAdmin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: true, defaultValue: 1),
                    Status = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("admin_pkey", x => x.AdminId);
                    table.ForeignKey(
                        name: "fk_admin_createdby",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_admin_user",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "admin_userid_key",
                table: "Admins",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admins_CreatedBy",
                table: "Admins",
                column: "CreatedBy");
        }
    }
}
