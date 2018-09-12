// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace Squidex.Infrastructure.Queries.OData
{
    public static class FilterBuilder
    {
        public static void ParseFilter(this ODataUriParser query, Query result)
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
                result.FullText = SearchTermVisitor.Visit(search.Expression).ToString();
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
                result.Filter = FilterVisitor.Visit(filter.Expression);
            }
        }
    }
}
