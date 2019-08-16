// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Net.Http;
using System.Text;
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
            if (context.IsIndex() && context.Response.StatusCode != 304)
            {
                using (var client = new HttpClient())
                {
                    var result = await client.GetAsync(WebpackUrl);

                    context.Response.StatusCode = (int)result.StatusCode;

                    if (result.IsSuccessStatusCode)
                    {
                        var html = await result.Content.ReadAsStringAsync();

                        html = html.AdjustHtml(context);

                        await context.Response.WriteHtmlAsync(html);
                    }
                }
            }
            else if (context.IsHtmlPath() && context.Response.StatusCode != 304)
            {
                var responseBuffer = new MemoryStream();
                var responseBody = context.Response.Body;

                context.Response.Body = responseBuffer;

                await next(context);

                if (context.Response.StatusCode != 304)
                {
                    context.Response.Body = responseBody;

                    var html = Encoding.UTF8.GetString(responseBuffer.ToArray());

                    html = html.AdjustHtml(context);

                    context.Response.ContentLength = Encoding.UTF8.GetByteCount(html);
                    context.Response.Body = responseBody;

                    await context.Response.WriteAsync(html);
                }
            }
            else
            {
                await next(context);
            }
        }
    }
}
