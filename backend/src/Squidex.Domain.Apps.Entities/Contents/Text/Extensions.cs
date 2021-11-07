// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Text;
using GeoJSON.Net;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public static class Extensions
    {
        public static string PrefixField(this string query, string prefix, bool underscore = false)
        {
            var indexOfColon = query.IndexOf(':', System.StringComparison.Ordinal);

            if (indexOfColon < 0)
            {
                return query;
            }

            var sb = new StringBuilder();

            int position = 0, lastIndexOfColon = 0;

            while (indexOfColon >= 0)
            {
                lastIndexOfColon = indexOfColon;

                var i = 0;

                for (i = indexOfColon - 1; i >= position; i--)
                {
                    var c = query[i];

                    if (!char.IsLetter(c) && c != '-' && c != '_')
                    {
                        break;
                    }
                }

                i++;

                sb.Append(query[position..i]);
                sb.Append(prefix);

                if (underscore)
                {
                    sb.Append(query[i..indexOfColon].Replace('-', '_'));
                }
                else
                {
                    sb.Append(query[i..indexOfColon]);
                }

                position = indexOfColon + 1;

                indexOfColon = query.IndexOf(':', position);
            }

            sb.Append(query[lastIndexOfColon..]);

            return sb.ToString();
        }

        public static Dictionary<string, GeoJSONObject>? ToGeo(this ContentData data, IJsonSerializer jsonSerializer)
        {
            Dictionary<string, GeoJSONObject>? result = null;

            foreach (var (field, value) in data)
            {
                if (value != null)
                {
                    foreach (var (key, jsonValue) in value)
                    {
                        GeoJsonValue.TryParse(jsonValue, jsonSerializer, out var geoJson);

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
                        DefaultPools.StringBuilder.Return(sb);
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
                    sb = DefaultPools.StringBuilder.Get();

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
