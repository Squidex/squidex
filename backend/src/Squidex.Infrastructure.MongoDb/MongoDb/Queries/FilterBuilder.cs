// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Infrastructure.MongoDb.Queries;

public static class FilterBuilder
{
    public static (FilterDefinition<TDocument>? Filter, bool Last) BuildFilter<TDocument>(this ClrQuery query, bool supportsSearch = true)
    {
        if (query.FullText != null)
        {
            if (!supportsSearch)
            {
                throw new ValidationException(T.Get("common.fullTextNotSupported"));
            }

            return (Builders<TDocument>.Filter.Text(query.FullText), false);
        }

        if (query.Filter != null)
        {
            return (query.Filter.BuildFilter<TDocument>(), true);
        }

        return (null, false);
    }

    public static FilterDefinition<T> BuildFilter<T>(this FilterNode<ClrValue> filterNode)
    {
        return FilterVisitor<T>.Visit(filterNode);
    }
}
