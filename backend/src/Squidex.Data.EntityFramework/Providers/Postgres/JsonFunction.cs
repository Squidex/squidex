// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure.Queries;
using Squidex.Providers.Postgres.App;

namespace Squidex.Providers.Postgres;

public static class JsonFunction
{
    private const int TypeAny = 0;
    private const int TypeNull = 1;
    private const int TypeText = 2;
    private const int TypeBoolean = 3;
    private const int TypeNumber = 4;

    private static readonly Dictionary<(int Type, CompareOperator Operator), string> Functions = new()
    {
        [(TypeAny, CompareOperator.Empty)] = "jsonb_empty",
        [(TypeAny, CompareOperator.Exists)] = "jsonb_exists",
        [(TypeNull, CompareOperator.Equals)] = "jsonb_null_equals",
        [(TypeNull, CompareOperator.NotEquals)] = "jsonb_null_notequals",
        [(TypeText, CompareOperator.Contains)] = "jsonb_text_contains",
        [(TypeText, CompareOperator.EndsWith)] = "jsonb_text_endswith",
        [(TypeText, CompareOperator.Equals)] = "jsonb_text_equals",
        [(TypeText, CompareOperator.GreaterThan)] = "jsonb_text_greaterthan",
        [(TypeText, CompareOperator.GreaterThanOrEqual)] = "jsonb_text_greaterthanorequal",
        [(TypeText, CompareOperator.In)] = "jsonb_text_in",
        [(TypeText, CompareOperator.LessThan)] = "jsonb_text_lessthan",
        [(TypeText, CompareOperator.LessThanOrEqual)] = "jsonb_text_lessthanorequal",
        [(TypeText, CompareOperator.Matchs)] = "jsonb_text_matchs",
        [(TypeText, CompareOperator.NotEquals)] = "jsonb_text_notequals",
        [(TypeText, CompareOperator.StartsWith)] = "jsonb_text_startswith",
        [(TypeBoolean, CompareOperator.Equals)] = "jsonb_boolean_equals",
        [(TypeBoolean, CompareOperator.In)] = "jsonb_boolean_in",
        [(TypeBoolean, CompareOperator.NotEquals)] = "jsonb_boolean_notequals",
        [(TypeNumber, CompareOperator.Equals)] = "jsonb_number_equals",
        [(TypeNumber, CompareOperator.GreaterThan)] = "jsonb_number_greaterthan",
        [(TypeNumber, CompareOperator.GreaterThanOrEqual)] = "jsonb_number_greaterthanorequal",
        [(TypeNumber, CompareOperator.In)] = "jsonb_number_in",
        [(TypeNumber, CompareOperator.LessThan)] = "jsonb_number_lessthan",
        [(TypeNumber, CompareOperator.LessThanOrEqual)] = "jsonb_number_lessthanorequal",
        [(TypeNumber, CompareOperator.NotEquals)] = "jsonb_number_notequals",
    };

    public static async Task InitializeAsync(DbContext dbContext,
        CancellationToken ct)
    {
        var sqlStream = typeof(PostgresDialect).Assembly.GetManifestResourceStream("Squidex.Providers.Postgres.json_function.sql");
        var sqlText = await new StreamReader(sqlStream!).ReadToEndAsync(ct);

        sqlText = sqlText.Replace("{", "{{", StringComparison.Ordinal);
        sqlText = sqlText.Replace("}", "}}", StringComparison.Ordinal);

        await dbContext.Database.ExecuteSqlRawAsync(sqlText, ct);
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
            return $"{fn}({path.JsonPath(false)})";
        }

        if (value.IsList)
        {
            return $"{fn}({path.JsonPath(false)}, ARRAY[{formattedValue}])";
        }

        return $"{fn}({path.JsonPath(false)}, {formattedValue})";
    }
}
