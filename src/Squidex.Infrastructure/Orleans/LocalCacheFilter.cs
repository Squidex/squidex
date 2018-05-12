// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure.Caching;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class LocalCacheFilter : IIncomingGrainCallFilter
    {
        private readonly ILocalCache localCache;

        public LocalCacheFilter(ILocalCache localCache)
        {
            Guard.NotNull(localCache, nameof(localCache));

            this.localCache = localCache;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            using (localCache.StartContext())
            {
                await context.Invoke();
            }
        }
    }
}
