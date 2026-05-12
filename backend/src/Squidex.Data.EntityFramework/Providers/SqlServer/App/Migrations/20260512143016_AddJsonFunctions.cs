using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Squidex.Providers.SqlServer.App.Migrations
{
    [DbContext(typeof(SqlServerAppDbContext))]
    [Migration("20260512143016_AddJsonFunctions")]
    internal sealed class AddJsonFunctions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            JsonFunctionMigration.Create(migrationBuilder, typeof(SqlServerDialect), "Squidex.Providers.SqlServer.json_function.sql", splitStatements: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            JsonFunctionMigration.Drop(migrationBuilder, typeof(SqlServerDialect), "Squidex.Providers.SqlServer.json_function.sql", splitStatements: true);
        }
    }
}
