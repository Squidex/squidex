// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web;

public static class ETagExtensions
{
    public static string ToEtag<T>(this IReadOnlyList<T> items) where T : Entity
    {
        using (Telemetry.Activities.StartActivity("CalculateEtag"))
        {
            var hash = Create(items, 0);

            return hash;
        }
    }

    public static string ToEtag<T>(this IResultList<T> entities) where T : Entity
    {
        using (Telemetry.Activities.StartActivity("CalculateEtag"))
        {
            var hash = Create(entities, entities.Total);

            return hash;
        }
    }

    private static string Create<T>(IReadOnlyList<T> entities, long total) where T : Entity
    {
        using (var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
        {
            hasher.AppendLong(total);

            foreach (var item in entities)
            {
                hasher.AppendString(item.UniqueId.ToString());
                hasher.AppendLong(item.Version);
            }

            return hasher.GetHexStringAndReset();
        }
    }

    public static string ToEtag<T>(this T entity) where T : Entity
    {
        return entity.Version.ToString(CultureInfo.InvariantCulture);
    }

    public static bool TryParseEtagVersion(this HttpContext httpContext, string header, out long version)
    {
        version = default;

        if (!httpContext.Request.Headers.TryGetValue(header, out var etagValue))
        {
            return false;
        }

        if (!EntityTagHeaderValue.TryParse(etagValue.ToString(), out var etag))
        {
            return false;
        }

        var tag = etag.Tag.AsSpan().Trim('"');

        return long.TryParse(tag, NumberStyles.Any, CultureInfo.InvariantCulture, out version);
    }
}
