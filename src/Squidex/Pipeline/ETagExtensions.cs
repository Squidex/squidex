// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Pipeline
{
    public static class ETagExtensions
    {
        public static string ToManyEtag<T>(this IEnumerable<T> items, long total = 0) where T : IGenerateEtag
        {
            return $"{total}_{string.Join(";", items.Select(x => $"{x.Id}{x.Version}"))}".Sha256Base64();
        }

        public static string ToSurrogateKeys<T>(this IEnumerable<T> items) where T : IGenerateEtag
        {
            return string.Join(" ", items.Select(x => x.Id));
        }

        public static string ToEtag<T>(this T item) where T : IGenerateEtag
        {
            return item.Version.ToString();
        }
    }
}
