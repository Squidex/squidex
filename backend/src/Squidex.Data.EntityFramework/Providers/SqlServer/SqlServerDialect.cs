// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Text;
using Squidex.Infrastructure.Queries;

namespace Squidex.Providers.SqlServer;

public sealed class SqlServerDialect : SqlDialect
{
    public static readonly SqlDialect Instance = new SqlServerDialect();

    private SqlServerDialect()
    {
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

    protected override string FormatField(PropertyPath path, ClrValue? value, bool isJson)
    {
        if (isJson && path.Count > 1)
        {
            var sb = new StringBuilder();

            if (value == null)
            {
                sb.Append("IIF(");
                sb.Append("ISNUMERIC(");
                sb.Append("JSON_VALUE(");
                sb.AppendJsonPath(path);
                sb.Append(')'); // END JSON_VALUE
                sb.Append(')'); // END ISNUMERIC
                sb.Append(" = 1,");
                sb.Append("FORMAT(");
                sb.Append("CAST(");
                sb.Append("JSON_VALUE(");
                sb.AppendJsonPath(path);
                sb.Append(')'); // END JSON_VALUE
                sb.Append(" as DECIMAL");
                sb.Append(')'); // END CAST
                sb.Append(", '000000000000000.00000'");
                sb.Append(')'); // END FORMAT
                sb.Append(", ");
                sb.Append("JSON_VALUE(");
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

        return $"[{path[0]}]";
    }
}

#pragma warning disable MA0048 // File name must match type name
internal static class SqlServerDialectExtensions
#pragma warning restore MA0048 // File name must match type name
{
    public static void AppendJsonPath(this StringBuilder sb, PropertyPath path)
    {
        sb.Append('[');
        sb.Append(path[0]);
        sb.Append("], \'$");

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
