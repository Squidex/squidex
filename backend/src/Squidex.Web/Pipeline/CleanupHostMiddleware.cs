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

namespace Squidex.Web.Pipeline
{
    public class CleanupHostMiddleware
    {
        private readonly RequestDelegate next;
        private readonly HostString host;

        public CleanupHostMiddleware(RequestDelegate next, IOptions<UrlsOptions> options)
        {
            this.next = next;

            host = new HostString(new Uri(options.Value.BaseUrl).Host);
        }

        public Task InvokeAsync(HttpContext context)
        {
            context.Request.Host = host;

            return next(context);
        }
    }
}
