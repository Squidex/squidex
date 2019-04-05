// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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

        public async Task Invoke(HttpContext context)
        {
            var basePath = context.Request.PathBase;

            if (context.IsHtmlPath() && basePath.HasValue)
            {
                var responseBuffer = new MemoryStream();
                var responseBody = context.Response.Body;

                context.Response.Body = responseBuffer;

                await next(context);

                context.Response.Body = responseBody;

                var response = Encoding.UTF8.GetString(responseBuffer.ToArray());

                response = AdjustBase(response, basePath);

                context.Response.ContentLength = Encoding.UTF8.GetByteCount(response);
                context.Response.Body = responseBody;

                await context.Response.WriteAsync(response);
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
