using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corprio.SocialWorker.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsTableToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizationID = table.Column<Guid>(type: "TEXT", nullable: false),
                    SendConfirmationEmail = table.Column<bool>(type: "INTEGER", nullable: false),
                    DefaultEmailSubject = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsSmtpSet = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmailToReceiveOrder = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    WarehouseID = table.Column<Guid>(type: "TEXT", nullable: false),
                    SelfPickUp = table.Column<bool>(type: "INTEGER", nullable: false),
                    SelfPickUpInstruction = table.Column<string>(type: "TEXT", nullable: true),
                    ShipToCustomer = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeliveryCharge = table.Column<decimal>(type: "TEXT", nullable: false),
                    DeliveryChargeProductID = table.Column<Guid>(type: "TEXT", nullable: true),
                    FreeShippingAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastUpdateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ApplicationID = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
