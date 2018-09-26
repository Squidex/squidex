// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;

namespace Squidex.Infrastructure
{
    public abstract class CachingProviderBase
    {
        private readonly IMemoryCache cache;

        protected IMemoryCache Cache
        {
            get { return cache; }
        }

        protected CachingProviderBase(IMemoryCache cache)
        {
            Guard.NotNull(cache, nameof(cache));

            this.cache = cache;
        }
    }
}
