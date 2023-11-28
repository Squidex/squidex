// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Squidex.Infrastructure.Security;

namespace Squidex.Web.Pipeline;

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

        context.Response.OnStarting(_ =>
        {
            var httpContext = (HttpContext)_;

            cachingManager.Finish(httpContext);

            if (httpContext.Response.Headers.TryGetValue(HeaderNames.ETag, out var value) && EntityTagHeaderValue.TryParse(value.ToString(), out var etag))
            {
                if (!cachingOptions.StrongETag && !etag.IsWeak)
                {
                    httpContext.Response.Headers[HeaderNames.ETag] = new EntityTagHeaderValue(etag.Tag, true).ToString();
                }
            }

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
}
