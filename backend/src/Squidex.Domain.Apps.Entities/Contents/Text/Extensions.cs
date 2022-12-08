// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using NetTopologySuite.Geometries;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public static class Extensions
{
    public static Dictionary<string, Geometry>? ToGeo(this ContentData data, IJsonSerializer serializer)
    {
        Dictionary<string, Geometry>? result = null;

        foreach (var (field, value) in data)
        {
            if (value != null)
            {
                foreach (var (key, jsonValue) in value)
                {
                    GeoJsonValue.TryParse(jsonValue, serializer, out var geoJson);

                    if (geoJson != null)
                    {
                        result ??= new Dictionary<string, Geometry>();
                        result[$"{field}.{key}"] = geoJson;
                    }
                }
            }
        }

        return result;
    }

    public static Dictionary<string, string>? ToTexts(this ContentData data)
    {
        Dictionary<string, string>? result = null;

        if (data != null)
        {
            var languages = new Dictionary<string, StringBuilder>();
            try
            {
                foreach (var (_, value) in data)
                {
                    if (value != null)
                    {
                        foreach (var (key, jsonValue) in value)
                        {
                            AppendJsonText(languages, key, jsonValue);
                        }
                    }
                }

                foreach (var (key, sb) in languages)
                {
                    if (sb.Length > 0)
                    {
                        result ??= new Dictionary<string, string>();
                        result[key] = sb.ToString();
                    }
                }
            }
            finally
            {
                foreach (var (_, sb) in languages)
                {
                    DefaultPools.StringBuilder.Return(sb);
                }
            }
        }

        return result;
    }

    private static void AppendJsonText(Dictionary<string, StringBuilder> languages, string language, JsonValue value)
    {
        switch (value.Value)
        {
            case string s:
                AppendText(languages, language, s);
                break;
            case JsonArray a:
                foreach (var item in a)
                {
                    AppendJsonText(languages, language, item);
                }

                break;
            case JsonObject o:
                foreach (var (_, item) in o)
                {
                    AppendJsonText(languages, language, item);
                }

                break;
        }
    }

    private static void AppendText(Dictionary<string, StringBuilder> languages, string language, string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            if (!languages.TryGetValue(language, out var sb))
            {
                sb = DefaultPools.StringBuilder.Get();

                languages[language] = sb;
            }

            sb.AppendIfNotEmpty(' ');
            sb.Append(text);
        }
    }
}
