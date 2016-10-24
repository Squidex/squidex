// ==========================================================================
//  CachingProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Squidex.Infrastructure;

namespace Squidex.Read.Utils
{
    public abstract class CachingProvider
    {
        private readonly IMemoryCache cache;

        protected IMemoryCache Cache
        {
            get { return cache; }
        }

        protected CachingProvider(IMemoryCache cache)
        {
            Guard.NotNull(cache, nameof(cache));

            this.cache = cache;
        }
    }
}
