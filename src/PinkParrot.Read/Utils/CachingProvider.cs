// ==========================================================================
//  CachingProvider.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using PinkParrot.Infrastructure;

namespace PinkParrot.Read.Utils
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
