// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure;

namespace Squidex.Web.Pipeline
{
    public sealed class EnforceHttpsMiddleware : IMiddleware
    {
        private readonly UrlsOptions urlsOptions;

        public EnforceHttpsMiddleware(IOptions<UrlsOptions> urlsOptions)
        {
            Guard.NotNull(urlsOptions, nameof(urlsOptions));

            this.urlsOptions = urlsOptions.Value;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!urlsOptions.EnforceHTTPS)
            {
                await next(context);
            }
            else
            {
                var hostName = context.Request.Host.ToString().ToLowerInvariant();

                if (!context.Request.IsHttps)
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
