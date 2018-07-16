// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.OData.UriParser;
using MongoDB.Driver;

namespace Squidex.Infrastructure.MongoDb.OData
{
    public static class SortBuilder
    {
        public static SortDefinition<T> BuildSort<T>(this ODataUriParser query, ConvertProperty propertyCalculator = null)
        {
            var orderBy = query.ParseOrderBy();

            if (orderBy != null)
            {
                var sorts = new List<SortDefinition<T>>();

                while (orderBy != null)
                {
                    sorts.Add(OrderBy<T>(orderBy, propertyCalculator));

                    orderBy = orderBy.ThenBy;
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

        public static SortDefinition<T> OrderBy<T>(OrderByClause clause, ConvertProperty propertyCalculator = null)
        {
            var propertyName = clause.Expression.BuildFieldDefinition(propertyCalculator);

            if (clause.Direction == OrderByDirection.Ascending)
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
