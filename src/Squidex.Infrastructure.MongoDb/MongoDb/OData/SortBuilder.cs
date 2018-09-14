// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using MongoDB.Driver;
using Squidex.Infrastructure.Queries;

namespace Squidex.Infrastructure.MongoDb.OData
{
    public static class SortBuilder
    {
        public static SortDefinition<T> BuildSort<T>(this Query query)
        {
            if (query.Sort.Count > 0)
            {
                var sorts = new List<SortDefinition<T>>();

                foreach (var sort in query.Sort)
                {
                    sorts.Add(OrderBy<T>(sort));
                }

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

            if (sort.SortOrder == SortOrder.Ascending)
            {
                return Builders<T>.Sort.Ascending(propertyName);
            }
            else
            {
                return Builders<T>.Sort.Descending(propertyName);
            }
        }
    }
}
