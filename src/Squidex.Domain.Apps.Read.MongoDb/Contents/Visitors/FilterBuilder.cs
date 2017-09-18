// ==========================================================================
//  FilterBuilder.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.OData;
using Microsoft.OData.UriParser;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read.MongoDb.Contents.Visitors
{
    public static class FilterBuilder
    {
        private static readonly FilterDefinitionBuilder<MongoContentEntity> Filter = Builders<MongoContentEntity>.Filter;

        public static FilterDefinition<MongoContentEntity> Build(ODataUriParser query, Schema schema)
        {
            SearchClause search;
            try
            {
                search = query.ParseSearch();
            }
            catch (ODataException ex)
            {
                throw new ValidationException("Query $search clause not valid", new ValidationError(ex.Message));
            }

            if (search != null)
            {
                return Filter.Text(SearchTermVisitor.Visit(search.Expression).ToString());
            }

            FilterClause filter;
            try
            {
                filter = query.ParseFilter();
            }
            catch (ODataException ex)
            {
                throw new ValidationException("Query $filter clause not valid", new ValidationError(ex.Message));
            }

            if (filter != null)
            {
                return FilterVisitor.Visit(filter.Expression, schema);
            }

            return null;
        }
    }
}
