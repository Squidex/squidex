// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Squidex.Areas.Frontend.Middlewares
{
    public sealed class IndexMiddleware
    {
        private readonly RequestDelegate next;

        public IndexMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.IsHtmlPath() && !context.Response.IsNotModified())
            {
                var responseBuffer = new MemoryStream();
                var responseBody = context.Response.Body;

                context.Response.Body = responseBuffer;

                await next(context);

                if (!context.Response.IsNotModified())
                {
                    context.Response.Body = responseBody;

                    var html = Encoding.UTF8.GetString(responseBuffer.ToArray());

                    html = html.AdjustBase(context);

                    if (context.IsIndex())
                    {
                        html = html.AddOptions(context);
                    }

                    context.Response.ContentLength = Encoding.UTF8.GetByteCount(html);
                    context.Response.Body = responseBody;

                    await context.Response.WriteAsync(html, context.RequestAborted);
                }
            }
            else
            {
                await next(context);
            }
        }
    }
}
