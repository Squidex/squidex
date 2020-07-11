// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Caching
{
    public static class LocalCacheExtensions
    {
        public static T GetOrCreate<T>(this ILocalCache cache, object key, Func<T> task)
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

            var result = task();

            cache.Add(key, result);

            return result;
        }
    }
}
