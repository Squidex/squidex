// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData;
using Microsoft.OData.UriParser;
using Squidex.Infrastructure.Validation;

namespace Squidex.Infrastructure.Queries.OData
{
    public static class FilterBuilder
    {
        public static void ParseFilter(this ODataUriParser query, ClrQuery result)
        {
            SearchClause searchClause;
            try
            {
                searchClause = query.ParseSearch();
            }
            catch (ODataException ex)
            {
                var error = $"OData $search clause not valid: {ex.Message}";

                throw new ValidationException(error, ex);
            }

            if (searchClause != null)
            {
                result.FullText = SearchTermVisitor.Visit(searchClause.Expression).ToString();
            }

            FilterClause filterClause;
            try
            {
                filterClause = query.ParseFilter();
            }
            catch (ODataException ex)
            {
                var error = $"OData $filter clause not valid: {ex.Message}";

                throw new ValidationException(error, ex);
            }

            if (filterClause != null)
            {
                var filter = FilterVisitor.Visit(filterClause.Expression);

                result.Filter = Optimizer<ClrValue>.Optimize(filter);
            }
        }
    }
}
