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
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, error) => true
                };

                using (var client = new HttpClient(handler))
                {
                    var result = await client.GetAsync(WebpackUrl);

                    context.Response.StatusCode = (int)result.StatusCode;

                    if (result.IsSuccessStatusCode)
                    {
                        var html = await result.Content.ReadAsStringAsync();

                        html = html.AdjustBase(context);

                        await context.Response.WriteAsync(html);
                    }
                }
            }
            else
            {
                await next(context);
            }
        }
    }
}
