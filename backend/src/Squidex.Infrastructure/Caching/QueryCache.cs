// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Squidex.Infrastructure.Caching;

public class QueryCache<TKey, T> : IQueryCache<TKey, T> where TKey : notnull where T : class, IWithId<TKey>
{
    private readonly ConcurrentDictionary<TKey, T?> entries = new ConcurrentDictionary<TKey, T?>();
    private readonly IMemoryCache? memoryCache;

    public QueryCache(IMemoryCache? memoryCache = null)
    {
        this.memoryCache = memoryCache;
    }

    public void SetMany(IEnumerable<(TKey, T?)> results,
        TimeSpan? permanentDuration = null)
    {
        Guard.NotNull(results);

        foreach (var (key, value) in results)
        {
            Set(key, value, permanentDuration);
        }
    }

    private void Set(TKey key, T? value,
        TimeSpan? permanentDuration = null)
    {
        entries[key] = value;

        if (memoryCache != null && permanentDuration > TimeSpan.Zero)
        {
            memoryCache.Set(key, value, permanentDuration.Value);
        }
    }

    public async Task<List<T>> CacheOrQueryAsync(IEnumerable<TKey> keys, Func<IEnumerable<TKey>, Task<IEnumerable<T>>> query,
        TimeSpan? permanentDuration = null)
    {
        Guard.NotNull(keys);
        Guard.NotNull(query);

        var items = GetMany(keys, permanentDuration.HasValue);

        var pendingIds = new HashSet<TKey>(keys.Where(key => !items.ContainsKey(key)));

        if (pendingIds.Count > 0)
        {
            var queried = (await query(pendingIds)).ToDictionary(x => x.Id);

            foreach (var id in pendingIds)
            {
                queried.TryGetValue(id, out var item);

                items[id] = item;

                Set(id, item, permanentDuration);
            }
        }

        return items.Values.NotNull().ToList();
    }

    private Dictionary<TKey, T?> GetMany(IEnumerable<TKey> keys,
        bool fromPermanentCache = false)
    {
        var result = new Dictionary<TKey, T?>();

        foreach (var key in keys)
        {
            if (entries.TryGetValue(key, out var value))
            {
                result[key] = value;
            }
            else if (fromPermanentCache && memoryCache != null && memoryCache.TryGetValue(key, out value))
            {
                result[key] = value;

                entries[key] = value;
            }
        }

        return result;
    }
}
