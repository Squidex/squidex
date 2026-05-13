// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Squidex.Providers;

internal static partial class JsonFunctionMigration
{
    public static void Create(MigrationBuilder migrationBuilder, Type anchorType, string resourceName, bool splitStatements)
    {
        var sqlText = ReadSql(anchorType, resourceName);

        if (splitStatements)
        {
            foreach (var statement in SplitStatements(sqlText))
            {
                if (statement.StartsWith("CREATE", StringComparison.OrdinalIgnoreCase))
                {
                    migrationBuilder.Sql(statement);
                }
            }
        }
        else
        {
            migrationBuilder.Sql(sqlText);
        }
    }

    public static void Drop(MigrationBuilder migrationBuilder, Type anchorType, string resourceName, bool splitStatements)
    {
        var sqlText = ReadSql(anchorType, resourceName);

        if (splitStatements)
        {
            foreach (var statement in SplitStatements(sqlText).Reverse())
            {
                if (statement.StartsWith("DROP", StringComparison.OrdinalIgnoreCase))
                {
                    migrationBuilder.Sql(statement);
                }
            }
        }
        else
        {
            foreach (var functionName in ParseFunctions(sqlText).Reverse())
            {
                migrationBuilder.Sql($"DROP FUNCTION IF EXISTS {functionName} CASCADE;");
            }
        }
    }

    private static string ReadSql(Type anchorType, string resourceName)
    {
        using var sqlStream = anchorType.Assembly.GetManifestResourceStream(resourceName) ??
            throw new InvalidOperationException($"Cannot find embedded resource '{resourceName}'.");

        using var reader = new StreamReader(sqlStream);

        return reader.ReadToEnd();
    }

    private static string[] SplitStatements(string sqlText)
    {
        return sqlText.Split(";;", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static IEnumerable<string> ParseFunctions(string sqlText)
    {
        return FunctionRegex().Matches(sqlText).Select(x => x.Groups[1].Value);
    }

    [GeneratedRegex(@"CREATE\s+OR\s+REPLACE\s+FUNCTION\s+([a-zA-Z0-9_]+)\s*\(", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture)]
    private static partial Regex FunctionRegex();
}
