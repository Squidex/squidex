using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Squidex.Providers.Postgres.App.Migrations
{
    /// <inheritdoc />
    public partial class AddTextStateVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "TextState",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "TextState");
        }
    }
}
