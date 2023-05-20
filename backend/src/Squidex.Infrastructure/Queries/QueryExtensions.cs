// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries;

public static class QueryExtensions
{
    public static bool HasField<T>(this FilterNode<T>? filter, string field)
    {
        if (filter == null)
        {
            return false;
        }

        var fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        filter.AddFields(fields);

        return fields.Contains(field);
    }
}
