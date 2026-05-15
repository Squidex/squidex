using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;
using Squidex.Providers.MySql;

#nullable disable

namespace Squidex.Providers.MySql.App.Migrations
{
    /// <inheritdoc />
    public partial class AddJsonFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var assembly = typeof(MySqlDialect).Assembly;

            using var sqlStream = assembly.GetManifestResourceStream("Squidex.Providers.MySql.json_function.sql")!;
            using var reader = new StreamReader(sqlStream);

            var sqlText = reader.ReadToEnd();
            var statements = sqlText.Split(";;", System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

            foreach (var statement in statements)
            {
                migrationBuilder.Sql(statement, suppressTransaction: true);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var functions = new[]
            {
                "json_empty",
                "json_exists",
                "json_null_equals",
                "json_null_notequals",
                "json_text_contains",
                "json_text_endswith",
                "json_text_equals",
                "json_text_greaterthan",
                "json_text_greaterthanorequal",
                "json_text_in",
                "json_text_lessthan",
                "json_text_lessthanorequal",
                "json_text_matchs",
                "json_text_notequals",
                "json_text_startswith",
                "json_boolean_equals",
                "json_boolean_in",
                "json_boolean_notequals",
                "json_number_equals",
                "json_number_greaterthan",
                "json_number_greaterthanorequal",
                "json_number_in",
                "json_number_lessthan",
                "json_number_lessthanorequal",
                "json_number_notequals",
            };

            foreach (var function in functions)
            {
                migrationBuilder.Sql($"DROP FUNCTION IF EXISTS {function}", suppressTransaction: true);
            }
        }
    }
}
