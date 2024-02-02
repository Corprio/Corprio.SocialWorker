using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corprio.SocialWorker.Migrations
{
    /// <inheritdoc />
    public partial class v20240201_updatedSchemaForOTP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OTP_Code",
                table: "MetaBotStatuses");

            migrationBuilder.DropColumn(
                name: "OTP_ExpiryTime",
                table: "MetaBotStatuses");

            migrationBuilder.AddColumn<Guid>(
                name: "OtpSessionID",
                table: "MetaBotStatuses",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtpSessionID",
                table: "MetaBotStatuses");

            migrationBuilder.AddColumn<string>(
                name: "OTP_Code",
                table: "MetaBotStatuses",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OTP_ExpiryTime",
                table: "MetaBotStatuses",
                type: "datetimeoffset",
                nullable: true);
        }
    }
}
