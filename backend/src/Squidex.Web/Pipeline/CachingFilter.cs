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
            Guard.NotNull(cachingManager, nameof(cachingManager));
            Guard.NotNull(cachingOptions, nameof(cachingOptions));

            this.cachingOptions = cachingOptions.Value;
            this.cachingManager = cachingManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            cachingManager.Start(context.HttpContext);

            AppendAuthHeaders(context.HttpContext);

            var resultContext = await next();

            if (resultContext.HttpContext.Response.Headers.TryGetString(HeaderNames.ETag, out var etag))
            {
                if (!cachingOptions.StrongETag && IsWeakEtag(etag))
                {
                    etag = ToWeakEtag(etag);

                    resultContext.HttpContext.Response.Headers[HeaderNames.ETag] = etag;
                }

                if (IsCacheable(resultContext.HttpContext, etag))
                {
                    resultContext.Result = new StatusCodeResult(304);
                }
            }

            cachingManager.Finish(resultContext.HttpContext);
        }

        private static bool IsCacheable(HttpContext httpContext, string etag)
        {
            return HttpMethods.IsGet(httpContext.Request.Method) &&
                httpContext.Response.StatusCode == 200 &&
                httpContext.Request.Headers.TryGetString(HeaderNames.IfNoneMatch, out var noneMatch) &&
                string.Equals(etag, noneMatch, StringComparison.Ordinal);
        }

        private void AppendAuthHeaders(HttpContext httpContext)
        {
            cachingManager.AddHeader("Auth-State");

            if (!string.IsNullOrWhiteSpace(httpContext.User.OpenIdSubject()))
            {
                cachingManager.AddHeader(HeaderNames.Authorization);
            }
            else if (!string.IsNullOrWhiteSpace(httpContext.User.OpenIdClientId()))
            {
                cachingManager.AddHeader("Auth-ClientId");
            }
        }

        private static string ToWeakEtag(string? etag)
        {
            return $"W/{etag}";
        }

        private static bool IsWeakEtag(string etag)
        {
            return !etag.StartsWith("W/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
