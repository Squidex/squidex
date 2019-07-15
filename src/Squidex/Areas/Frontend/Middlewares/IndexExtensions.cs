// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Areas.Api.Controllers.UI;
using Squidex.Infrastructure.Json;
using Squidex.Web;

namespace Squidex.Areas.Frontend.Middlewares
{
    public static class IndexExtensions
    {
        public static bool IsIndex(this HttpContext context)
        {
            return context.Request.Path.Value.EndsWith("/index.html", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsHtmlPath(this HttpContext context)
        {
            return context.Request.Path.Value.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsHtml(this HttpContext context)
        {
            return context.Response.ContentType?.ToLower().Contains("text/html") == true;
        }

        public static string AdjustHtml(this string html, HttpContext httpContext)
        {
            var result = html;

            if (httpContext.Request.PathBase.HasValue)
            {
                result = result.Replace("<base href=\"/\">", $"<base href=\"{httpContext.Request.PathBase}/\">");
            }

            var uiOptions = httpContext.RequestServices.GetService<IOptions<MyUIOptions>>()?.Value;

            if (uiOptions != null)
            {
                var values = httpContext.RequestServices.GetService<ExposedValues>();

                if (values != null)
                {
                    uiOptions.More["info"] = values.ToString();
                }

                var jsonSerializer = httpContext.RequestServices.GetRequiredService<IJsonSerializer>();
                var jsonOptions = jsonSerializer.Serialize(uiOptions, false);

                result = result.Replace("<body>", $"<body><script>var options = {jsonOptions};</script>");
            }

            return result;
        }
    }
}
