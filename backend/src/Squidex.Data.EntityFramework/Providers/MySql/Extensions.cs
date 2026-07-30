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

internal static class Extensions
{
    public static StringBuilder AppendJsonPath(this StringBuilder sb, PropertyPath path)
    {
        sb.Append('`');
        // Escape embedded backticks so a crafted path segment cannot break out of the identifier.
        sb.Append(path[0].Replace("`", "``", StringComparison.Ordinal));
        sb.Append("`, ");
        sb.AppendJsonPropertyPath(path);
        return sb;
    }

    public static StringBuilder AppendJsonPropertyPath(this StringBuilder sb, PropertyPath path)
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
    // member inside a single-quoted SQL string literal. Escape double-quotes/backslashes at the
    // JSON-path level, then backslashes/single-quotes at the MySQL string-literal level. MySQL treats
    // the backslash as a string-literal escape character, so the JSON-path escapes must themselves be
    // escaped again to survive string-literal parsing. This prevents SQL injection.
    private static string EscapeProperty(string property)
    {
        return property
            // JSON path escaping.
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            // MySQL string-literal escaping.
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("'", "''", StringComparison.Ordinal);
    }

    public static string JsonSubPath(this PropertyPath path)
    {
        return new StringBuilder().AppendJsonPropertyPath(path).ToString();
    }

    public static string JsonPath(this PropertyPath path)
    {
        return new StringBuilder().AppendJsonPath(path).ToString();
    }
}
