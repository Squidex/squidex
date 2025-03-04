// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using System.Text;

namespace Squidex.Infrastructure.Queries;

public class SqlDialect
{
    private const string Tab = "  ";

    public virtual bool IsDuplicateIndexException(Exception exception, string name)
    {
        return false;
    }

    public virtual string SelectTables()
    {
        return string.Empty;
    }

    public virtual string BuildSelectStatement(SqlQuery request)
    {
        var sb = new StringBuilder("SELECT");

        sb.AppendLines(request.Fields, Tab);
        sb.AppendLine($"FROM {FormatTable(request.Table)}");

        if (request.Where.Count > 0)
        {
            var query = And(request.Where);

            sb.AppendLine("WHERE");
            sb.Append(Tab);
            sb.Append(query);
            sb.AppendLine();
        }

        if (request.Order.Count > 0)
        {
            sb.Append("ORDER BY");
            sb.AppendLines(request.Order, Tab);
        }

        var pagination = FormatLimitOffset(request.Limit, request.Offset, request.Order.Count > 0);

        if (!string.IsNullOrEmpty(pagination))
        {
            sb.AppendLine(pagination);
        }

        return sb.ToString();
    }

    public virtual string GeoIndex(string name, string table, string field)
    {
        throw new NotSupportedException();
    }

    public virtual string TextIndex(string name, string table, string field)
    {
        throw new NotSupportedException();
    }

    public virtual string TextIndexPrepare(string name)
    {
        return string.Empty;
    }

    public virtual string FormatLimitOffset(long limit, long offset, bool hasOrder)
    {
        var hasLimit = limit > 0 && limit < long.MaxValue;

        if (hasLimit && offset > 0)
        {
            return $"LIMIT {limit} OFFSET {offset}";
        }

        if (offset > 0)
        {
            return $"OFFSET {offset}";
        }

        if (hasLimit)
        {
            return $"LIMIT {limit}";
        }

        return string.Empty;
    }

    public virtual string? JsonColumnType()
    {
        return null;
    }

    public virtual string And(IEnumerable<string> parts)
    {
        return $"({string.Join(" AND ", parts)})";
    }

    public virtual string Or(IEnumerable<string> parts)
    {
        return $"({string.Join(" OR ", parts)})";
    }

    public virtual string Not(string part)
    {
        return $"NOT ({part})";
    }

    public virtual string SelectAll()
    {
        return "*";
    }

    public virtual string CountAll()
    {
        return $"COUNT(*) as {Field("Value", false)}";
    }

    public virtual string Field(PropertyPath path, bool isJson)
    {
        return $"{FormatField(path, isJson)}";
    }

    public virtual string OrderBy(PropertyPath path, SortOrder order, bool isJson)
    {
        return $"{FormatField(path, isJson)} {FormatOrder(order)}";
    }

    public virtual string Where(PropertyPath path, CompareOperator op, ClrValue value, SqlParams queryParameters, bool isJson)
    {
        return $"{FormatField(path, isJson)} {FormatOperator(op, value)} {FormatValues(op, value, queryParameters)}";
    }

    public virtual string WhereQuery(PropertyPath path, CompareOperator op, string query, bool isJson)
    {
        return $"{FormatField(path, isJson)} {FormatOperator(op, ClrValue.Null)} ({query})";
    }

    public virtual string WhereMatch(PropertyPath path, string query, SqlParams queryParameters)
    {
        throw new NotSupportedException();
    }

    protected virtual string FormatValues(CompareOperator op, ClrValue value, SqlParams queryParameters)
    {
        if (!value.IsList && value.ValueType == ClrValueType.Null)
        {
            return "NULL";
        }

        string[] parameters;
        if (value.IsList && value.Value is IEnumerable list)
        {
            parameters = list.Cast<object>().Select(AddParameter).ToArray();
        }
        else
        {
            parameters = [AddParameter(value.Value!)];
        }

        string AddParameter(object value)
        {
            value = FormatRawValue(value, op);

            return queryParameters.AddPositional(value);
        }

        if (op == CompareOperator.In)
        {
            return $"({string.Join(", ", parameters)})";
        }

        return parameters[0];
    }

    protected virtual object FormatRawValue(object value, CompareOperator op)
    {
        switch (op)
        {
            case CompareOperator.StartsWith:
                value = $"{value}%";
                break;
            case CompareOperator.EndsWith:
                value = $"%{value}";
                break;
            case CompareOperator.Contains:
                value = $"%{value}%";
                break;
        }

        return value;
    }

    protected virtual string FormatOperator(CompareOperator op, ClrValue value)
    {
        switch (op)
        {
            case CompareOperator.Equals when value.ValueType == ClrValueType.Null:
                return "IS";
            case CompareOperator.Equals:
                return "=";
            case CompareOperator.NotEquals when value.ValueType == ClrValueType.Null:
                return "IS NOT";
            case CompareOperator.NotEquals:
                return "!=";
            case CompareOperator.LessThan:
                return "<";
            case CompareOperator.LessThanOrEqual:
                return "<=";
            case CompareOperator.Contains:
                return "LIKE";
            case CompareOperator.EndsWith:
                return "LIKE";
            case CompareOperator.StartsWith:
                return "LIKE";
            case CompareOperator.GreaterThan:
                return ">";
            case CompareOperator.GreaterThanOrEqual:
                return ">=";
            case CompareOperator.In:
                return "IN";
            default:
                ThrowHelper.NotSupportedException();
                return null!;
        }
    }

    protected virtual string FormatOrder(SortOrder order)
    {
        switch (order)
        {
            case SortOrder.Ascending:
                return "ASC";
            case SortOrder.Descending:
                return "DESC";
            default:
                throw new ArgumentException("Invalid enum value.", nameof(order));
        }
    }

    protected virtual string FormatTable(string tableName)
    {
        return tableName;
    }

    protected virtual string FormatField(PropertyPath path, bool isJson)
    {
        if (isJson)
        {
            throw new InvalidOperationException("JSON is not supported by basic dialect.");
        }

        return path[0];
    }
}
