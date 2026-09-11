using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RequestFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class OTPAddedInApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OtpCode",
                table: "ApplicationUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpExpiry",
                table: "ApplicationUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtpCode",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "OtpExpiry",
                table: "ApplicationUsers");
        }
    }
}
