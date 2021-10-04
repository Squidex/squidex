// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Areas.Api.Controllers.UI;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Infrastructure.Json;
using Squidex.Web;

namespace Squidex.Areas.Frontend.Middlewares
{
    public static class IndexExtensions
    {
        private static readonly ConcurrentDictionary<string, string> Texts = new ConcurrentDictionary<string, string>();

        public static bool IsIndex(this HttpContext context)
        {
            var path = context.Request.Path.Value;

            return path == "/" || path?.EndsWith("/index.html", StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool IsHtmlPath(this HttpContext context)
        {
            return context.Request.Path.Value?.EndsWith(".html", StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool IsNotModified(this HttpResponse response)
        {
            return response.StatusCode == (int)HttpStatusCode.NotModified;
        }

        public static string AdjustBase(this string html, HttpContext httpContext)
        {
            if (httpContext.Request.PathBase != null)
            {
                html = html.Replace("<base href=\"/\">", $"<base href=\"{httpContext.Request.PathBase}/\">", StringComparison.OrdinalIgnoreCase);
            }

            return html;
        }

        public static string AddOptions(this string html, HttpContext httpContext)
        {
            var uiOptions = httpContext.RequestServices.GetService<IOptions<MyUIOptions>>()?.Value;

            if (uiOptions != null)
            {
                var values = httpContext.RequestServices.GetService<ExposedValues>();

                if (values != null)
                {
                    uiOptions.More["info"] = values.ToString();
                }

                var notifo = httpContext.RequestServices!.GetRequiredService<IOptions<NotifoOptions>>();

                if (notifo.Value.IsConfigured())
                {
                    uiOptions.More["notifoApi"] = notifo.Value.ApiUrl;
                }

                uiOptions.More["culture"] = CultureInfo.CurrentUICulture.Name;

                var jsonSerializer = httpContext.RequestServices.GetRequiredService<IJsonSerializer>();
                var jsonOptions = jsonSerializer.Serialize(uiOptions, true);

                var texts = GetText(CultureInfo.CurrentUICulture.Name);

                html = html.Replace("<body>", $"<body>\n<script>\nvar options = {jsonOptions};\nvar texts = {texts};</script>", StringComparison.OrdinalIgnoreCase);
            }

            return html;
        }

        private static string GetText(string culture)
        {
            if (!Texts.TryGetValue(culture, out var result))
            {
                var assembly = typeof(IndexExtensions).Assembly;

                var resourceName = $"Squidex.Areas.Frontend.Resources.frontend_{culture}.json";
                var resourceStream = assembly.GetManifestResourceStream(resourceName);

                if (resourceStream != null)
                {
                    using (var reader = new StreamReader(resourceStream))
                    {
                        result = reader.ReadToEnd();

                        Texts[culture] = result;
                    }
                }
                else
                {
                    return GetText("en");
                }
            }

            return result;
        }
    }
}
