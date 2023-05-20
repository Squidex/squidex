// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Validation;

namespace Squidex.Infrastructure.MongoDb.Queries;

public static class FilterBuilder
{
    public static (FilterDefinition<T>? Filter, bool Last) BuildFilter<T>(this ClrQuery query, bool supportsSearch = true)
    {
        if (query.FullText != null)
        {
            if (!supportsSearch)
            {
                throw new ValidationException(Translations.T.Get("common.fullTextNotSupported"));
            }

            return (Builders<T>.Filter.Text(query.FullText), false);
        }

        if (query.Filter != null)
        {
            return (query.Filter.BuildFilter<T>(), true);
        }

        return (null, false);
    }

    public static FilterDefinition<T> BuildFilter<T>(this FilterNode<ClrValue> filterNode)
    {
        return FilterVisitor<T>.Visit(filterNode);
    }
}
