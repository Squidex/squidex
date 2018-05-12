// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Squidex.Config;

namespace Squidex.Pipeline
{
    public sealed class EnforceHttpsMiddleware : IMiddleware
    {
        private readonly IOptions<MyUrlsOptions> urls;

        public EnforceHttpsMiddleware(IOptions<MyUrlsOptions> urls)
        {
            this.urls = urls;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!urls.Value.EnforceHTTPS)
            {
                await next(context);
            }
            else
            {
                var hostName = context.Request.Host.ToString().ToLowerInvariant();

                if (!string.Equals(context.Request.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                {
                    var newUrl = string.Concat("https://", hostName, context.Request.Path, context.Request.QueryString);

                    context.Response.Redirect(newUrl, true);
                }
                else
                {
                    await next(context);
                }
            }
        }
    }
}
