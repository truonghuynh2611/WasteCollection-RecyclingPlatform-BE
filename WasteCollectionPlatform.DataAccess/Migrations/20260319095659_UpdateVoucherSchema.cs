using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WasteCollectionPlatform.Common.Enums;

#nullable disable

namespace WasteCollectionPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVoucherSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Vouchers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoucherCode",
                table: "Vouchers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Vouchers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Vouchers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExpiryDays",
                table: "Vouchers",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "VoucherCode",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "ExpiryDays",
                table: "Vouchers");
        }
    }
}
