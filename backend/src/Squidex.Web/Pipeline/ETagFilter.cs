// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Squidex.Web.Pipeline
{
    public sealed class ETagFilter : IAsyncActionFilter
    {
        private readonly ETagOptions options;

        public ETagFilter(IOptions<ETagOptions> options)
        {
            this.options = options.Value;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            var httpContext = context.HttpContext;

            if (httpContext.Response.Headers.TryGetHeaderString(HeaderNames.ETag, out var etag))
            {
                if (!options.Strong && !etag.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
                {
                    etag = $"W/{etag}";

                    httpContext.Response.Headers[HeaderNames.ETag] = etag;
                }

                if (HttpMethods.IsGet(httpContext.Request.Method) &&
                    httpContext.Response.StatusCode == 200 &&
                    httpContext.Request.Headers.TryGetHeaderString(HeaderNames.IfNoneMatch, out var noneMatch) &&
                    string.Equals(etag, noneMatch, StringComparison.Ordinal))
                {
                    resultContext.Result = new StatusCodeResult(304);
                }
            }
        }
    }
}
