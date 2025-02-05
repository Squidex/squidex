// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Queries;

namespace Squidex.Providers.Postgres;

public class PostgresDialect : SqlDialect
{
    public static readonly SqlDialect Instance = new PostgresDialect();

    private PostgresDialect()
    {
    }

    protected override string FormatTable(string tableName)
    {
        return $"\"{tableName}\"";
    }

    public override string OrderBy(PropertyPath path, SortOrder order, bool isJson)
    {
        if (isJson)
        {
            var sqlOrder = FormatOrder(order);
            var sqlPath = path.JsonPath(true);

            return $"CASE WHEN jsonb_typeof({path.JsonPath(false)}) = 'number' THEN ({sqlPath})::numeric END {sqlOrder} NULLS LAST, {sqlPath} {sqlOrder}";
        }

        return base.OrderBy(path, order, isJson);
    }

    public override string Where(PropertyPath path, CompareOperator op, ClrValue value, SqlParams queryParameters, bool isJson)
    {
        if (isJson)
        {
            var issNumeric = value.ValueType is
                ClrValueType.Single or
                ClrValueType.Double or
                ClrValueType.Int32 or
                ClrValueType.Int64;
            if (issNumeric)
            {
                var sqlOp = FormatOperator(op, value);
                var sqlRhs = FormatValues(op, value, queryParameters);

                return $"(CASE WHEN jsonb_typeof({path.JsonPath(false)}) = 'number' THEN ({path.JsonPath(true)})::numeric {sqlOp} {sqlRhs} ELSE FALSE END)";
            }

            var isBoolean = value.ValueType is ClrValueType.Boolean;
            if (isBoolean)
            {
                var sqlOp = FormatOperator(op, value);
                var sqlRhs = FormatValues(op, value, queryParameters);

                return $"(CASE WHEN jsonb_typeof({path.JsonPath(false)}) = 'boolean' THEN ({path.JsonPath(true)})::boolean {sqlOp} {sqlRhs} ELSE FALSE END)";
            }
        }

        return base.Where(path, op, value, queryParameters, isJson);
    }

    protected override string FormatField(PropertyPath path, bool isJson)
    {
        var baseField = path[0];

        if (isJson && path.Count > 1)
        {
            return path.JsonPath(true);
        }

        return $"\"{baseField}\"";
    }
}
