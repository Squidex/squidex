// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;

namespace Squidex.Domain.Apps.Core.ValidateContent;

public static class ObjectPath
{
    public static string ToPathString(this IEnumerable<string> path)
    {
        var sb = new StringBuilder();

        var index = 0;

        foreach (var property in path)
        {
            if (index == 0)
            {
                sb.Append(property);
            }
            else
            {
                if (property[0] != '[')
                {
                    sb.Append('.');
                }

                sb.Append(property);
            }

            index++;
        }

        return sb.ToString();
    }
}
