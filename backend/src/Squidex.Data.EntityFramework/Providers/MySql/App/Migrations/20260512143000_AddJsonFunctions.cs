using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Squidex.Providers.MySql.App.Migrations
{
    [DbContext(typeof(MySqlAppDbContext))]
    [Migration("20260512143000_AddJsonFunctions")]
    internal sealed class AddJsonFunctions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            JsonFunctionMigration.Create(migrationBuilder, typeof(MySqlDialect), "Squidex.Providers.MySql.json_function.sql", splitStatements: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            JsonFunctionMigration.Drop(migrationBuilder, typeof(MySqlDialect), "Squidex.Providers.MySql.json_function.sql", splitStatements: true);
        }
    }
}
