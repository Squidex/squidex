// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Infrastructure.Queries;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.States;

public sealed class IndexDefinition : List<IndexField>
{
    public string ToName()
    {
        var sb = new StringBuilder();

        foreach (var field in this)
        {
            if (sb.Length > 0)
            {
                sb.Append('_');
            }

            sb.Append(field.Name);
            sb.Append('_');

            if (field.Order == SortOrder.Ascending)
            {
                sb.Append("asc");
            }
            else
            {
                sb.Append("desc");
            }
        }

        return sb.ToString();
    }
}

public sealed record IndexField(string Name, SortOrder Order);
