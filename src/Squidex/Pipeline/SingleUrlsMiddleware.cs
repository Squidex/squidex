// ==========================================================================
//  SingleUrlsMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Config;

namespace Squidex.Pipeline
{
    public sealed class SingleUrlsMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IOptions<MyUrlsOptions> urls;
        private readonly ILogger<SingleUrlsMiddleware> logger;

        public SingleUrlsMiddleware(RequestDelegate next, ILoggerFactory factory, IOptions<MyUrlsOptions> urls)
        {
            this.next = next;
            this.urls = urls;

            logger = factory.CreateLogger<SingleUrlsMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            if (!urls.Value.EnforceSSL)
            {
                await next(context);
            }
            else
            {
                var currentUrl = string.Concat(context.Request.Scheme, "://", context.Request.Host, context.Request.Path);

                var hostName = context.Request.Host.ToString().ToLowerInvariant();
                if (hostName.StartsWith("www."))
                {
                    hostName = hostName.Substring(4);
                }

                var newUrl = string.Concat("https://", hostName, context.Request.Path);

                if (!string.Equals(newUrl, currentUrl, StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogError("Invalid url: {0} instead {1}", currentUrl, newUrl);

                    context.Response.Redirect(newUrl + context.Request.QueryString, true);
                }
                else
                {
                    await next(context);
                }
            }
        }
    }
}
