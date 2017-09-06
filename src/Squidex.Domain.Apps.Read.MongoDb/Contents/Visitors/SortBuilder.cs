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
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Read.MongoDb.Contents.Visitors
{
    public static class SortBuilder
    {
        private static readonly SortDefinitionBuilder<MongoContentEntity> Sort = Builders<MongoContentEntity>.Sort;

        public static SortDefinition<MongoContentEntity> BuildSort(ODataUriParser query, Schema schema)
        {
            var orderBy = query.ParseOrderBy();

            if (orderBy != null)
            {
                var sorts = new List<SortDefinition<MongoContentEntity>>();

                while (orderBy != null)
                {
                    sorts.Add(OrderBy(orderBy, schema));

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

        public static SortDefinition<MongoContentEntity> OrderBy(OrderByClause clause, Schema schema)
        {
            if (clause.Direction == OrderByDirection.Ascending)
            {
                return Sort.Ascending(PropertyVisitor.Visit(clause.Expression, schema));
            }
            else
            {
                return Sort.Descending(PropertyVisitor.Visit(clause.Expression, schema));
            }
        }
    }
}
