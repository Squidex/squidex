// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.DataLoader;
using Squidex.Infrastructure.Caching;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Cache;

internal static class CachingDataLoaderExtensions
{
    public static IDataLoader<CacheableId<TKey>, T> GetOrAddCachingLoader<TKey, T>(this DataLoaderContext dataLoaderContext, IQueryCache<TKey, T> queryCache, string loaderKey,
        Func<IEnumerable<TKey>, CancellationToken, Task<IDictionary<TKey, T>>> queryDelegate, bool canCache = true, int maxBatchSize = int.MaxValue)
        where TKey : notnull where T : class
    {
        return dataLoaderContext.GetOrAdd(loaderKey, () =>
        {
            return new CachingBatchDataLoader<TKey, T>(queryCache, queryDelegate, canCache, maxBatchSize);
        });
    }

    public static IDataLoader<TKey, T> GetOrAddNonCachingBatchLoader<TKey, T>(this DataLoaderContext dataLoaderContext, string loaderKey,
        Func<IEnumerable<TKey>, CancellationToken, Task<IDictionary<TKey, T>>> queryDelegate, int maxBatchSize = int.MaxValue)
        where TKey : notnull where T : class
    {
        return dataLoaderContext.GetOrAdd(loaderKey, () =>
        {
            return new NonCachingBatchLoader<TKey, T>(queryDelegate, maxBatchSize);
        });
    }
}
