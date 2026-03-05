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
