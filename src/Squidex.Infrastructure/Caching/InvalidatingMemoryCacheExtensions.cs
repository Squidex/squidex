// ==========================================================================
//  InvalidatingMemoryCacheExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;

namespace Squidex.Infrastructure.Caching
{
    public static class InvalidatingMemoryCacheExtensions
    {
        public static void Invalidate(this IMemoryCache cache, object key)
        {
            var invalidatingCache = cache as IInvalidatingCache;

            invalidatingCache?.Invalidate(key);
        }
    }
}
