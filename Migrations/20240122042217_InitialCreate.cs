using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corprio.SocialWorker.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MetaUsers",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Dormant = table.Column<bool>(type: "bit", nullable: false),
                    FacebookUserID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrganizationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApplicationID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaUsers", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MetaBotStatuses",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuyerID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FacebookUserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Language = table.Column<int>(type: "int", nullable: false),
                    ThinkingOf = table.Column<int>(type: "int", nullable: false),
                    ProductMemoryString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VariationMemoryString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttributeValueMemoryString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CartString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuyerCorprioID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BuyerEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OTP_Code = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    OTP_ExpiryTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApplicationID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaBotStatuses", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MetaBotStatuses_MetaUsers_FacebookUserID",
                        column: x => x.FacebookUserID,
                        principalTable: "MetaUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetaPages",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacebookUserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Token = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InstagramID = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApplicationID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaPages", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MetaPages_MetaUsers_FacebookUserID",
                        column: x => x.FacebookUserID,
                        principalTable: "MetaUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetaPosts",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacebookPageID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KeywordForShoppingIntention = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PostedWith = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApplicationID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaPosts", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MetaPosts_MetaPages_FacebookPageID",
                        column: x => x.FacebookPageID,
                        principalTable: "MetaPages",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetaBotStatuses_FacebookUserID",
                table: "MetaBotStatuses",
                column: "FacebookUserID");

            migrationBuilder.CreateIndex(
                name: "IX_MetaPages_FacebookUserID",
                table: "MetaPages",
                column: "FacebookUserID");

            migrationBuilder.CreateIndex(
                name: "IX_MetaPosts_FacebookPageID",
                table: "MetaPosts",
                column: "FacebookPageID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetaBotStatuses");

            migrationBuilder.DropTable(
                name: "MetaPosts");

            migrationBuilder.DropTable(
                name: "MetaPages");

            migrationBuilder.DropTable(
                name: "MetaUsers");
        }
    }
}
