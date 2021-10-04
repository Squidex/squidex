// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Squidex.Areas.Frontend.Middlewares
{
    public sealed class WebpackMiddleware
    {
        private const string WebpackUrl = "https://localhost:3000/index.html";

        private readonly RequestDelegate next;

        public WebpackMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.IsIndex() && !context.Response.IsNotModified())
            {
                try
                {
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true
                    };

                    using (var client = new HttpClient(handler))
                    {
                        var result = await client.GetAsync(WebpackUrl, context.RequestAborted);

                        context.Response.StatusCode = (int)result.StatusCode;

                        if (result.IsSuccessStatusCode)
                        {
                            var html = await result.Content.ReadAsStringAsync(context.RequestAborted);

                            html = html.AdjustBase(context);

                            await context.Response.WriteAsync(html, context.RequestAborted);
                        }
                    }
                }
                catch
                {
                    context.Request.Path = "/identity-server/webpack";

                    await next(context);
                }
            }
            else
            {
                await next(context);
            }
        }
    }
}
