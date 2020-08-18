﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
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

                var notifo = httpContext.RequestServices.GetService<IOptions<NotifoOptions>>();

                if (notifo.Value.IsConfigured())
                {
                    uiOptions.More["notifoApi"] = notifo.Value.ApiUrl;
                }

                uiOptions.More["culture"] = CultureInfo.CurrentUICulture.Name;

                var jsonSerializer = httpContext.RequestServices.GetRequiredService<IJsonSerializer>();
                var jsonOptions = jsonSerializer.Serialize(uiOptions, true);

                var texts = GetText(CultureInfo.CurrentUICulture.Name);

                result = result.Replace("<body>", $"<body>\n<script>\nvar options = {jsonOptions};\nvar texts = {texts};</script>");
            }

            return result;
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
