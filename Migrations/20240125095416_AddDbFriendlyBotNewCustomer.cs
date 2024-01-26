using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corprio.SocialWorker.Migrations
{
    /// <inheritdoc />
    public partial class AddDbFriendlyBotNewCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NewCustomer",
                table: "MetaBotStatuses",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostedProductID",
                table: "MetaBotStatuses",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewCustomer",
                table: "MetaBotStatuses");

            migrationBuilder.DropColumn(
                name: "PostedProductID",
                table: "MetaBotStatuses");
        }
    }
}
