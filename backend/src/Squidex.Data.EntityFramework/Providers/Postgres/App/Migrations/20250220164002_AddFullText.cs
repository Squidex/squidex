using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Squidex.Providers.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddFullText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "Geos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    AppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SchemaId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Stage = table.Column<byte>(type: "smallint", nullable: false),
                    ServeAll = table.Column<bool>(type: "boolean", nullable: false),
                    ServePublished = table.Column<bool>(type: "boolean", nullable: false),
                    GeoField = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    GeoObject = table.Column<Geometry>(type: "geometry", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Geos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Texts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    AppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SchemaId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Stage = table.Column<byte>(type: "smallint", nullable: false),
                    ServeAll = table.Column<bool>(type: "boolean", nullable: false),
                    ServePublished = table.Column<bool>(type: "boolean", nullable: false),
                    Texts = table.Column<string>(type: "text", nullable: false)
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

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
        }
    }
}
