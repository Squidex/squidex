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
#pragma warning disable MA0048 // File name must match type name
internal static class Extensions
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
