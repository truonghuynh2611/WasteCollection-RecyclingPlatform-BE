using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteCollectionPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReportStatusEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:collector_role.collector_role", "member,leader")
                .Annotation("Npgsql:Enum:image_type", "Citizen,Collector")
                .Annotation("Npgsql:Enum:point_transaction_type", "Earn,Redeem")
                .Annotation("Npgsql:Enum:report_status", "Pending,Accepted,Assigned,OnTheWay,Collected,Failed")
                .Annotation("Npgsql:Enum:team_type", "Main,Support")
                .Annotation("Npgsql:Enum:user_role.user_role", "citizen,collector,enterprise,admin,manager")
                .OldAnnotation("Npgsql:Enum:collector_role.collector_role", "member,leader")
                .OldAnnotation("Npgsql:Enum:image_type", "Citizen,Collector")
                .OldAnnotation("Npgsql:Enum:point_transaction_type", "Earn,Redeem")
                .OldAnnotation("Npgsql:Enum:report_status", "Pending,Assigned,Processing,Completed,Cancelled")
                .OldAnnotation("Npgsql:Enum:team_type", "Main,Support")
                .OldAnnotation("Npgsql:Enum:user_role.user_role", "citizen,collector,enterprise,admin,manager");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:collector_role.collector_role", "member,leader")
                .Annotation("Npgsql:Enum:image_type", "Citizen,Collector")
                .Annotation("Npgsql:Enum:point_transaction_type", "Earn,Redeem")
                .Annotation("Npgsql:Enum:report_status", "Pending,Assigned,Processing,Completed,Cancelled")
                .Annotation("Npgsql:Enum:team_type", "Main,Support")
                .Annotation("Npgsql:Enum:user_role.user_role", "citizen,collector,enterprise,admin,manager")
                .OldAnnotation("Npgsql:Enum:collector_role.collector_role", "member,leader")
                .OldAnnotation("Npgsql:Enum:image_type", "Citizen,Collector")
                .OldAnnotation("Npgsql:Enum:point_transaction_type", "Earn,Redeem")
                .OldAnnotation("Npgsql:Enum:report_status", "Pending,Accepted,Assigned,OnTheWay,Collected,Failed")
                .OldAnnotation("Npgsql:Enum:team_type", "Main,Support")
                .OldAnnotation("Npgsql:Enum:user_role.user_role", "citizen,collector,enterprise,admin,manager");
        }
    }
}
