﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using Squidex.Infrastructure;

namespace Squidex.Web.Pipeline
{
    public sealed class CachingFilter : IAsyncActionFilter
    {
        private readonly CachingManager cachingManager;

        public CachingFilter(CachingManager cachingManager)
        {
            Guard.NotNull(cachingManager, nameof(cachingManager));

            this.cachingManager = cachingManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            cachingManager.Start(context.HttpContext);

            var resultContext = await next();

            if (resultContext.HttpContext.Response.Headers.TryGetString(HeaderNames.ETag, out var etag))
            {
                if (IsCacheable(resultContext.HttpContext, etag))
                {
                    resultContext.Result = new StatusCodeResult(304);
                }
            }
        }

        private static bool IsCacheable(HttpContext httpContext, string etag)
        {
            return HttpMethods.IsGet(httpContext.Request.Method) &&
                httpContext.Response.StatusCode == 200 &&
                httpContext.Request.Headers.TryGetString(HeaderNames.IfNoneMatch, out var noneMatch) &&
                string.Equals(etag, noneMatch, StringComparison.Ordinal);
        }
    }
}
