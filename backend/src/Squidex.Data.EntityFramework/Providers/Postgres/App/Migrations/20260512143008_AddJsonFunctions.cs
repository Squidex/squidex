using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Squidex.Providers.Postgres.App.Migrations
{
    [DbContext(typeof(PostgresAppDbContext))]
    [Migration("20260512143008_AddJsonFunctions")]
    internal sealed class AddJsonFunctions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            JsonFunctionMigration.Create(migrationBuilder, typeof(PostgresDialect), "Squidex.Providers.Postgres.json_function.sql", splitStatements: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            JsonFunctionMigration.Drop(migrationBuilder, typeof(PostgresDialect), "Squidex.Providers.Postgres.json_function.sql", splitStatements: false);
        }
    }
}
