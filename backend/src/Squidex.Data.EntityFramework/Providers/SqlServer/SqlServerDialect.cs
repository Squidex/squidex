// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Queries;

namespace Squidex.Providers.SqlServer;

public sealed class SqlServerDialect : SqlDialect
{
    public static readonly SqlDialect Instance = new SqlServerDialect();

    private SqlServerDialect()
    {
    }

    public override string FormatLimitOffset(long limit, long offset, bool hasOrder)
    {
        var hasLimit = limit > 0 && limit < long.MaxValue;

        if (hasLimit)
        {
            return $"OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY";
        }

        if (offset > 0 || hasOrder)
        {
            return $"OFFSET {offset} ROWS";
        }

        return string.Empty;
    }

    protected override string FormatTable(string tableName)
    {
        return $"[{tableName}]";
    }

    public override string OrderBy(PropertyPath path, SortOrder order, bool isJson)
    {
        if (isJson)
        {
            var sqlOrder = FormatOrder(order);
            var sqlPath = path.JsonPath();

            return $"IIF(ISNUMERIC(JSON_VALUE({sqlPath})) = 1, CAST(JSON_VALUE({sqlPath}) AS NUMERIC), NULL) {sqlOrder}, JSON_VALUE({sqlPath}) {sqlOrder}";
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
                var sqlPath = path.JsonPath();
                var sqlOp = FormatOperator(op, value);
                var sqlRhs = FormatValues(op, value, queryParameters);

                return $"TRY_CAST(JSON_VALUE({sqlPath}) AS NUMERIC) {sqlOp} {sqlRhs}";
            }

            var isBoolean = value.ValueType is ClrValueType.Boolean;
            if (isBoolean)
            {
                var sqlPath = path.JsonPath();
                var sqlOp = FormatOperator(op, value);
                var sqlRhs = FormatValues(op, value, queryParameters);

                return $"IIF(JSON_VALUE({sqlPath}) = 'true', 1, 0) {sqlOp} {sqlRhs}";
            }

            var isNull = value.ValueType is ClrValueType.Null;
            if (isNull)
            {
                var sqlPath = path.JsonPath();

                if (op == CompareOperator.Equals)
                {
                    return $"JSON_QUERY({sqlPath}) IS NULL AND JSON_VALUE({sqlPath}) IS NULL";
                }

                if (op == CompareOperator.NotEquals)
                {
                    return $"JSON_QUERY({sqlPath}) IS NOT NULL OR JSON_VALUE({sqlPath}) IS NOT NULL";
                }
            }
        }

        return base.Where(path, op, value, queryParameters, isJson);
    }

    protected override string FormatField(PropertyPath path, bool isJson)
    {
        if (isJson && path.Count > 1)
        {
            return $"JSON_VALUE({path.JsonPath()})";
        }

        return $"[{path[0]}]";
    }
}
