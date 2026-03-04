// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Infrastructure.Queries;
using SqlException = Microsoft.Data.SqlClient.SqlException;

namespace Squidex.Providers.SqlServer;

public sealed class SqlServerDialect : SqlDialect
{
    public static readonly SqlDialect Instance = new SqlServerDialect();

    private SqlServerDialect()
    {
    }

    public override bool IsDuplicateIndexException(Exception exception, string name)
    {
        return exception is SqlException ex && ex.Number is 1913 or 7642 or 7652;
    }

    public override string GeoIndex(string name, string table, string field)
    {
        return $"CREATE SPATIAL INDEX {name} ON {FormatTable(table)} ({FormatField(field, false)}) USING GEOGRAPHY_GRID;";
    }

    public override string TextIndex(string name, string table, string field)
    {
        return $"CREATE FULLTEXT INDEX ON {FormatTable(table)} ({FormatField(field, false)}) KEY INDEX PK_{table} ON {name} WITH CHANGE_TRACKING AUTO;";
    }

    public override string TextIndexPrepare(string name)
    {
        return $"CREATE FULLTEXT CATALOG {name};";
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

    public override string WhereMatch(PropertyPath path, string query, SqlParams queryParameters)
    {
        if (query.Contains(' ', StringComparison.OrdinalIgnoreCase))
        {
            query = $"\"{query}\"";
        }

        return $"CONTAINS({FormatField(path, false)}, {queryParameters.AddPositional(query)})";
    }

    public override string Where(PropertyPath path, CompareOperator op, ClrValue value, SqlParams queryParameters, bool isJson)
    {
        if (isJson)
        {
            var sqlPath = path.JsonPath();
            var sqlOp = FormatOperator(op, value);
            var sqlRhs = FormatValues(op, value, queryParameters);

            var isNull = value.ValueType is ClrValueType.Null;
            var isBoolean = value.ValueType is ClrValueType.Boolean;
            var isNumeric = value.ValueType is
                ClrValueType.Single or
                ClrValueType.Double or
                ClrValueType.Int32 or
                ClrValueType.Int64;

            string ScalarCondition()
            {
                if (isNull)
                {
                    if (op == CompareOperator.Equals)
                    {
                        return $"JSON_QUERY({sqlPath}) IS NULL AND JSON_VALUE({sqlPath}) IS NULL";
                    }

                    if (op == CompareOperator.NotEquals)
                    {
                        return $"JSON_QUERY({sqlPath}) IS NOT NULL OR JSON_VALUE({sqlPath}) IS NOT NULL";
                    }
                }

                if (isNumeric)
                {
                    return $"TRY_CAST(JSON_VALUE({sqlPath}) AS NUMERIC) {sqlOp} {sqlRhs}";
                }

                if (isBoolean)
                {
                    return $"IIF(JSON_VALUE({sqlPath}) = 'true', 1, 0) {sqlOp} {sqlRhs}";
                }

                return base.Where(path, op, value, queryParameters, isJson);
            }

            string ArrayCondition(string field, string op)
            {
                if (isNumeric)
                {
                    return $"TRY_CAST({field} AS NUMERIC) {op} {sqlRhs}";
                }

                if (isBoolean)
                {
                    return $"IIF({field} = 'true', 1, 0) {op} {sqlRhs}";
                }

                return $"{field} = {sqlRhs}";
            }

            if (value.IsList && op == CompareOperator.In)
            {
                return $"""
                    CASE WHEN LEFT(JSON_QUERY({sqlPath}), 1) = '['
                        THEN (
                            SELECT COUNT(*)
                            FROM OPENJSON({sqlPath}) AS field_arr
                            WHERE {ArrayCondition("field_arr.value", "IN")}
                        )
                        ELSE (
                            SELECT COUNT(*)
                            WHERE {ScalarCondition()}
                        )
                    END > 0
                    """;
            }

            if (op == CompareOperator.Equals)
            {
                return $"""
                    CASE WHEN LEFT(JSON_QUERY({sqlPath}), 1) = '['
                        THEN (
                            SELECT COUNT(*)
                            FROM OPENJSON({sqlPath}) AS field_arr
                            WHERE {ArrayCondition("field_arr.value", "=")}
                        )
                        ELSE (
                            SELECT COUNT(*)
                            WHERE {ScalarCondition()}
                        )
                    END > 0
                    """;
            }

            return ScalarCondition();
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
