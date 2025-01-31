// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Queries;

namespace Squidex.Providers.MySql;

public sealed class MySqlDialect : SqlDialect
{
    public static readonly SqlDialect Instance = new MySqlDialect();

    private MySqlDialect()
    {
    }

    public override string FormatLimitOffset(long limit, long offset, bool hasOrder)
    {
        var hasLimit = limit > 0 && limit < long.MaxValue;

        if (offset > 0)
        {
            return $"LIMIT {limit} OFFSET {offset}";
        }

        if (offset > 0)
        {
            return $"LIMIT 18446744073709551615 OFFSET {offset}";
        }

        if (hasLimit)
        {
            return $"LIMIT {limit}";
        }

        return string.Empty;
    }

    protected override string FormatTable(string tableName)
    {
        return $"`{tableName}`";
    }

    protected override object FormatRawValue(object value, CompareOperator op)
    {
        switch (value)
        {
            case true:
                return 1;
            case false:
                return 0;
            default:
                return base.FormatRawValue(value, op);
        }
    }

    public override string OrderBy(PropertyPath path, SortOrder order, bool isJson)
    {
        if (isJson)
        {
            var sqlOrder = FormatOrder(order);
            var sqlPath = path.JsonPath();

            return $"IF(JSON_TYPE(JSON_EXTRACT({sqlPath})) IN ('INTEGER', 'DOUBLE', 'DECIMAL'), CAST(JSON_VALUE({sqlPath}) AS DOUBLE), NULL) {sqlOrder}, JSON_VALUE({sqlPath}) {sqlOrder}";
        }

        return base.OrderBy(path, order, isJson);
    }

    public override string Where(PropertyPath path, CompareOperator op, ClrValue value, SqlParams queryParameters, bool isJson)
    {
        if (isJson)
        {
            var isBoolean = value.ValueType is ClrValueType.Boolean;
            if (isBoolean)
            {
                var sqlPath = path.JsonPath();
                var sqlOp = FormatOperator(op, value);
                var sqlRhs = FormatValues(op, value, queryParameters);

                return $"IF(JSON_VALUE({sqlPath}) = 'true', 1, 0) {sqlOp} {sqlRhs}";
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

        return $"`{path[0]}`";
    }
}
