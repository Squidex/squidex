// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace Squidex.Pipeline
{
    public sealed class ETagFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            var httpContext = context.HttpContext;

            if (HttpMethods.IsGet(httpContext.Request.Method) &&
                httpContext.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var noneMatch) &&
                httpContext.Response.StatusCode == 200 &&
                httpContext.Response.Headers.TryGetValue(HeaderNames.ETag, out var etag) &&
                !string.IsNullOrWhiteSpace(noneMatch) &&
                !string.IsNullOrWhiteSpace(etag) &&
                string.Equals(etag, noneMatch, System.StringComparison.Ordinal))
            {
                resultContext.Result = new StatusCodeResult(304);
            }
        }
    }
}
