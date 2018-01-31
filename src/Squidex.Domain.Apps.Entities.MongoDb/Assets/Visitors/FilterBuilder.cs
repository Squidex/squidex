// ==========================================================================
//  FilterBuilder.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using System.Collections.Generic;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using MongoDB.Driver;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors
{
    public static class FilterBuilder
    {
        private static readonly FilterDefinitionBuilder<MongoAssetEntity> Filter = Builders<MongoAssetEntity>.Filter;

        public static List<FilterDefinition<MongoAssetEntity>> Build(ODataUriParser query)
        {
            List<FilterDefinition<MongoAssetEntity>> filters = new List<FilterDefinition<MongoAssetEntity>>();

            SearchClause search;
            try
            {
                search = query.ParseSearch();
            }
            catch (ODataException ex)
            {
                throw new ValidationException("Query $search clause not valid.", new ValidationError(ex.Message));
            }

            if (search != null)
            {
                filters.Add(Filter.Text(SearchTermVisitor.Visit(search.Expression).ToString()));
            }

            FilterClause filter;
            try
            {
                filter = query.ParseFilter();
            }
            catch (ODataException ex)
            {
                throw new ValidationException("Query $filter clause not valid.", new ValidationError(ex.Message));
            }

            if (filter != null)
            {
                filters.Add(FilterVisitor.Visit(filter.Expression));
            }

            return filters;
        }
    }
}
