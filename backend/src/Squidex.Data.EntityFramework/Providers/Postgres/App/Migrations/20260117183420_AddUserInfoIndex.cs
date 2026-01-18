using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Squidex.Providers.Postgres.App.Migrations
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
                    Id = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    AppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SchemaId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Stage = table.Column<byte>(type: "smallint", nullable: false),
                    ServeAll = table.Column<bool>(type: "boolean", nullable: false),
                    ServePublished = table.Column<bool>(type: "boolean", nullable: false),
                    UserInfoApiKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UserInfoRole = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
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
