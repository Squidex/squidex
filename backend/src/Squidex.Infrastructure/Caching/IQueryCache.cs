// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Caching;

public interface IQueryCache<TKey, T> where TKey : notnull
{
    void Set(TKey key, T item, TimeSpan cacheDuration);

    bool TryGet(TKey key, out T result);
}
