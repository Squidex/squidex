using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;
using Squidex.Providers.Postgres;

#nullable disable

namespace Squidex.Providers.Postgres.App.Migrations
{
    /// <inheritdoc />
    public partial class AddJsonFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var assembly = typeof(PostgresDialect).Assembly;

            using var sqlStream = assembly.GetManifestResourceStream("Squidex.Providers.Postgres.json_function.sql")!;
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
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_empty(jsonb)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_exists(jsonb)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_null_equals(jsonb)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_null_notequals(jsonb)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_text_equals(jsonb, text)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_text_notequals(jsonb, text)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_text_lessthan(jsonb, text)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_text_lessthanorequal(jsonb, text)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_text_greaterthan(jsonb, text)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_text_greaterthanorequal(jsonb, text)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_text_contains(jsonb, text)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_text_startswith(jsonb, text)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_text_endswith(jsonb, text)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_text_matchs(jsonb, text)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_text_in(jsonb, text[])");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_number_equals(jsonb, numeric)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_number_notequals(jsonb, numeric)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_number_lessthan(jsonb, numeric)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_number_lessthanorequal(jsonb, numeric)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_number_greaterthan(jsonb, numeric)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_number_greaterthanorequal(jsonb, numeric)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_number_in(jsonb, numeric[])");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_boolean_equals(jsonb, boolean)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_boolean_notequals(jsonb, boolean)");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS jsonb_boolean_in(jsonb, boolean[])");
        }
    }
}
