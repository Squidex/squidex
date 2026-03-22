// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure.Queries;

namespace Squidex.Providers.MySql;

public static class JsonFunction
{
    private const int TypeAny = 0;
    private const int TypeNull = 1;
    private const int TypeText = 2;
    private const int TypeBoolean = 3;
    private const int TypeNumber = 4;

    private static readonly Dictionary<(int Type, CompareOperator Operator), string> Functions = new ()
    {
        [(TypeAny, CompareOperator.Empty)] = "json_empty",
        [(TypeAny, CompareOperator.Exists)] = "json_exists",
        [(TypeNull, CompareOperator.Equals)] = "json_null_equals",
        [(TypeNull, CompareOperator.NotEquals)] = "json_null_notequals",
        [(TypeText, CompareOperator.Contains)] = "json_text_contains",
        [(TypeText, CompareOperator.EndsWith)] = "json_text_endswith",
        [(TypeText, CompareOperator.Equals)] = "json_text_equals",
        [(TypeText, CompareOperator.GreaterThan)] = "json_text_greaterthan",
        [(TypeText, CompareOperator.GreaterThanOrEqual)] = "json_text_greaterthanorequal",
        [(TypeText, CompareOperator.In)] = "json_text_in",
        [(TypeText, CompareOperator.LessThan)] = "json_text_lessthan",
        [(TypeText, CompareOperator.LessThanOrEqual)] = "json_text_lessthanorequal",
        [(TypeText, CompareOperator.Matchs)] = "json_text_matchs",
        [(TypeText, CompareOperator.NotEquals)] = "json_text_notequals",
        [(TypeText, CompareOperator.StartsWith)] = "json_text_startswith",
        [(TypeBoolean, CompareOperator.Equals)] = "json_boolean_equals",
        [(TypeBoolean, CompareOperator.In)] = "json_boolean_in",
        [(TypeBoolean, CompareOperator.NotEquals)] = "json_boolean_notequals",
        [(TypeNumber, CompareOperator.Equals)] = "json_number_equals",
        [(TypeNumber, CompareOperator.GreaterThan)] = "json_number_greaterthan",
        [(TypeNumber, CompareOperator.GreaterThanOrEqual)] = "json_number_greaterthanorequal",
        [(TypeNumber, CompareOperator.In)] = "json_number_in",
        [(TypeNumber, CompareOperator.LessThan)] = "json_number_lessthan",
        [(TypeNumber, CompareOperator.LessThanOrEqual)] = "json_number_lessthanorequal",
        [(TypeNumber, CompareOperator.NotEquals)] = "json_number_notequals",
    };

    public static async Task InitializeAsync(DbContext dbContext,
        CancellationToken ct)
    {
        var sqlStream = typeof(MySqlDialect).Assembly.GetManifestResourceStream("Squidex.Providers.MySql.json_function.sql");
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

        if (!Functions.TryGetValue((type, op), out var fn))
        {
            throw new NotSupportedException($"No jsonb function for type={value.ValueType}, operator={op}.");
        }

        if (type is TypeNull or TypeAny)
        {
            return $"{fn}(`{path[0]}`, {path.JsonSubPath()})";
        }

        var arg = formattedValue;
        if (value.IsList)
        {
            arg = $"JSON_ARRAY({formattedValue})";
        }

        return $"{fn}(`{path[0]}`, {path.JsonSubPath()}, {arg})";
    }
}
