using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corprio.SocialWorker.Migrations
{
    /// <inheritdoc />
    public partial class v0_6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Mentioned",
                table: "MetaMentions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mentioned",
                table: "MetaMentions");
        }
    }
}
