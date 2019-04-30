// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Squidex.Areas.Frontend.Middlewares
{
    public sealed class WebpackMiddleware
    {
        private const string WebpackUrl = "http://localhost:3000/index.html";
        private readonly RequestDelegate next;

        public WebpackMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.IsHtmlPath())
            {
                if (context.Response.StatusCode != 304)
                {
                    using (var client = new HttpClient())
                    {
                        var result = await client.GetAsync(WebpackUrl);

                        context.Response.StatusCode = (int)result.StatusCode;

                        if (result.IsSuccessStatusCode)
                        {
                            var html = await result.Content.ReadAsStringAsync();

                            var basePath = context.Request.PathBase;

                            if (basePath.HasValue)
                            {
                                html = AdjustBase(html, basePath.Value);
                            }

                            await context.Response.WriteHtmlAsync(html);
                        }
                    }
                }
            }
            else
            {
                await next(context);
            }
        }

        private static string AdjustBase(string response, string baseUrl)
        {
            return response.Replace("<base href=\"/\">", $"<base href=\"{baseUrl}/\">");
        }
    }
}
