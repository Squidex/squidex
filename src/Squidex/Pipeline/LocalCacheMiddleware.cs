// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;

namespace Squidex.Pipeline
{
    public sealed class LocalCacheMiddleware : IMiddleware
    {
        private readonly ILocalCache localCache;

        public LocalCacheMiddleware(ILocalCache localCache)
        {
            Guard.NotNull(localCache, nameof(localCache));

            this.localCache = localCache;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            using (localCache.StartContext())
            {
                await next(context);
            }
        }
    }
}
