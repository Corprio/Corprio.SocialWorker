using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corprio.SocialWorker.Migrations
{
    /// <inheritdoc />
    public partial class _20240422123410_v0_5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MetaMentions",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacebookPageID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreaterID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreaterName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CDNUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApplicationID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaMentions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MetaMentions_MetaPages_FacebookPageID",
                        column: x => x.FacebookPageID,
                        principalTable: "MetaPages",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetaMentions_FacebookPageID",
                table: "MetaMentions",
                column: "FacebookPageID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetaMentions");
        }
    }
}
