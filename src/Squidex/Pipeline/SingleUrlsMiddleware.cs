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

namespace Squidex.Pipeline
{
    public sealed class SingleUrlsMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<SingleUrlsMiddleware> logger;

        public SingleUrlsMiddleware(RequestDelegate next, ILoggerFactory factory)
        {
            this.next = next;

            logger = factory.CreateLogger<SingleUrlsMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            var currentUrl = string.Concat(context.Request.Scheme, "://", context.Request.Host, context.Request.Path);

            var hostName = context.Request.Host.ToString().ToLowerInvariant();
            if (hostName.StartsWith("www"))
            {
                hostName = hostName.Substring(3);
            }

            var requestPath = context.Request.Path.ToString();

            if (!requestPath.EndsWith("/") && 
                !requestPath.Contains("."))
            {
                requestPath = requestPath + "/";
            }

            var newUrl = string.Concat("https://", hostName, requestPath);

            if (!string.Equals(newUrl, currentUrl, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogError("Invalid url: {0} instead {1}", currentUrl, newUrl);

                context.Response.Redirect(newUrl, true);
            }
            else
            {
                await next(context);
            }
        }
    }
}
