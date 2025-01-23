// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
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

    protected override string FormatField(PropertyPath path, ClrValue? value, bool isJson)
    {
        var baseField = path[0];

        var isBoolean = value?.ValueType is ClrValueType.Boolean;
        var issNumeric = value?.ValueType is
            ClrValueType.Single or
            ClrValueType.Double or
            ClrValueType.Int32 or
            ClrValueType.Int64;

        if (isJson && path.Count > 1)
        {
            var sb = new StringBuilder();
            sb.Append("(\"");
            sb.Append(baseField);
            sb.Append('"');

            var i = 1;
            foreach (var property in path.Skip(1))
            {
                if (i == path.Count - 1 && !issNumeric && !isBoolean && value != null)
                {
                    sb.Append("->>");
                }
                else
                {
                    sb.Append("->");
                }

                if (int.TryParse(property, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
                {
                    sb.Append(index);
                }
                else
                {
                    sb.Append($"'{property}'");
                }

                i++;
            }

            sb.Append(')');

            if (issNumeric)
            {
                sb.Append("::numeric");
            }
            else if (isBoolean)
            {
                sb.Append("::bool");
            }

            return sb.ToString();
        }

        return $"\"{baseField}\"";
    }
}
