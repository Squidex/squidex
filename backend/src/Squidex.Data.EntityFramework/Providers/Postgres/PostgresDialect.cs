// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
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
            var sb = new StringBuilder();
            sb.Append("CASE WHEN ");
            sb.Append("jsonb_typeof(");
            sb.AppendJsonPath(path, false);
            sb.Append(')');
            sb.Append(" = 'number'");
            sb.Append(" THEN ");
            sb.Append('(');
            sb.AppendJsonPath(path, true);
            sb.Append(')');
            sb.Append("::numeric");
            sb.Append(" END ");
            sb.Append(FormatOrder(order));
            sb.Append(" NULLS LAST,");
            sb.AppendJsonPath(path, true);
            sb.Append(' ');
            sb.Append(FormatOrder(order));
            return sb.ToString();
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
                var sb = new StringBuilder();
                sb.Append('(');
                sb.Append("CASE WHEN ");
                sb.Append("jsonb_typeof(");
                sb.AppendJsonPath(path, false);
                sb.Append(')');
                sb.Append(" = 'number' THEN ");
                sb.Append('(');
                sb.AppendJsonPath(path, true);
                sb.Append(')');
                sb.Append("::numeric ");
                sb.Append(FormatOperator(op, value));
                sb.Append(' ');
                sb.Append(FormatValues(op, value, queryParameters));
                sb.Append(" ELSE FALSE END");
                sb.Append(')');
                return sb.ToString();
            }

            var isBoolean = value.ValueType is ClrValueType.Boolean;
            if (isBoolean)
            {
                var sb = new StringBuilder();
                sb.Append('(');
                sb.Append("CASE WHEN ");
                sb.Append("jsonb_typeof(");
                sb.AppendJsonPath(path, false);
                sb.Append(')');
                sb.Append(" = 'boolean' THEN ");
                sb.Append('(');
                sb.AppendJsonPath(path, true);
                sb.Append(')');
                sb.Append("::boolean ");
                sb.Append(FormatOperator(op, value));
                sb.Append(' ');
                sb.Append(FormatValues(op, value, queryParameters));
                sb.Append(" ELSE FALSE END");
                sb.Append(')');
                return sb.ToString();
            }
        }

        return base.Where(path, op, value, queryParameters, isJson);
    }

    protected override string FormatField(PropertyPath path, ClrValue? value, bool isJson)
    {
        var baseField = path[0];

        if (isJson && path.Count > 1)
        {
            return new StringBuilder().AppendJsonPath(path, true).ToString();
        }

        return $"\"{baseField}\"";
    }
}
