// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
            var sqlPathCast = path.JsonPath(true);
            var sqlPath = path.JsonPath(false);
            var sqlOp = FormatOperator(op, value);
            var sqlRhs = FormatValues(op, value, queryParameters);

            string BuildCondition(string path, string castPath)
            {
                var isNumeric = value.ValueType is
                    ClrValueType.Single or
                    ClrValueType.Double or
                    ClrValueType.Int32 or
                    ClrValueType.Int64;

                if (isNumeric)
                {
                    return $"(CASE WHEN jsonb_typeof({path}) = 'number' THEN ({castPath})::numeric {sqlOp} {sqlRhs} ELSE FALSE END)";
                }

                var isBoolean = value.ValueType is ClrValueType.Boolean;
                if (isBoolean)
                {
                    return $"(CASE WHEN jsonb_typeof({path}) = 'boolean' THEN ({castPath})::boolean {sqlOp} {sqlRhs} ELSE FALSE END)";
                }

                var isString = value.ValueType is
                    ClrValueType.Instant or
                    ClrValueType.Guid or
                    ClrValueType.String;
                if (isString)
                {
                    return $"{path} #>> '{{{{}}}}' {sqlOp} {sqlRhs}";
                }

                var isNull = value.ValueType is ClrValueType.Null;
                if (isNull)
                {
                    var nullOp = FormatOperator(op, "null");
                    return $"jsonb_typeof({path}) {nullOp} 'null'";
                }

                return base.Where(path, op, value, queryParameters, false);
            }

            return $"""
                CASE WHEN jsonb_typeof({sqlPath}) = 'array'
                    THEN EXISTS (
                        SELECT 1
                        FROM jsonb_array_elements({sqlPath}) AS __element
                        WHERE {BuildCondition("__element", "__element")}
                    )
                    ELSE {BuildCondition(sqlPath, sqlPathCast)}
                END
                """;
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
