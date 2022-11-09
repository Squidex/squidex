// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Caching;

namespace Squidex.Web.Pipeline;

public sealed class LocalCacheMiddleware
{
    private readonly RequestDelegate next;

    public LocalCacheMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILocalCache localCache)
    {
        using (localCache.StartContext())
        {
            await next(context);
        }
    }
}
