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

            if (httpContext.Response.Headers.TryGetValue(HeaderNames.ETag, out var etag) && !string.IsNullOrWhiteSpace(etag))
            {
                string etagValue = etag;

                if (!options.Strong)
                {
                    etagValue = "W/" + etag;

                    httpContext.Response.Headers[HeaderNames.ETag] = etagValue;
                }

                if (HttpMethods.IsGet(httpContext.Request.Method) &&
                    httpContext.Response.StatusCode == 200 &&
                    httpContext.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var noneMatch) && !string.IsNullOrWhiteSpace(noneMatch) &&
                    string.Equals(etagValue, noneMatch, System.StringComparison.Ordinal))
                {
                    resultContext.Result = new StatusCodeResult(304);
                }
            }
        }
    }
}
