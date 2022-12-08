// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Caching;

public interface IQueryCache<TKey, T> where TKey : notnull where T : class, IWithId<TKey>
{
    void SetMany(IEnumerable<(TKey, T?)> results,
        TimeSpan? permanentDuration = null);

    Task<List<T>> CacheOrQueryAsync(IEnumerable<TKey> keys, Func<IEnumerable<TKey>, Task<IEnumerable<T>>> query,
        TimeSpan? permanentDuration = null);
}
