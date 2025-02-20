using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Squidex.Providers.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddFullText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Geos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SchemaId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Stage = table.Column<byte>(type: "tinyint", nullable: false),
                    ServeAll = table.Column<bool>(type: "bit", nullable: false),
                    ServePublished = table.Column<bool>(type: "bit", nullable: false),
                    GeoField = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    GeoObject = table.Column<Geometry>(type: "geography", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Geos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Texts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SchemaId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Stage = table.Column<byte>(type: "tinyint", nullable: false),
                    ServeAll = table.Column<bool>(type: "bit", nullable: false),
                    ServePublished = table.Column<bool>(type: "bit", nullable: false),
                    Texts = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Texts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Geos");

            migrationBuilder.DropTable(
                name: "Texts");
        }
    }
}
