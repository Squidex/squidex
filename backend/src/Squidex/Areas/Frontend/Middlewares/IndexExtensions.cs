// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Squidex.Areas.Api.Controllers.UI;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Web;

namespace Squidex.Areas.Frontend.Middlewares;

public static class IndexExtensions
{
    private static readonly ConcurrentDictionary<string, string> Texts = new ConcurrentDictionary<string, string>();

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
            var clonedOptions = uiOptions with
            {
                More = new Dictionary<string, object>
                {
                    ["culture"] = CultureInfo.CurrentUICulture.Name
                }
            };

            var jsonOptions = httpContext.RequestServices.GetRequiredService<JsonSerializerOptions>();

            using var jsonDocument = JsonSerializer.SerializeToDocument(uiOptions, jsonOptions);

            if (httpContext.RequestServices.GetService<ExposedValues>() is ExposedValues values)
            {
                clonedOptions.More["info"] = values.ToString();
            }

            var notifo = httpContext.RequestServices!.GetService<IOptions<NotifoOptions>>()?.Value;

            if (notifo?.IsConfigured() == true)
            {
                clonedOptions.More["notifoApi"] = notifo.ApiUrl;
            }

            var options = httpContext.Features.Get<OptionsFeature>();

            if (options != null)
            {
                foreach (var (key, value) in options.Options)
                {
                    clonedOptions.More[key] = value;
                }
            }

            scripts.Add($"var options = {JsonSerializer.Serialize(clonedOptions)};");
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
