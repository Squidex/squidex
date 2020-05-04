// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Squidex.Web;

namespace Squidex.Areas.Api.Config
{
    public sealed class IdentityServerPathMiddleware
    {
        private readonly UrlsOptions urlsOptions;
        private readonly RequestDelegate next;

        public IdentityServerPathMiddleware(IOptions<UrlsOptions> urlsOptions, RequestDelegate next)
        {
            this.urlsOptions = urlsOptions.Value;

            this.next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            context.SetIdentityServerOrigin(urlsOptions.BaseUrl);
            context.SetIdentityServerBasePath(Constants.IdentityServerPrefix);

            return next(context);
        }
    }
}
