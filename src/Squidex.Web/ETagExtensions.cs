// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Web
{
    public static class ETagExtensions
    {
        private static readonly int GuidLength = Guid.Empty.ToString().Length;

        public static string ToEtag<T>(this IReadOnlyList<T> items) where T : IEntity, IEntityWithVersion
        {
            using (Profiler.Trace("CalculateEtag"))
            {
                var unhashed = Unhashed(items, 0);

                return unhashed.Sha256Base64();
            }
        }

        public static string ToEtag<T>(this IResultList<T> items) where T : IEntity, IEntityWithVersion
        {
            using (Profiler.Trace("CalculateEtag"))
            {
                var unhashed = Unhashed(items, items.Total);

                return unhashed.Sha256Base64();
            }
        }

        private static string Unhashed<T>(IReadOnlyList<T> items, long total) where T : IEntity, IEntityWithVersion
        {
            var sb = new StringBuilder();

            foreach (var item in items)
            {
                AppendItem(item, sb);

                sb.Append(";");
            }

            sb.Append(total);

            return sb.ToString();
        }

        public static string ToSurrogateKey<T>(this T item) where T : IEntity
        {
            return item.Id.ToString();
        }

        public static string ToSurrogateKeys<T>(this IReadOnlyList<T> items) where T : IEntity
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

        public static string ToEtag<T>(this T item) where T : IEntity, IEntityWithVersion
        {
            var sb = new StringBuilder();

            AppendItem(item, sb);

            return sb.ToString();
        }

        private static void AppendItem<T>(T item, StringBuilder sb) where T : IEntity, IEntityWithVersion
        {
            sb.Append(item.Id);
            sb.Append(";");
            sb.Append(item.Version);

            if (item is IEntityWithCacheDependencies withDependencies)
            {
                if (withDependencies.CacheDependencies != null)
                {
                    foreach (var dependency in withDependencies.CacheDependencies)
                    {
                        sb.Append(";");
                        sb.Append(dependency);
                    }
                }
            }
        }
    }
}
