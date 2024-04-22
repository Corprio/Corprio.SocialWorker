using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corprio.SocialWorker.Migrations
{
    /// <inheritdoc />
    public partial class v0_7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreaterName",
                table: "MetaMentions",
                newName: "CreatorName");

            migrationBuilder.RenameColumn(
                name: "CreaterID",
                table: "MetaMentions",
                newName: "CreatorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatorName",
                table: "MetaMentions",
                newName: "CreaterName");

            migrationBuilder.RenameColumn(
                name: "CreatorID",
                table: "MetaMentions",
                newName: "CreaterID");
        }
    }
}
