// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;

namespace Squidex.Infrastructure.Queries;

public static class Extensions
{
    public static void AppendLines(this StringBuilder sb, List<string> lines, string tab)
    {
        sb.AppendLine();
        sb.Append(tab);
        sb.Append(lines[0]);

        foreach (var line in lines.Skip(1))
        {
            sb.Append(',');
            sb.AppendLine();
            sb.Append(tab);
            sb.Append(line);
        }

        sb.AppendLine();
    }
}
