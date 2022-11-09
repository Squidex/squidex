// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.UriParser;

namespace Squidex.Infrastructure.Queries.OData;

public static class SortBuilder
{
    public static void ParseSort(this ODataUriParser query, ClrQuery result)
    {
        var orderBy = query.ParseOrderBy();

        if (orderBy != null)
        {
            while (orderBy != null)
            {
                result.Sort ??= new List<SortNode>();
                result.Sort.Add(OrderBy(orderBy));

                orderBy = orderBy.ThenBy;
            }
        }
    }

    public static SortNode OrderBy(OrderByClause clause)
    {
        var path = PropertyPathVisitor.Visit(clause.Expression);

        if (clause.Direction == OrderByDirection.Ascending)
        {
            return new SortNode(path, SortOrder.Ascending);
        }
        else
        {
            return new SortNode(path, SortOrder.Descending);
        }
    }
}
