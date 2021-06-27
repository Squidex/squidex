// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Caching;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class LocalCacheFilter : IIncomingGrainCallFilter
    {
        private readonly ILocalCache localCache;

        public LocalCacheFilter(ILocalCache localCache)
        {
            this.localCache = localCache;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            if (!context.Grain.GetType().Namespace!.StartsWith("Orleans", StringComparison.OrdinalIgnoreCase))
            {
                using (localCache.StartContext())
                {
                    await context.Invoke();
                }
            }
            else
            {
                await context.Invoke();
            }
        }
    }
}
