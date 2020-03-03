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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;

namespace Squidex.Web.Pipeline
{
    public sealed class CachingFilter : IAsyncActionFilter
    {
        private readonly CachingOptions cachingOptions;
        private readonly CachingManager cachingManager;

        public CachingFilter(CachingManager cachingManager, IOptions<CachingOptions> cachingOptions)
        {
            Guard.NotNull(cachingManager);
            Guard.NotNull(cachingOptions);

            this.cachingOptions = cachingOptions.Value;
            this.cachingManager = cachingManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;

            cachingManager.Start(httpContext);

            cachingManager.AddHeader("Auth-State");

            if (!string.IsNullOrWhiteSpace(httpContext.User.OpenIdSubject()))
            {
                cachingManager.AddHeader(HeaderNames.Authorization);
            }
            else if (!string.IsNullOrWhiteSpace(httpContext.User.OpenIdClientId()))
            {
                cachingManager.AddHeader("Auth-ClientId");
            }

            var resultContext = await next();

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

            cachingManager.Finish(httpContext);
        }
    }
}
