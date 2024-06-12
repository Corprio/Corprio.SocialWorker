using System;
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
            migrationBuilder.DropForeignKey(
                name: "FK_MetaBotStatuses_MetaUsers_FacebookUserID",
                table: "MetaBotStatuses");

            migrationBuilder.DropTable(
                name: "CommentWebhooks");

            migrationBuilder.DropTable(
                name: "FeedWebhooks");

            migrationBuilder.DropTable(
                name: "MessageWebhooks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MetaBotStatuses",
                table: "MetaBotStatuses");

            migrationBuilder.RenameTable(
                name: "MetaBotStatuses",
                newName: "BotStatuses");

            migrationBuilder.RenameColumn(
                name: "MetaUserName",
                table: "BotStatuses",
                newName: "BuyerUserName");

            migrationBuilder.RenameIndex(
                name: "IX_MetaBotStatuses_FacebookUserID",
                table: "BotStatuses",
                newName: "IX_BotStatuses_FacebookUserID");

            migrationBuilder.AlterColumn<Guid>(
                name: "FacebookUserID",
                table: "BotStatuses",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "LineChannelID",
                table: "BotStatuses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BotStatuses",
                table: "BotStatuses",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "LineChannels",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Dormant = table.Column<bool>(type: "bit", nullable: false),
                    ChannelID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChannelName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ChannelSecret = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChannelToken = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    OrganizationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApplicationID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineChannels", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MetaCommentWebhooks",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediaItemID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WebhookChangeID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApplicationID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaCommentWebhooks", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MetaFeedWebhooks",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SenderID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedTime = table.Column<double>(type: "float", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApplicationID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaFeedWebhooks", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MetaMessageWebhooks",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SenderID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TimeStamp = table.Column<double>(type: "float", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApplicationID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaMessageWebhooks", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BotStatuses_LineChannelID",
                table: "BotStatuses",
                column: "LineChannelID");

            migrationBuilder.AddForeignKey(
                name: "FK_BotStatuses_LineChannels_LineChannelID",
                table: "BotStatuses",
                column: "LineChannelID",
                principalTable: "LineChannels",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_BotStatuses_MetaUsers_FacebookUserID",
                table: "BotStatuses",
                column: "FacebookUserID",
                principalTable: "MetaUsers",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BotStatuses_LineChannels_LineChannelID",
                table: "BotStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_BotStatuses_MetaUsers_FacebookUserID",
                table: "BotStatuses");

            migrationBuilder.DropTable(
                name: "LineChannels");

            migrationBuilder.DropTable(
                name: "MetaCommentWebhooks");

            migrationBuilder.DropTable(
                name: "MetaFeedWebhooks");

            migrationBuilder.DropTable(
                name: "MetaMessageWebhooks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BotStatuses",
                table: "BotStatuses");

            migrationBuilder.DropIndex(
                name: "IX_BotStatuses_LineChannelID",
                table: "BotStatuses");

            migrationBuilder.DropColumn(
                name: "LineChannelID",
                table: "BotStatuses");

            migrationBuilder.RenameTable(
                name: "BotStatuses",
                newName: "MetaBotStatuses");

            migrationBuilder.RenameColumn(
                name: "BuyerUserName",
                table: "MetaBotStatuses",
                newName: "MetaUserName");

            migrationBuilder.RenameIndex(
                name: "IX_BotStatuses_FacebookUserID",
                table: "MetaBotStatuses",
                newName: "IX_MetaBotStatuses_FacebookUserID");

            migrationBuilder.AlterColumn<Guid>(
                name: "FacebookUserID",
                table: "MetaBotStatuses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetaBotStatuses",
                table: "MetaBotStatuses",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "CommentWebhooks",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastUpdateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MediaItemID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    WebhookChangeID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentWebhooks", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FeedWebhooks",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedTime = table.Column<double>(type: "float", nullable: false),
                    LastUpdateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PostID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    SenderID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedWebhooks", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MessageWebhooks",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastUpdateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RecipientID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    SenderID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TimeStamp = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageWebhooks", x => x.ID);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_MetaBotStatuses_MetaUsers_FacebookUserID",
                table: "MetaBotStatuses",
                column: "FacebookUserID",
                principalTable: "MetaUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
