using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WasteCollectionPlatform.Common.Enums;

#nullable disable

namespace WasteCollectionPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:image_type", "Citizen,Collector")
                .Annotation("Npgsql:Enum:point_transaction_type", "Earn,Redeem")
                .Annotation("Npgsql:Enum:report_status", "Pending,Accepted,Assigned,OnTheWay,Collected,Failed")
                .Annotation("Npgsql:Enum:team_type", "Main,Support")
                .Annotation("Npgsql:Enum:user_role", "citizen,collector,enterprise,admin");

            migrationBuilder.Sql("DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'image_type') THEN CREATE TYPE image_type AS ENUM ('Citizen', 'Collector'); END IF; END $$;");
            migrationBuilder.Sql("DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'point_transaction_type') THEN CREATE TYPE point_transaction_type AS ENUM ('Earn', 'Redeem'); END IF; END $$;");
            migrationBuilder.Sql("DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'report_status') THEN CREATE TYPE report_status AS ENUM ('Pending', 'Accepted', 'Assigned', 'OnTheWay', 'Collected', 'Failed'); END IF; END $$;");
            migrationBuilder.Sql("DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'team_type') THEN CREATE TYPE team_type AS ENUM ('Main', 'Support'); END IF; END $$;");
            migrationBuilder.Sql("DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'user_role') THEN CREATE TYPE user_role AS ENUM ('citizen', 'collector', 'enterprise', 'admin'); END IF; END $$;");

            migrationBuilder.CreateTable(
                name: "district",
                columns: table => new
                {
                    districtid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    districtname = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("district_pkey", x => x.districtid);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    userid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fullname = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    role = table.Column<UserRole>(type: "user_role", nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    emailverified = table.Column<bool>(type: "boolean", nullable: false),
                    verificationtoken = table.Column<string>(type: "text", nullable: true),
                    verificationtokenexpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resetpasswordtoken = table.Column<string>(type: "text", nullable: true),
                    resettokenexpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("User_pkey", x => x.userid);
                });

            migrationBuilder.CreateTable(
                name: "voucher",
                columns: table => new
                {
                    voucherid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vouchername = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    pointsrequired = table.Column<int>(type: "integer", nullable: false),
                    stockquantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("voucher_pkey", x => x.voucherid);
                });

            migrationBuilder.CreateTable(
                name: "area",
                columns: table => new
                {
                    areaid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    districtid = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("area_pkey", x => x.areaid);
                    table.ForeignKey(
                        name: "fk_area_district",
                        column: x => x.districtid,
                        principalTable: "district",
                        principalColumn: "districtid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "citizen",
                columns: table => new
                {
                    citizenid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    totalpoints = table.Column<int>(type: "integer", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("citizen_pkey", x => x.citizenid);
                    table.ForeignKey(
                        name: "fk_citizen_user",
                        column: x => x.userid,
                        principalTable: "User",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enterprise",
                columns: table => new
                {
                    enterpriseid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    districtid = table.Column<int>(type: "integer", nullable: true),
                    wastetypes = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    dailycapacity = table.Column<int>(type: "integer", nullable: true),
                    currentload = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    status = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("enterprise_pkey", x => x.enterpriseid);
                    table.ForeignKey(
                        name: "fk_enterprise_district",
                        column: x => x.districtid,
                        principalTable: "district",
                        principalColumn: "districtid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_enterprise_user",
                        column: x => x.userid,
                        principalTable: "User",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refreshtoken",
                columns: table => new
                {
                    refreshtokenid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expiresat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    isrevoked = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    revokedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("refreshtoken_pkey", x => x.refreshtokenid);
                    table.ForeignKey(
                        name: "fk_refreshtoken_user",
                        column: x => x.userid,
                        principalTable: "User",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "team",
                columns: table => new
                {
                    teamid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    areaid = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("team_pkey", x => x.teamid);
                    table.ForeignKey(
                        name: "fk_team_area",
                        column: x => x.areaid,
                        principalTable: "area",
                        principalColumn: "areaid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wastereport",
                columns: table => new
                {
                    reportid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    citizenid = table.Column<int>(type: "integer", nullable: false),
                    areaid = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    wastetype = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    citizenlatitude = table.Column<decimal>(type: "numeric(10,8)", precision: 10, scale: 8, nullable: true),
                    citizenlongitude = table.Column<decimal>(type: "numeric(11,8)", precision: 11, scale: 8, nullable: true),
                    collectorlatitude = table.Column<decimal>(type: "numeric(10,8)", precision: 10, scale: 8, nullable: true),
                    collectorlongitude = table.Column<decimal>(type: "numeric(11,8)", precision: 11, scale: 8, nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expiretime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("wastereport_pkey", x => x.reportid);
                    table.ForeignKey(
                        name: "fk_report_area",
                        column: x => x.areaid,
                        principalTable: "area",
                        principalColumn: "areaid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_report_citizen",
                        column: x => x.citizenid,
                        principalTable: "citizen",
                        principalColumn: "citizenid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "collector",
                columns: table => new
                {
                    collectorid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    teamid = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    currenttaskcount = table.Column<int>(type: "integer", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("collector_pkey", x => x.collectorid);
                    table.ForeignKey(
                        name: "fk_collector_team",
                        column: x => x.teamid,
                        principalTable: "team",
                        principalColumn: "teamid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_collector_user",
                        column: x => x.userid,
                        principalTable: "User",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    notificationid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reportid = table.Column<int>(type: "integer", nullable: true),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    isread = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("notification_pkey", x => x.notificationid);
                    table.ForeignKey(
                        name: "fk_notification_report",
                        column: x => x.reportid,
                        principalTable: "wastereport",
                        principalColumn: "reportid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_notification_user",
                        column: x => x.userid,
                        principalTable: "User",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pointhistory",
                columns: table => new
                {
                    pointlogid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    citizenid = table.Column<int>(type: "integer", nullable: false),
                    reportid = table.Column<int>(type: "integer", nullable: true),
                    voucherid = table.Column<int>(type: "integer", nullable: true),
                    pointamount = table.Column<int>(type: "integer", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pointhistory_pkey", x => x.pointlogid);
                    table.ForeignKey(
                        name: "fk_point_citizen",
                        column: x => x.citizenid,
                        principalTable: "citizen",
                        principalColumn: "citizenid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_point_report",
                        column: x => x.reportid,
                        principalTable: "wastereport",
                        principalColumn: "reportid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_point_voucher",
                        column: x => x.voucherid,
                        principalTable: "voucher",
                        principalColumn: "voucherid",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "reportassignment",
                columns: table => new
                {
                    assignmentid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reportid = table.Column<int>(type: "integer", nullable: false),
                    teamid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("reportassignment_pkey", x => x.assignmentid);
                    table.ForeignKey(
                        name: "fk_assignment_report",
                        column: x => x.reportid,
                        principalTable: "wastereport",
                        principalColumn: "reportid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_assignment_team",
                        column: x => x.teamid,
                        principalTable: "team",
                        principalColumn: "teamid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reportimage",
                columns: table => new
                {
                    imageid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reportid = table.Column<int>(type: "integer", nullable: false),
                    imageurl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("reportimage_pkey", x => x.imageid);
                    table.ForeignKey(
                        name: "fk_image_report",
                        column: x => x.reportid,
                        principalTable: "wastereport",
                        principalColumn: "reportid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_area_districtid",
                table: "area",
                column: "districtid");

            migrationBuilder.CreateIndex(
                name: "citizen_userid_key",
                table: "citizen",
                column: "userid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "collector_userid_key",
                table: "collector",
                column: "userid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_collector_teamid",
                table: "collector",
                column: "teamid");

            migrationBuilder.CreateIndex(
                name: "enterprise_userid_key",
                table: "enterprise",
                column: "userid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_enterprise_districtid",
                table: "enterprise",
                column: "districtid");

            migrationBuilder.CreateIndex(
                name: "IX_notification_reportid",
                table: "notification",
                column: "reportid");

            migrationBuilder.CreateIndex(
                name: "IX_notification_userid",
                table: "notification",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_pointhistory_citizenid",
                table: "pointhistory",
                column: "citizenid");

            migrationBuilder.CreateIndex(
                name: "IX_pointhistory_reportid",
                table: "pointhistory",
                column: "reportid");

            migrationBuilder.CreateIndex(
                name: "IX_pointhistory_voucherid",
                table: "pointhistory",
                column: "voucherid");

            migrationBuilder.CreateIndex(
                name: "idx_refreshtoken_token",
                table: "refreshtoken",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_refreshtoken_userid",
                table: "refreshtoken",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_reportassignment_reportid",
                table: "reportassignment",
                column: "reportid");

            migrationBuilder.CreateIndex(
                name: "IX_reportassignment_teamid",
                table: "reportassignment",
                column: "teamid");

            migrationBuilder.CreateIndex(
                name: "IX_reportimage_reportid",
                table: "reportimage",
                column: "reportid");

            migrationBuilder.CreateIndex(
                name: "IX_team_areaid",
                table: "team",
                column: "areaid");

            migrationBuilder.CreateIndex(
                name: "User_email_key",
                table: "User",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wastereport_areaid",
                table: "wastereport",
                column: "areaid");

            migrationBuilder.CreateIndex(
                name: "IX_wastereport_citizenid",
                table: "wastereport",
                column: "citizenid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "collector");

            migrationBuilder.DropTable(
                name: "enterprise");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "pointhistory");

            migrationBuilder.DropTable(
                name: "refreshtoken");

            migrationBuilder.DropTable(
                name: "reportassignment");

            migrationBuilder.DropTable(
                name: "reportimage");

            migrationBuilder.DropTable(
                name: "voucher");

            migrationBuilder.DropTable(
                name: "team");

            migrationBuilder.DropTable(
                name: "wastereport");

            migrationBuilder.DropTable(
                name: "area");

            migrationBuilder.DropTable(
                name: "citizen");

            migrationBuilder.DropTable(
                name: "district");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
