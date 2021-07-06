// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure.Collections
{
    public static class ImmutableList
    {
        private static class Empties<T>
        {
#pragma warning disable SA1401 // Fields should be private
            public static ImmutableList<T> Instance = new ImmutableList<T>();
#pragma warning restore SA1401 // Fields should be private
        }

        public static ImmutableList<T> Empty<T>()
        {
            return Empties<T>.Instance;
        }

        public static ImmutableList<T> Create<T>(params T[]? items)
        {
            if (items == null || items.Length == 0)
            {
                return Empty<T>();
            }

            return new ImmutableList<T>(items.ToList());
        }

        public static ImmutableList<T> ToImmutableList<T>(this IEnumerable<T> source)
        {
            var inner = source.ToList();

            if (inner.Count == 0)
            {
                return Empty<T>();
            }

            return new ImmutableList<T>(inner);
        }
    }
}
