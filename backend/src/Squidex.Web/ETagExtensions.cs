// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;

namespace Squidex.Web;

public static class ETagExtensions
{
    public static string ToEtag<T>(this IReadOnlyList<T> items) where T : IEntity, IEntityWithVersion
    {
        using (Telemetry.Activities.StartActivity("CalculateEtag"))
        {
            var hash = Create(items, 0);

            return hash;
        }
    }

    public static string ToEtag<T>(this IResultList<T> entities) where T : IEntity, IEntityWithVersion
    {
        using (Telemetry.Activities.StartActivity("CalculateEtag"))
        {
            var hash = Create(entities, entities.Total);

            return hash;
        }
    }

    private static string Create<T>(IReadOnlyList<T> entities, long total) where T : IEntity, IEntityWithVersion
    {
        using (var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
        {
            hasher.AppendData(BitConverter.GetBytes(total));

            foreach (var item in entities)
            {
                hasher.AppendData(Encoding.Default.GetBytes(item.UniqueId.ToString()));
                hasher.AppendData(BitConverter.GetBytes(item.Version));
            }

            var cacheBuffer = hasher.GetHashAndReset();
            var cacheString = BitConverter.ToString(cacheBuffer).Replace("-", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

            return cacheString;
        }
    }

    public static string ToEtag<T>(this T entity) where T : IEntity, IEntityWithVersion
    {
        return entity.Version.ToString(CultureInfo.InvariantCulture);
    }

    public static bool TryParseEtagVersion(this HttpContext httpContext, string header, out long version)
    {
        version = default;

        if (httpContext.Request.Headers.TryGetString(header, out var etag))
        {
            var span = etag.AsSpan();

            if (span.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
            {
                span = span[2..];
            }

            if (long.TryParse(span, NumberStyles.Any, CultureInfo.InvariantCulture, out version))
            {
                return true;
            }
        }

        return false;
    }
}
