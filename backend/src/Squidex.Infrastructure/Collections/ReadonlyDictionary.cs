// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Collections;

public static class ReadonlyDictionary
{
    private static class Empties<TKey, TValue> where TKey : notnull
    {
        public static readonly ReadonlyDictionary<TKey, TValue> Instance = new ReadonlyDictionary<TKey, TValue>();
    }

    public static ReadonlyDictionary<TKey, TValue> Empty<TKey, TValue>() where TKey : notnull
    {
        return Empties<TKey, TValue>.Instance;
    }

    public static ReadonlyDictionary<TKey, TValue> ToReadonlyDictionary<TKey, TValue>(this Dictionary<TKey, TValue> source) where TKey : notnull
    {
        if (source.Count == 0)
        {
            return Empty<TKey, TValue>();
        }

        return new ReadonlyDictionary<TKey, TValue>(source);
    }

    public static ReadonlyDictionary<TKey, TValue> ToReadonlyDictionary<TKey, TValue>(this IEnumerable<TValue> source, Func<TValue, TKey> keySelector) where TKey : notnull
    {
        var inner = source.ToDictionary(keySelector);

        if (inner.Count == 0)
        {
            return Empty<TKey, TValue>();
        }

        return new ReadonlyDictionary<TKey, TValue>(inner);
    }

    public static ReadonlyDictionary<TKey, TValue> ToReadonlyDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector) where TKey : notnull
    {
        var inner = source.ToDictionary(keySelector, elementSelector);

        if (inner.Count == 0)
        {
            return Empty<TKey, TValue>();
        }

        return new ReadonlyDictionary<TKey, TValue>(inner);
    }
}
