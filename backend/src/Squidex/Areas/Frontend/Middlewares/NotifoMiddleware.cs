// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Squidex.Domain.Apps.Entities.History;

namespace Squidex.Areas.Frontend.Middlewares
{
    public class NotifoMiddleware
    {
        private readonly RequestDelegate next;
        private readonly NotifoOptions options;

        public NotifoMiddleware(RequestDelegate next, IOptions<NotifoOptions> options)
        {
            this.next = next;

            this.options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Equals("/notifo-sw.js") && options.IsConfigured())
            {
                context.Response.Headers[HeaderNames.ContentType] = "text/javascript";

                var url = options.ApiUrl;

                if (options.ApiUrl.Contains("localhost:5002"))
                {
                    url = "https://localhost:3002";
                }

                var script = $"importScripts('{url}/notifo-sdk-worker.js')";

                await context.Response.WriteAsync(script);
            }
            else
            {
                await next(context);
            }
        }
    }
}
