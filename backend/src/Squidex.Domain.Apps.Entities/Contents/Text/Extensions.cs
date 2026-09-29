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
                        result ??= [];
                        result[$"{field}.{key}"] = geoJson;
                    }
                }
            }
        }

        return result;
    }

    public static List<UserInfoValue>? ToUserInfos(this ContentData data)
    {
        List<UserInfoValue>? result = null;

        foreach (var (field, value) in data)
        {
            if (value != null)
            {
                foreach (var (key, jsonValue) in value)
                {
                    UserInfoValue.TryParse(jsonValue, out var userInfo);

                    if (userInfo != null)
                    {
                        result ??= [];
                        result.Add(userInfo);
                    }
                }
            }
        }

        return result;
    }

    public static Dictionary<string, string>? GetWeightedTexts(this UpsertIndexEntry upsert, int titleWeight = 3)
    {
        // Not all text indexes support field weights, therefore they can boost the titles by repeating them.
        if (upsert.Titles is not { Count: > 0 } titles)
        {
            return upsert.Texts;
        }

        var result = new Dictionary<string, string>(upsert.Texts ?? []);

        foreach (var (language, title) in titles)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < titleWeight; i++)
            {
                sb.AppendIfNotEmpty(' ');
                sb.Append(title);
            }

            if (result.TryGetValue(language, out var text))
            {
                sb.Append(' ');
                sb.Append(text);
            }

            result[language] = sb.ToString();
        }

        return result;
    }
}
