using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Squidex.Providers.SqlServer.App.Migrations
{
    /// <inheritdoc />
    public partial class AddUserInfoIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserInfos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SchemaId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Stage = table.Column<byte>(type: "tinyint", nullable: false),
                    ServeAll = table.Column<bool>(type: "bit", nullable: false),
                    ServePublished = table.Column<bool>(type: "bit", nullable: false),
                    UserInfoApiKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserInfoRole = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInfos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserInfos_UserInfoApiKey",
                table: "UserInfos",
                column: "UserInfoApiKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserInfos");
        }
    }
}
