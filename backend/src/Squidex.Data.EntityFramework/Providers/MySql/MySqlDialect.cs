// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Text;
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

    protected override string FormatField(PropertyPath path, ClrValue? value, bool isJson)
    {
        var isBoolean = value?.ValueType is ClrValueType.Boolean;

        if (isJson && path.Count > 1)
        {
            var sb = new StringBuilder();

            if (isBoolean)
            {
                sb.Append("IF(");
                sb.Append("JSON_VALUE(");
                sb.AppendJsonPath(path);
                sb.Append(')'); // END JSON_VALUE
                sb.Append(" = 'true', 1, 0");
                sb.Append(')'); // END IF
            }
            else if (value == null)
            {
                sb.Append("IF(");
                sb.Append("JSON_TYPE(");
                sb.Append("JSON_EXTRACT(");
                sb.AppendJsonPath(path);
                sb.Append(')'); // END JSON_VALUE
                sb.Append(')'); // END JSON_TYPE
                sb.Append(" IN ('INTEGER', 'DOUBLE', 'DECIMAL'), ");
                sb.Append("LPAD(");
                sb.Append("FORMAT(");
                sb.Append("JSON_VALUE(");
                sb.AppendJsonPath(path);
                sb.Append(')'); // END JSON_VALUE
                sb.Append(", 6");
                sb.Append(')'); // END FORMAT
                sb.Append(", 20, '0'");
                sb.Append(')'); // END LPAD
                sb.Append(", ");
                sb.Append("JSON_EXTRACT(");
                sb.AppendJsonPath(path);
                sb.Append(')');
                sb.Append(')'); // END IF
            }
            else
            {
                sb.Append("JSON_VALUE(");
                sb.AppendJsonPath(path);
                sb.Append(')');
            }

            return sb.ToString();
        }

        return $"`{path[0]}`";
    }
}

#pragma warning disable MA0048 // File name must match type name
internal static class MySqlDialectExtensions
#pragma warning restore MA0048 // File name must match type name
{
    public static void AppendJsonPath(this StringBuilder sb, PropertyPath path)
    {
        sb.Append('`');
        sb.Append(path[0]);
        sb.Append("`, \'$");

        foreach (var property in path.Skip(1))
        {
            if (int.TryParse(property, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
            {
                sb.Append(CultureInfo.InvariantCulture, $"[{index}]");
            }
            else
            {
                sb.Append('.');
                sb.Append(property);
            }
        }

        sb.Append('\'');
    }
}
