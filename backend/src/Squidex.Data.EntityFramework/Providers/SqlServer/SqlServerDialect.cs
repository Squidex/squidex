// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure.Queries;
using SqlException = Microsoft.Data.SqlClient.SqlException;

namespace Squidex.Providers.SqlServer;

public sealed class SqlServerDialect : SqlDialect
{
    public static readonly SqlDialect Instance = new SqlServerDialect();

    private SqlServerDialect()
    {
    }

    public override Task InitializeAsync(DbContext dbContext,
        CancellationToken ct)
    {
        return JsonFunction.InitializeAsync(dbContext, ct);
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

    public override IEnumerable<string> CreateIndex(string name, string table, IReadOnlyList<SqlIndexKey> keys, string description)
    {
        // SQL Server cannot index expressions, therefore we have to create computed columns first.
        var sqlComputed = keys.Select((x, i) => (Key: x, Column: $"{name}_{i}")).Where(x => x.Key.IsExpression).ToList();

        if (sqlComputed.Count > 0)
        {
            yield return $"ALTER TABLE {FormatTable(table)} ADD {string.Join(", ", sqlComputed.Select(x => $"{FormatTable(x.Column)} AS ({x.Key.Expression})"))};";
        }

        var sqlColumns = string.Join(", ", keys.Select((x, i) => x.IsExpression ? $"{FormatTable($"{name}_{i}")} {x.Order}" : $"{x.Expression} {x.Order}"));

        yield return $"CREATE INDEX {FormatTable(name)} ON {FormatTable(table)} ({sqlColumns});";
        yield return $"""
            DECLARE @schema sysname = SCHEMA_NAME();
            EXEC sp_addextendedproperty
                @name = N'squidex', @value = {FormatLiteral(description)},
                @level0type = N'SCHEMA', @level0name = @schema,
                @level1type = N'TABLE', @level1name = {FormatLiteral(table)},
                @level2type = N'INDEX', @level2name = {FormatLiteral(name)};
            """;
    }

    public override IEnumerable<string> DropIndex(string name, string table)
    {
        yield return $"DROP INDEX IF EXISTS {FormatTable(name)} ON {FormatTable(table)};";
        yield return $"""
            DECLARE @sql NVARCHAR(MAX) = N'';
            SELECT @sql = @sql + N'ALTER TABLE {FormatTable(table)} DROP COLUMN ' + QUOTENAME(name) + N';'
            FROM sys.computed_columns
            WHERE object_id = OBJECT_ID({FormatLiteral(FormatTable(table))}) AND name LIKE {FormatLiteral($"{name}[_]%")};
            EXEC sp_executesql @sql;
            """;
    }

    public override Task<List<SqlIndexInfo>> QueryIndexesAsync(DbContext dbContext, string table,
        CancellationToken ct)
    {
        var sqlQuery = $"""
            SELECT i.name AS {nameof(SqlIndexInfo.Name)}, CAST(p.value AS NVARCHAR(4000)) AS {nameof(SqlIndexInfo.Description)}
            FROM sys.indexes i
            LEFT JOIN sys.extended_properties p ON p.class = 7 AND p.major_id = i.object_id AND p.minor_id = i.index_id AND p.name = N'squidex'
            WHERE i.object_id = OBJECT_ID({FormatLiteral(FormatTable(table))}) AND i.name IS NOT NULL
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

        // Use TRY_CAST, because ISNUMERIC also accepts values like '$' that cannot be converted.
        return $"IIF(ISNUMERIC(JSON_VALUE({sqlPath})) = 1, TRY_CAST(JSON_VALUE({sqlPath}) AS NUMERIC), NULL)";
    }

    private static string SortText(PropertyPath path)
    {
        // Long texts would exceed the maximum size of index keys and the writes would fail.
        return $"CAST(JSON_VALUE({path.JsonPath()}) AS NVARCHAR(200))";
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

        return $"[{path[0]}]";
    }
}
