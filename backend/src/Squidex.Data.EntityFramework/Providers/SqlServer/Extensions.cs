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

internal static class Extensions
{
    public static StringBuilder AppendJsonPath(this StringBuilder sb, PropertyPath path)
    {
        sb.Append('[');
        // Escape embedded closing brackets so a crafted path segment cannot break out of the identifier.
        sb.Append(path[0].Replace("]", "]]", StringComparison.Ordinal));
        sb.Append("], ");
        sb.AppendJsonSubPath(path);
        return sb;
    }

    public static StringBuilder AppendJsonSubPath(this StringBuilder sb, PropertyPath path)
    {
        sb.Append("\'$");

        foreach (var property in path.Skip(1))
        {
            if (int.TryParse(property, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
            {
                sb.Append(CultureInfo.InvariantCulture, $"[{index}]");
            }
            else
            {
                sb.Append('.');
                sb.Append('"');
                sb.Append(EscapeProperty(property));
                sb.Append('"');
            }
        }

        sb.Append('\'');
        return sb;
    }

    // The property name is a user-controlled JSON path segment that is embedded as a double-quoted
    // member inside a single-quoted SQL string literal. Escape backslashes and double-quotes at the
    // JSON-path level and single-quotes at the SQL-literal level to prevent SQL injection. SQL Server
    // does not treat the backslash as a string-literal escape character, so the JSON-path escapes
    // reach the JSON parser verbatim.
    private static string EscapeProperty(string property)
    {
        return property
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("'", "''", StringComparison.Ordinal);
    }

    public static string JsonSubPath(this PropertyPath path)
    {
        return new StringBuilder().AppendJsonSubPath(path).ToString();
    }

    public static string JsonPath(this PropertyPath path)
    {
        return new StringBuilder().AppendJsonPath(path).ToString();
    }
}
