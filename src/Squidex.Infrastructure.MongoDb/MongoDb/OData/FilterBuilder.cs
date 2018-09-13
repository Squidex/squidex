// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Infrastructure.Queries;

namespace Squidex.Infrastructure.MongoDb.OData
{
    public static class FilterBuilder
    {
        public static (FilterDefinition<T> Filter, bool Last) BuildFilter<T>(this Query query, bool supportsSearch = true)
        {
            if (query.FullText != null)
            {
                if (!supportsSearch)
                {
                    throw new ValidationException("Query $search clause not supported.");
                }

                return (Builders<T>.Filter.Text(query.FullText), false);
            }

            if (query.Filter != null)
            {
                return (FilterVisitor<T>.Visit(query.Filter), true);
            }

            return (null, false);
        }
    }
}
