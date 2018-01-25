// ==========================================================================
//  SortBuilder.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using System.Collections.Generic;
using Microsoft.OData.UriParser;
using MongoDB.Driver;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors
{
    public static class SortBuilder
    {
        private static readonly SortDefinitionBuilder<MongoAssetEntity> Sort = Builders<MongoAssetEntity>.Sort;

        public static SortDefinition<MongoAssetEntity> BuildSort(ODataUriParser query)
        {
            var orderBy = query.ParseOrderBy();

            if (orderBy != null)
            {
                var sorts = new List<SortDefinition<MongoAssetEntity>>();

                while (orderBy != null)
                {
                    sorts.Add(OrderBy(orderBy));

                    orderBy = orderBy.ThenBy;
                }

                if (sorts.Count > 1)
                {
                    return Sort.Combine(sorts);
                }
                else
                {
                    return sorts[0];
                }
            }
            else
            {
                return Sort.Descending(x => x.LastModified);
            }
        }

        public static SortDefinition<MongoAssetEntity> OrderBy(OrderByClause clause)
        {
            if (clause.Direction == OrderByDirection.Ascending)
            {
                return Sort.Ascending(PropertyVisitor.Visit(clause.Expression));
            }
            else
            {
                return Sort.Descending(PropertyVisitor.Visit(clause.Expression));
            }
        }
    }
}
