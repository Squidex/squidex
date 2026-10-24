// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Npgsql;
using Squidex.Infrastructure.Queries;
using Squidex.Providers.Postgres.App;

namespace Squidex.Providers.Postgres;

public class PostgresDialect : SqlDialect
{
    public static readonly SqlDialect Instance = new PostgresDialect();

    private PostgresDialect()
    {
    }

    public override Task InitializeAsync(DbContext dbContext,
        CancellationToken ct)
    {
        return JsonFunction.InitializeAsync(dbContext, ct);
    }

    public override bool IsDuplicateIndexException(Exception exception, string name)
    {
        return exception is PostgresException ex && ex.SqlState == "42P07";
    }

    public override string? JsonColumnType()
    {
        return "jsonb";
    }

    public override string GeoIndex(string name, string table, string field)
    {
        return $"CREATE INDEX {name} ON {FormatTable(table)} USING GIST ({FormatField(field, false)});";
    }

    public override string TextIndex(string name, string table, string field)
    {
        return $"CREATE INDEX {name} ON {FormatTable(table)} USING GIN (to_tsvector('simple', {FormatField(field, false)}));";
    }

    protected override string FormatTable(string tableName)
    {
        return $"\"{tableName}\"";
    }

    public override IEnumerable<string> CreateIndex(string name, string table, IReadOnlyList<SqlIndexKey> keys, string description)
    {
        var sqlColumns = string.Join(", ", keys.Select(x => x.IsExpression ? $"({x.Expression}) {x.Order}" : $"{x.Expression} {x.Order}"));

        yield return $"CREATE INDEX {FormatTable(name)} ON {FormatTable(table)} ({sqlColumns});";
        yield return $"COMMENT ON INDEX {FormatTable(name)} IS {FormatLiteral(description)};";
    }

    public override IEnumerable<string> DropIndex(string name, string table)
    {
        yield return $"DROP INDEX IF EXISTS {FormatTable(name)};";
    }

    public override Task<List<SqlIndexInfo>> QueryIndexesAsync(DbContext dbContext, string table,
        CancellationToken ct)
    {
        var sqlQuery = $"""
            SELECT i.relname AS "{nameof(SqlIndexInfo.Name)}", obj_description(i.oid, 'pg_class') AS "{nameof(SqlIndexInfo.Description)}"
            FROM pg_index x
            JOIN pg_class i ON i.oid = x.indexrelid
            JOIN pg_class t ON t.oid = x.indrelid
            WHERE t.relname = {FormatLiteral(table)} AND t.relnamespace = current_schema()::regnamespace
            """;

        return dbContext.Database.SqlQueryRaw<SqlIndexInfo>(sqlQuery).ToListAsync(ct);
    }

    public override IEnumerable<SqlIndexKey> IndexKeys(PropertyPath path, SortOrder order, bool isJson)
    {
        if (isJson)
        {
            var sqlOrder = FormatOrder(order);

            // Must match the expressions of the ORDER BY clause, otherwise the index is not used.
            yield return new SqlIndexKey(SortNumber(path), $"{sqlOrder} NULLS LAST", true);
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

            return $"{SortNumber(path)} {sqlOrder} NULLS LAST, {SortText(path)} {sqlOrder}";
        }

        return base.OrderBy(path, order, isJson);
    }

    private static string SortNumber(PropertyPath path)
    {
        return $"CASE WHEN jsonb_typeof({path.JsonPath(false)}) = 'number' THEN ({path.JsonPath(true)})::numeric END";
    }

    private static string SortText(PropertyPath path)
    {
        // Long texts would exceed the maximum size of btree index entries and the writes would fail.
        return $"left({path.JsonPath(true)}, 200)";
    }

    public override string WhereMatch(PropertyPath path, string query, SqlParams queryParameters)
    {
        if (query.Contains(' ', StringComparison.OrdinalIgnoreCase))
        {
            query = query.Replace(" ", " & ", StringComparison.Ordinal);
        }

        return $"to_tsvector('simple', {FormatField(path, false)}) @@ to_tsquery({queryParameters.AddPositional(query)})";
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
        var baseField = path[0];
        if (isJson && path.Count > 1)
        {
            return path.JsonPath(true);
        }

        if (baseField == "__element" || baseField.Contains("->", StringComparison.Ordinal))
        {
            return baseField;
        }

        return $"\"{baseField}\"";
    }
}
