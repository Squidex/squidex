using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Squidex.Providers.Postgres.App.Migrations
{
    /// <inheritdoc />
    public partial class AddContentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FromSchema",
                table: "ContentReferencesPublished",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "00000000-0000-0000-0000-000000000000");

            migrationBuilder.AddColumn<string>(
                name: "FromSchema",
                table: "ContentReferencesAll",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "00000000-0000-0000-0000-000000000000");

            migrationBuilder.CreateTable(
                name: "ContentTables",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SchemaId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentTables", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentTables_AppId_SchemaId",
                table: "ContentTables",
                columns: new[] { "AppId", "SchemaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentTables");

            migrationBuilder.DropColumn(
                name: "FromSchema",
                table: "ContentReferencesPublished");

            migrationBuilder.DropColumn(
                name: "FromSchema",
                table: "ContentReferencesAll");
        }
    }
}
