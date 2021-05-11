// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure.Collections
{
    public static class ImmutableDictionary
    {
        private static class Empties<TKey, TValue> where TKey : notnull
        {
#pragma warning disable SA1401 // Fields should be private
            public static ImmutableDictionary<TKey, TValue> Instance = new ImmutableDictionary<TKey, TValue>();
#pragma warning restore SA1401 // Fields should be private
        }

        public static ImmutableDictionary<TKey, TValue> Empty<TKey, TValue>() where TKey : notnull
        {
            return Empties<TKey, TValue>.Instance;
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this Dictionary<TKey, TValue> source) where TKey : notnull
        {
            if (source.Count == 0)
            {
                return Empty<TKey, TValue>();
            }

            return new ImmutableDictionary<TKey, TValue>(source);
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this IEnumerable<TValue> source, Func<TValue, TKey> keySelector) where TKey : notnull
        {
            var inner = source.ToDictionary(keySelector);

            if (inner.Count == 0)
            {
                return Empty<TKey, TValue>();
            }

            return new ImmutableDictionary<TKey, TValue>(inner);
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector) where TKey : notnull
        {
            var inner = source.ToDictionary(keySelector, elementSelector);

            if (inner.Count == 0)
            {
                return Empty<TKey, TValue>();
            }

            return new ImmutableDictionary<TKey, TValue>(inner);
        }
    }
}
