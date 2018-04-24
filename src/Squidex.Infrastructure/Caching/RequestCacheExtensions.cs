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
    public static class RequestCacheExtensions
    {
        public static async Task<T> GetOrCreateAsync<T>(this IRequestCache cache, object key, Func<Task<T>> task)
        {
            if (cache.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            typedValue = await task();

            cache.Add(key, typedValue);

            return typedValue;
        }

        public static T GetOrCreate<T>(this IRequestCache cache, object key, Func<T> task)
        {
            if (cache.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            typedValue = task();

            cache.Add(key, typedValue);

            return typedValue;
        }
    }
}
