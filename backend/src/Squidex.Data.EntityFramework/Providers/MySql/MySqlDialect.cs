// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Squidex.Infrastructure.Queries;

namespace Squidex.Providers.MySql;

public sealed class MySqlDialect : SqlDialect
{
    public static readonly SqlDialect Instance = new MySqlDialect();

    private MySqlDialect()
    {
    }

    public override Task InitializeAsync(DbContext dbContext,
        CancellationToken ct)
    {
        return JsonFunction.InitializeAsync(dbContext, ct);
    }

    public override bool IsDuplicateIndexException(Exception exception, string name)
    {
        return exception is MySqlException ex && ex.Number == 1061;
    }

    public override string? JsonColumnType()
    {
        return "json";
    }

    public override string GeoIndex(string name, string table, string field)
    {
        return $"CREATE SPATIAL INDEX {name} ON {FormatTable(table)} ({FormatField(field, false)});";
    }

    public override string TextIndex(string name, string table, string field)
    {
        return $"CREATE FULLTEXT INDEX {name} ON {FormatTable(table)} ({FormatField(field, false)});";
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

    public override IEnumerable<string> CreateIndex(string name, string table, IReadOnlyList<SqlIndexKey> keys, string description)
    {
        var sqlColumns = string.Join(", ", keys.Select(x => x.IsExpression ? $"({x.Expression}) {x.Order}" : $"{x.Expression} {x.Order}"));

        yield return $"CREATE INDEX {FormatTable(name)} ON {FormatTable(table)} ({sqlColumns}) COMMENT {FormatLiteral(description)};";
    }

    public override IEnumerable<string> DropIndex(string name, string table)
    {
        yield return $"DROP INDEX {FormatTable(name)} ON {FormatTable(table)};";
    }

    public override Task<List<SqlIndexInfo>> QueryIndexesAsync(DbContext dbContext, string table,
        CancellationToken ct)
    {
        var sqlQuery = $"""
            SELECT INDEX_NAME AS {nameof(SqlIndexInfo.Name)}, MAX(INDEX_COMMENT) AS {nameof(SqlIndexInfo.Description)}
            FROM information_schema.STATISTICS
            WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = {FormatLiteral(table)}
            GROUP BY INDEX_NAME
            """;

        return dbContext.Database.SqlQueryRaw<SqlIndexInfo>(sqlQuery).ToListAsync(ct);
    }

    public override IEnumerable<SqlIndexKey> IndexKeys(PropertyPath path, SortOrder order, bool isJson)
    {
        if (isJson)
        {
            var sqlOrder = FormatOrder(order);

            // Must match the expressions of the ORDER BY clause, otherwise the index is not used.
            yield return new SqlIndexKey(SortNumber(path), sqlOrder, true);
            yield return new SqlIndexKey(SortText(path), sqlOrder, true);
            yield break;
        }

        foreach (var key in base.IndexKeys(path, order, isJson))
        {
            yield return key;
        }
    }

    public override string OrderBy(PropertyPath path, SortOrder order, bool isJson)
    {
        if (isJson)
        {
            var sqlOrder = FormatOrder(order);

            return $"{SortNumber(path)} {sqlOrder}, {SortText(path)} {sqlOrder}";
        }

        return base.OrderBy(path, order, isJson);
    }

    private static string SortNumber(PropertyPath path)
    {
        var sqlPath = path.JsonPath();

        return $"IF(JSON_TYPE(JSON_EXTRACT({sqlPath})) IN ('INTEGER', 'DOUBLE', 'DECIMAL'), CAST(JSON_VALUE({sqlPath}) AS DOUBLE), NULL)";
    }

    private static string SortText(PropertyPath path)
    {
        // Long texts would exceed the maximum size of index keys.
        return $"LEFT(JSON_VALUE({path.JsonPath()}), 200)";
    }

    public override string WhereMatch(PropertyPath path, string query, SqlParams queryParameters)
    {
        return $"MATCH({FormatField(path, false)}) AGAINST({queryParameters.AddPositional(query)})";
    }

    public override string Where(PropertyPath path, CompareOperator op, ClrValue value, SqlParams queryParameters, bool isJson)
    {
        if (isJson)
        {
            return JsonFunction.Create(path, op, value, FormatValues(op, value, queryParameters, true));
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
