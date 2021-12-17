// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Controllers.Images.Models;
using Squidex.Areas.Api.Controllers.Images.Service;
using Squidex.Infrastructure.Json;

namespace Squidex.Areas.Api.Controllers.Images
{
    public sealed class ImagesMiddleware : IMiddleware
    {
        private readonly InProcessImageResizer inProcessImageResizer;

        public ImagesMiddleware(InProcessImageResizer inProcessImageResizer)
        {
            this.inProcessImageResizer = inProcessImageResizer;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!HttpMethods.IsPost(context.Request.Method))
            {
                context.Response.StatusCode = 404;
                return;
            }

            var request = await ReadRequestAsync(context);

            if (request == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            var result = await inProcessImageResizer.ResizeAsync(request!, context.RequestAborted);

            var response = new ResizeResponse
            {
                ResultPath = result
            };

            await WriteResponseAsync(context, response);
        }

        private static ValueTask<ResizeRequest?> ReadRequestAsync(HttpContext context)
        {
            return context.Request.ReadFromJsonAsync<ResizeRequest>(context.RequestAborted);
        }

        private static Task WriteResponseAsync(HttpContext context, ResizeResponse response)
        {
            return context.Response.WriteAsJsonAsync(response, context.RequestAborted);
        }
    }
}
