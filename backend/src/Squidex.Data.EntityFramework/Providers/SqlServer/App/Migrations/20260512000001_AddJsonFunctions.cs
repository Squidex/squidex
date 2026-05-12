using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;
using Squidex.Providers.SqlServer;

#nullable disable

namespace Squidex.Providers.SqlServer.App.Migrations
{
    /// <inheritdoc />
    public partial class AddJsonFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var assembly = typeof(SqlServerDialect).Assembly;

            using var sqlStream = assembly.GetManifestResourceStream("Squidex.Providers.SqlServer.json_function.sql")!;
            using var reader = new StreamReader(sqlStream);

            var sqlText = reader.ReadToEnd();
            var statements = sqlText.Split(";;", System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

            foreach (var statement in statements)
            {
                migrationBuilder.Sql(statement);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var functions = new[]
            {
                "dbo.json_empty",
                "dbo.json_exists",
                "dbo.json_null_equals",
                "dbo.json_null_notequals",
                "dbo.json_text_contains",
                "dbo.json_text_endswith",
                "dbo.json_text_equals",
                "dbo.json_text_greaterthan",
                "dbo.json_text_greaterthanorequal",
                "dbo.json_text_lessthan",
                "dbo.json_text_lessthanorequal",
                "dbo.json_text_matchs",
                "dbo.json_text_notequals",
                "dbo.json_text_startswith",
                "dbo.json_boolean_equals",
                "dbo.json_boolean_notequals",
                "dbo.json_number_equals",
                "dbo.json_number_greaterthan",
                "dbo.json_number_greaterthanorequal",
                "dbo.json_number_lessthan",
                "dbo.json_number_lessthanorequal",
                "dbo.json_number_notequals",
            };

            foreach (var function in functions)
            {
                migrationBuilder.Sql($"DROP FUNCTION IF EXISTS {function}");
            }
        }
    }
}
