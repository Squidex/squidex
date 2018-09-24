// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Squidex.Pipeline
{
    public sealed class ETagFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            var httpContext = context.HttpContext;

            if (!httpContext.Response.Headers.TryGetValue("Etag", out _) && resultContext.Result is ObjectResult obj && obj.Value is IGenerateEtag g)
            {
                var calculatedEtag = g.GenerateETag();

                if (!string.IsNullOrWhiteSpace(calculatedEtag))
                {
                    httpContext.Response.Headers.Add("Etag", calculatedEtag);
                }
            }

            if (httpContext.Request.Method == "GET" &&
                httpContext.Request.Headers.TryGetValue("If-None-Match", out var noneMatch) &&
                httpContext.Response.StatusCode == 200 &&
                httpContext.Response.Headers.TryGetValue("Etag", out var etag) &&
                !string.IsNullOrWhiteSpace(noneMatch) &&
                !string.IsNullOrWhiteSpace(etag) &&
                string.Equals(etag, noneMatch, System.StringComparison.Ordinal))
            {
                resultContext.Result = new StatusCodeResult(304);
            }
        }
    }
}
