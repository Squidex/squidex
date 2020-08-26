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

            var uri = new Uri(options.Value.BaseUrl);

            if (HasHttpPort(uri) || HasHttpsPort(uri))
            {
                host = new HostString(uri.Host);
            }
            else
            {
                host = new HostString(uri.Host, uri.Port);
            }
        }

        public Task InvokeAsync(HttpContext context)
        {
            context.Request.Host = host;

            return next(context);
        }

        private static bool HasHttpPort(Uri uri)
        {
            return uri.Scheme == "http" && uri.Port == 80;
        }

        private static bool HasHttpsPort(Uri uri)
        {
            return uri.Scheme == "https" && uri.Port == 443;
        }
    }
}
