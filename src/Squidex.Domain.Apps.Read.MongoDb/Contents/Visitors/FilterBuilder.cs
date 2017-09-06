// ==========================================================================
//  FilterBuilder.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.OData.UriParser;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Read.MongoDb.Contents.Visitors
{
    public static class FilterBuilder
    {
        private static readonly FilterDefinitionBuilder<MongoContentEntity> Filter = Builders<MongoContentEntity>.Filter;

        public static FilterDefinition<MongoContentEntity> Build(ODataUriParser query, Schema schema)
        {
            var search = query.ParseSearch();

            if (search != null)
            {
                return Filter.Text(SearchTermVisitor.Visit(search.Expression).ToString());
            }

            var filter = query.ParseFilter();

            if (filter != null)
            {
                return FilterVisitor.Visit(filter.Expression, schema);
            }

            return null;
        }
    }
}
