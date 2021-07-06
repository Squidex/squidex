// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Squidex.Infrastructure.Security;

namespace Squidex.Web.Pipeline
{
    public sealed class CachingKeysMiddleware
    {
        private readonly CachingOptions cachingOptions;
        private readonly CachingManager cachingManager;
        private readonly RequestDelegate next;

        public CachingKeysMiddleware(CachingManager cachingManager, IOptions<CachingOptions> cachingOptions, RequestDelegate next)
        {
            this.cachingOptions = cachingOptions.Value;
            this.cachingManager = cachingManager;

            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            cachingManager.Start(context);

            AppendAuthHeaders(context);

            context.Response.OnStarting(x =>
            {
                var httpContext = (HttpContext)x;

                if (httpContext.Response.Headers.TryGetString(HeaderNames.ETag, out var etag))
                {
                    if (!cachingOptions.StrongETag && IsWeakEtag(etag))
                    {
                        httpContext.Response.Headers[HeaderNames.ETag] = ToWeakEtag(etag);
                    }
                }

                cachingManager.Finish(httpContext);

                return Task.CompletedTask;
            }, context);

            await next(context);
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
