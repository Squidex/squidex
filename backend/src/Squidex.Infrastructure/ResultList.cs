// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Infrastructure;

public static class ResultList
{
    private sealed class Impl<T> : ReadonlyList<T>, IResultList<T>
    {
        public long Total { get; }

        public Impl(List<T> items, long total)
            : base(items)
        {
            Total = total;
        }
    }

    private static class Empties<T>
    {
#pragma warning disable SA1401 // Fields should be private
        public static Impl<T> Instance = new Impl<T>([], 0);
#pragma warning restore SA1401 // Fields should be private
    }

    public static IResultList<T> Empty<T>()
    {
        return Empties<T>.Instance;
    }

    public static IResultList<T> Create<T>(long total, List<T> items)
    {
        return new Impl<T>(items, total);
    }

    public static IResultList<T> Create<T>(long total, IEnumerable<T> items)
    {
        return new Impl<T>(items.ToList(), total);
    }

    public static IResultList<T> CreateFrom<T>(long total, params T[] items)
    {
        return new Impl<T>(items.ToList(), total);
    }

    public static IResultList<T> Sorted<T, TKey>(this IResultList<T> input, Func<T, TKey> idProvider, IReadOnlyList<TKey> ids) where TKey : notnull where T : class
    {
        if (input.Count == 0)
        {
            return Empty<T>();
        }

        var result = new List<T>(ids.Count);

        if (input.Count >= 5)
        {
            var dictionary = new Dictionary<TKey, T>(input.Count);

            foreach (var item in input)
            {
                dictionary[idProvider(item)] = item;
            }

            foreach (var id in ids)
            {
                if (dictionary.TryGetValue(id, out var item))
                {
                    result.Add(item);
                }
            }
        }
        else
        {
            foreach (var id in ids)
            {
                T? item = null;

                foreach (var candidate in input)
                {
                    if (Equals(id, idProvider(candidate)))
                    {
                        item = candidate;
                        break;
                    }
                }

                if (item != null)
                {
                    result.Add(item);
                }
            }
        }

        return Create(input.Total, result);
    }
}
