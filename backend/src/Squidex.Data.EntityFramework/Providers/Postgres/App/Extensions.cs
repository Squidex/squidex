// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Text;
using Squidex.Infrastructure.Queries;

namespace Squidex.Providers.Postgres.App;

public static class Extensions
{
    public static StringBuilder AppendJsonPath(this StringBuilder sb, PropertyPath path, bool asString)
    {
        sb.Append('"');
        sb.Append(path[0]);
        sb.Append('"');

        var i = 1;
        foreach (var property in path.Skip(1))
        {
            if (i == path.Count - 1 && asString)
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

        return sb;
    }

    public static string JsonPath(this PropertyPath path, bool asString)
    {
        return new StringBuilder().AppendJsonPath(path, asString).ToString();
    }
}
