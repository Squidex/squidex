// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure.Queries;

namespace Squidex.Providers.SqlServer;

public static class JsonFunction
{
    private const int TypeAny = 0;
    private const int TypeNull = 1;
    private const int TypeText = 2;
    private const int TypeBoolean = 3;
    private const int TypeNumber = 4;

    private static readonly Dictionary<(int Type, CompareOperator Operator), string> Functions = new ()
    {
        [(TypeAny, CompareOperator.Empty)] = "dbo.json_empty",
        [(TypeAny, CompareOperator.Exists)] = "dbo.json_exists",
        [(TypeNull, CompareOperator.Equals)] = "dbo.json_null_equals",
        [(TypeNull, CompareOperator.NotEquals)] = "dbo.json_null_notequals",
        [(TypeText, CompareOperator.Contains)] = "dbo.json_text_contains",
        [(TypeText, CompareOperator.EndsWith)] = "dbo.json_text_endswith",
        [(TypeText, CompareOperator.Equals)] = "dbo.json_text_equals",
        [(TypeText, CompareOperator.GreaterThan)] = "dbo.json_text_greaterthan",
        [(TypeText, CompareOperator.GreaterThanOrEqual)] = "dbo.json_text_greaterthanorequal",
        [(TypeText, CompareOperator.LessThan)] = "dbo.json_text_lessthan",
        [(TypeText, CompareOperator.LessThanOrEqual)] = "dbo.json_text_lessthanorequal",
        [(TypeText, CompareOperator.Matchs)] = "dbo.json_text_matchs",
        [(TypeText, CompareOperator.NotEquals)] = "dbo.json_text_notequals",
        [(TypeText, CompareOperator.StartsWith)] = "dbo.json_text_startswith",
        [(TypeBoolean, CompareOperator.Equals)] = "dbo.json_boolean_equals",
        [(TypeBoolean, CompareOperator.NotEquals)] = "dbo.json_boolean_notequals",
        [(TypeNumber, CompareOperator.Equals)] = "dbo.json_number_equals",
        [(TypeNumber, CompareOperator.GreaterThan)] = "dbo.json_number_greaterthan",
        [(TypeNumber, CompareOperator.GreaterThanOrEqual)] = "dbo.json_number_greaterthanorequal",
        [(TypeNumber, CompareOperator.LessThan)] = "dbo.json_number_lessthan",
        [(TypeNumber, CompareOperator.LessThanOrEqual)] = "dbo.json_number_lessthanorequal",
        [(TypeNumber, CompareOperator.NotEquals)] = "dbo.json_number_notequals",
    };

    public static async Task InitializeAsync(DbContext dbContext,
        CancellationToken ct)
    {
        var sqlStream = typeof(SqlServerDialect).Assembly.GetManifestResourceStream("Squidex.Providers.SqlServer.json_function.sql");
        var sqlText = await new StreamReader(sqlStream!).ReadToEndAsync(ct);

        sqlText = sqlText.Replace("{", "{{", StringComparison.Ordinal);
        sqlText = sqlText.Replace("}", "}}", StringComparison.Ordinal);

        var statements = sqlText.Split(";;", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        // We want to filter out the drop statements and multiple function creations are not supported.
        foreach (var statement in statements)
        {
#if RELEASE
            if (statement.StartsWith("DROP", StringComparison.Ordinal))
            {
                continue;
            }
#endif
            await dbContext.Database.ExecuteSqlRawAsync(statement, ct);
        }
    }

    public static string Create(PropertyPath path, CompareOperator op, ClrValue value, string formattedValue)
    {
        var type = -1;
        if (op is CompareOperator.Exists or CompareOperator.Empty)
        {
            type = TypeAny;
        }
        else
        {
            switch (value.ValueType)
            {
                case ClrValueType.Single:
                case ClrValueType.Double:
                case ClrValueType.Int32:
                case ClrValueType.Int64:
                    type = TypeNumber;
                    break;
                case ClrValueType.Instant:
                case ClrValueType.Guid:
                case ClrValueType.String:
                    type = TypeText;
                    break;
                case ClrValueType.Boolean:
                    type = TypeBoolean;
                    break;
                case ClrValueType.Null:
                    type = TypeNull;
                    break;
            }
        }

        var jsonPath = path.JsonPath();

        // SQL Server scalar functions cannot accept parameterized arrays, embedding
        // SQL parameter references (@p0, @p1) inside a JSON string literal
        // produces invalid JSON at execution time. IN is therefore handled inline so
        // parameters remain proper SQL parameters.
        if (value.IsList)
        {
            return type switch
            {
                TypeText =>
                    $"""
                    (CASE WHEN JSON_QUERY({jsonPath}) IS NOT NULL AND LEFT(LTRIM(JSON_QUERY({jsonPath})), 1) = '['
                        THEN CASE WHEN EXISTS (SELECT 1 FROM OPENJSON(JSON_QUERY({jsonPath})) AS j WHERE j.[type] = 1 AND j.[value] IN ({formattedValue})) THEN 1 ELSE 0 END
                        WHEN JSON_QUERY({jsonPath}) IS NOT NULL THEN 0
                        ELSE CASE WHEN EXISTS (SELECT 1 FROM OPENJSON({jsonPath}) AS j WHERE j.[type] = 1 AND j.[value] IN ({formattedValue})) THEN 1 ELSE 0 END
                    END) = 1
                    """,
                TypeNumber =>
                    $"""
                    (CASE WHEN JSON_QUERY({jsonPath}) IS NOT NULL AND LEFT(LTRIM(JSON_QUERY({jsonPath})), 1) = '['
                        THEN CASE WHEN EXISTS (SELECT 1 FROM OPENJSON(JSON_QUERY({jsonPath})) AS j WHERE j.[type] = 2 AND TRY_CAST(j.[value] AS DECIMAL(38, 10)) IN ({formattedValue})) THEN 1 ELSE 0 END
                        WHEN JSON_QUERY({jsonPath}) IS NOT NULL THEN 0
                        ELSE CASE WHEN EXISTS (SELECT 1 FROM OPENJSON({jsonPath}) AS j WHERE j.[type] = 2 AND TRY_CAST(j.[value] AS DECIMAL(38, 10)) IN ({formattedValue})) THEN 1 ELSE 0 END
                    END) = 1
                    """,
                TypeBoolean =>
                    $"""
                    (CASE WHEN JSON_QUERY({jsonPath}) IS NOT NULL AND LEFT(LTRIM(JSON_QUERY({jsonPath})), 1) = '['
                        THEN CASE WHEN EXISTS (SELECT 1 FROM OPENJSON(JSON_QUERY({jsonPath})) AS j WHERE j.[type] = 3 AND IIF(j.[value] = 'true', 1, IIF(j.[value] = 'false', 0, NULL)) IN ({formattedValue})) THEN 1 ELSE 0 END
                        WHEN JSON_QUERY({jsonPath}) IS NOT NULL THEN 0
                        ELSE CASE WHEN EXISTS (SELECT 1 FROM OPENJSON({jsonPath}) AS j WHERE j.[type] = 3 AND IIF(j.[value] = 'true', 1, IIF(j.[value] = 'false', 0, NULL)) IN ({formattedValue})) THEN 1 ELSE 0 END
                    END) = 1
                    """,
                _ => throw new NotSupportedException($"No json function for type={type}, operator={op}."),
            };
        }

        if (!Functions.TryGetValue((type, op), out var fn))
        {
            throw new NotSupportedException($"No json function for type={type}, operator={op}.");
        }

        if (type is TypeNull or TypeAny)
        {
            return $"{fn}({jsonPath}) = 1";
        }

        if (value.IsList)
        {
            return $"{fn}({jsonPath}, N'[{formattedValue}]') = 1";
        }

        return $"{fn}({jsonPath}, {formattedValue}) = 1";
    }
}
