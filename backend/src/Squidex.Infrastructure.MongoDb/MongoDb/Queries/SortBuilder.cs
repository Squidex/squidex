// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Infrastructure.Queries;

namespace Squidex.Infrastructure.MongoDb.Queries;

public static class SortBuilder
{
    public static SortDefinition<T>? BuildSort<T>(this ClrQuery query)
    {
        if (query is { Sort: not null, Sort: { Count: > 0 } })
        {
            var sorts = query.Sort.Select(OrderBy<T>).ToList();

            if (sorts.Count > 1)
            {
                return Builders<T>.Sort.Combine(sorts);
            }
            else
            {
                return sorts[0];
            }
        }

        return null;
    }

    public static SortDefinition<T> OrderBy<T>(SortNode sort)
    {
        var propertyName = string.Join(".", sort.Path);

        if (sort.Order == SortOrder.Ascending)
        {
            return Builders<T>.Sort.Ascending(propertyName);
        }
        else
        {
            return Builders<T>.Sort.Descending(propertyName);
        }
    }
}
