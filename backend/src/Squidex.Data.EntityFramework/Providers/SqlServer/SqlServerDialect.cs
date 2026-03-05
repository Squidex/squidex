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

            string BuildCondition(string path, string queryPath)
            {
                var isNumeric = value.ValueType is
                    ClrValueType.Single or
                    ClrValueType.Double or
                    ClrValueType.Int32 or
                    ClrValueType.Int64;
                if (isNumeric)
                {
                    return $"TRY_CAST({path} AS NUMERIC) {sqlOp} {sqlRhs}";
                }

                var isBoolean = value.ValueType is ClrValueType.Boolean;
                if (isBoolean)
                {
                    return $"IIF({path} = 'true', 1, IIF({path} = 'false', 0, NULL)) {sqlOp} {sqlRhs}";
                }

                var isString = value.ValueType is
                    ClrValueType.Instant or
                    ClrValueType.Guid or
                    ClrValueType.String;
                if (isString)
                {
                    return $"{path} {sqlOp} {sqlRhs}";
                }

                var isNull = value.ValueType is ClrValueType.Null;
                if (isNull)
                {
                    if (op == CompareOperator.Equals)
                    {
                        return $"{queryPath} IS NULL AND {path} IS NULL";
                    }

                    if (op == CompareOperator.NotEquals)
                    {
                        return $"{queryPath} IS NOT NULL OR {path} IS NOT NULL";
                    }
                }

                return base.Where(path, op, value, queryParameters, false);
            }

            return $"""
                (
                    CASE WHEN LEFT(LTRIM(JSON_QUERY({sqlPath})), 1) = '['
                        THEN CASE WHEN EXISTS (
                            SELECT 1
                            FROM OPENJSON({sqlPath}) AS __element
                            WHERE {BuildCondition("__element.[value]", "__element.[value]")}
                        ) THEN 1 ELSE 0 END
                        ELSE CASE WHEN {BuildCondition($"JSON_VALUE({sqlPath})", $"JSON_QUERY({sqlPath})")} THEN 1 ELSE 0 END
                    END
                ) = 1
                """;
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
