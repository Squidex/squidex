// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData;
using Microsoft.OData.UriParser;
using MongoDB.Driver;

namespace Squidex.Infrastructure.MongoDb.OData
{
    public static class FilterBuilder
    {
        public static (FilterDefinition<T> Filter, bool Last) BuildFilter<T>(this ODataUriParser query, ConvertProperty convertProperty = null, ConvertValue convertValue = null, bool supportsSearch = true)
        {
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
                if (!supportsSearch)
                {
                    throw new ValidationException("Query $search clause not supported.");
                }

                return (Builders<T>.Filter.Text(SearchTermVisitor.Visit(search.Expression).ToString()), false);
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
                return (FilterVisitor<T>.Visit(filter.Expression, convertProperty, convertValue), true);
            }

            return (null, false);
        }
    }
}
