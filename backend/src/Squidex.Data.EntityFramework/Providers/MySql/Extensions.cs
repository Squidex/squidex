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
        return sb;
    }

    public static string JsonPath(this PropertyPath path)
    {
        return new StringBuilder().AppendJsonPath(path).ToString();
    }
}
