// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Pipeline
{
    public static class ETagExtensions
    {
        private static readonly int GuidLength = Guid.Empty.ToString().Length;

        public static string ToManyEtag<T>(this IReadOnlyList<T> items, long total = 0) where T : IGenerateETag
        {
            using (Profiler.Trace("CalculateEtag"))
            {
                var unhashed = Unhashed(items, total);

                return unhashed.Sha256Base64();
            }
        }

        private static string Unhashed<T>(IReadOnlyList<T> items, long total) where T : IGenerateETag
        {
            var sb = new StringBuilder((items.Count * (GuidLength + 4)) + 10);

            sb.Append(total);
            sb.Append("_");

            if (items.Count > 0)
            {
                sb.Append(items[0].Id.ToString());
                sb.Append(items[0].Version);

                for (var i = 1; i < items.Count; i++)
                {
                    sb.Append(";");
                    sb.Append(items[i].Id.ToString());
                    sb.Append(items[i].Version);
                }
            }

            return sb.ToString().Sha256Base64();
        }

        public static string ToSurrogateKeys<T>(this IReadOnlyList<T> items) where T : IGenerateETag
        {
            if (items.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(items.Count * (GuidLength + 1));

            sb.Append(items[0].Id.ToString());

            for (var i = 1; i < items.Count; i++)
            {
                sb.Append(" ");
                sb.Append(items[i].Id.ToString());
            }

            return sb.ToString();
        }

        public static string ToEtag<T>(this T item) where T : IGenerateETag
        {
            return item.Version.ToString();
        }
    }
}
