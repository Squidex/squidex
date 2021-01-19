// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.IO;
using System.Text;
using GeoJSON.Net;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.ObjectPool;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public static class Extensions
    {
        private static readonly ObjectPool<StringBuilder> StringBuilderPool =
            new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

        private static readonly ObjectPool<MemoryStream> MemoryStreamPool =
            new DefaultObjectPool<MemoryStream>(new DefaultPooledObjectPolicy<MemoryStream>());

        public static Dictionary<string, GeoJSONObject>? ToGeo(this ContentData data, IJsonSerializer jsonSerializer)
        {
            Dictionary<string, GeoJSONObject>? result = null;

            foreach (var (field, value) in data)
            {
                if (value != null)
                {
                    foreach (var (key, jsonValue) in value)
                    {
                        var geoJson = GetGeoJson(jsonSerializer, jsonValue);

                        if (geoJson != null)
                        {
                            result ??= new Dictionary<string, GeoJSONObject>();
                            result[$"{field}.{key}"] = geoJson;
                        }
                    }
                }
            }

            return result;
        }

        private static GeoJSONObject? GetGeoJson(IJsonSerializer jsonSerializer, IJsonValue value)
        {
            if (value is JsonObject geoObject)
            {
                var stream = MemoryStreamPool.Get();

                try
                {
                    stream.Position = 0;

                    jsonSerializer.Serialize(geoObject, stream, true);

                    stream.Position = 0;

                    return jsonSerializer.Deserialize<GeoJSONObject>(stream, null, true);
                }
                catch
                {
                    if (geoObject.TryGetValue<JsonNumber>("latitude", out var lat) &&
                        geoObject.TryGetValue<JsonNumber>("longitude", out var lon))
                    {
                        return new Point(new Position(lat.Value, lon.Value));
                    }
                }
                finally
                {
                    MemoryStreamPool.Return(stream);
                }
            }

            return null;
        }

        public static Dictionary<string, string>? ToTexts(this ContentData data)
        {
            Dictionary<string, string>? result = null;

            if (data != null)
            {
                var languages = new Dictionary<string, StringBuilder>();
                try
                {
                    foreach (var value in data.Values)
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
                        StringBuilderPool.Return(sb);
                    }
                }
            }

            return result;
        }

        private static void AppendJsonText(Dictionary<string, StringBuilder> languages, string language, IJsonValue value)
        {
            if (value.Type == JsonValueType.String)
            {
                AppendText(languages, language, value.ToString());
            }
            else if (value is JsonArray array)
            {
                foreach (var item in array)
                {
                    AppendJsonText(languages, language, item);
                }
            }
            else if (value is JsonObject obj)
            {
                foreach (var item in obj.Values)
                {
                    AppendJsonText(languages, language, item);
                }
            }
        }

        private static void AppendText(Dictionary<string, StringBuilder> languages, string language, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (!languages.TryGetValue(language, out var sb))
                {
                    sb = StringBuilderPool.Get();

                    languages[language] = sb;
                }

                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }

                sb.Append(text);
            }
        }
    }
}
