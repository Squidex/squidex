// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Squidex.Areas.Api.Controllers.UI;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Web;

namespace Squidex.Areas.Frontend.Middlewares
{
    public static class IndexExtensions
    {
        private static readonly ConcurrentDictionary<string, string> Texts = new ConcurrentDictionary<string, string>();
        private static readonly JsonSerializer JsonSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });

        public static string AddOptions(this string html, HttpContext httpContext)
        {
            const string Placeholder = "/* INJECT OPTIONS */";

            if (!html.Contains(Placeholder, StringComparison.Ordinal))
            {
                return html;
            }

            var scripts = new List<string>
            {
                $"var texts = {GetText(CultureInfo.CurrentUICulture.Name)};"
            };

            var uiOptions = httpContext.RequestServices.GetService<IOptions<MyUIOptions>>()?.Value;

            if (uiOptions != null)
            {
                var json = JObject.FromObject(uiOptions, JsonSerializer);

                var values = httpContext.RequestServices.GetService<ExposedValues>();

                if (values != null)
                {
                    json["more"] ??= new JObject();
                    json["more"]!["info"] = values.ToString();
                }

                var notifo = httpContext.RequestServices!.GetService<IOptions<NotifoOptions>>();

                if (notifo?.Value.IsConfigured() == true)
                {
                    json["more"] ??= new JObject();
                    json["more"]!["notifoApi"] = notifo.Value.ApiUrl;
                }

                var options = httpContext.Features.Get<OptionsFeature>();

                if (options != null)
                {
                    foreach (var (key, value) in options.Options)
                    {
                        json[key] = JToken.FromObject(value);
                    }
                }

                json["more"] ??= new JObject();
                json["more"]!["culture"] = CultureInfo.CurrentUICulture.Name;

                scripts.Add($"var options = {json.ToString(Formatting.Indented)};");
            }

            html = html.Replace(Placeholder, string.Join(Environment.NewLine, scripts), StringComparison.OrdinalIgnoreCase);

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
