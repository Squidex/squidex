// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Caching
{
    public static class LocalCacheExtensions
    {
        public static async Task<T> GetOrCreateAsync<T>(this ILocalCache cache, object key, Func<Task<T>> task)
        {
            if (cache.TryGetValue(key, out var value))
            {
                if (value is T typed)
                {
                    return typed;
                }
                else
                {
                    return default!;
                }
            }

            var result = await task();

            cache.Add(key, result);

            return result;
        }
    }
}
