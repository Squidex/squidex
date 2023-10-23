// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.DataLoader;
using Squidex.Infrastructure.Caching;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Cache;

record struct CacheableId<T>(T Id, TimeSpan CacheDuration = default);

internal class CachingBatchDataLoader<TKey, T> : DataLoaderBase<CacheableId<TKey>, T> where TKey : notnull where T : class
{
    private readonly IQueryCache<TKey, T> queryCache;
    private readonly Func<IEnumerable<TKey>, CancellationToken, Task<IDictionary<TKey, T>>> queryDelegate;

    public CachingBatchDataLoader(IQueryCache<TKey, T> queryCache,
        Func<IEnumerable<TKey>, CancellationToken, Task<IDictionary<TKey, T>>> queryDelegate, bool canCache = true, int maxBatchSize = int.MaxValue)
        : base(canCache, maxBatchSize)
    {
        this.queryCache = queryCache;
        this.queryDelegate = queryDelegate;
    }

    protected override async Task FetchAsync(IEnumerable<DataLoaderPair<CacheableId<TKey>, T>> list,
        CancellationToken cancellationToken)
    {
        var unmatched = new List<DataLoaderPair<CacheableId<TKey>, T>>(list.Count());

        foreach (var entry in list)
        {
            if (entry.Key.CacheDuration != default && queryCache.TryGet(entry.Key.Id, out var cached))
            {
                entry.SetResult(cached);
            }
            else
            {
                unmatched.Add(entry);
            }
        }

        if (unmatched.Count == 0)
        {
            return;
        }

        var ids = unmatched.Select(x => x.Key.Id).Distinct();

        var entries = await queryDelegate(ids, cancellationToken);

        foreach (var entry in unmatched)
        {
            entries.TryGetValue(entry.Key.Id, out var value);
            entry.SetResult(value!);

            if (value != null && entry.Key.CacheDuration != default)
            {
                queryCache.Set(entry.Key.Id, value, entry.Key.CacheDuration);
            }
        }
    }
}
