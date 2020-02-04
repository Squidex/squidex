// ==========================================================================
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
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Squidex.Web.Pipeline
{
    public sealed class CachingFilter : IAsyncActionFilter
    {
        private readonly CachingOptions cachingOptions;
        private readonly CachingManager cachingManager;

        public CachingFilter(CachingManager cachingManager, IOptions<CachingOptions> cachingOptions)
        {
            this.cachingOptions = cachingOptions.Value;
            this.cachingManager = cachingManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;

            cachingManager.Start(httpContext);

            var resultContext = await next();

            cachingManager.Finish(httpContext, cachingOptions.MaxSurrogateKeys);

            if (httpContext.Response.Headers.TryGetString(HeaderNames.ETag, out var etag))
            {
                if (!cachingOptions.StrongETag && !etag.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
                {
                    etag = $"W/{etag}";

                    httpContext.Response.Headers[HeaderNames.ETag] = etag;
                }

                if (HttpMethods.IsGet(httpContext.Request.Method) &&
                    httpContext.Response.StatusCode == 200 &&
                    httpContext.Request.Headers.TryGetString(HeaderNames.IfNoneMatch, out var noneMatch) &&
                    string.Equals(etag, noneMatch, StringComparison.Ordinal))
                {
                    resultContext.Result = new StatusCodeResult(304);
                }
            }
        }
    }
}
