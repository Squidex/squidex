// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Collections;

public static class ReadonlyList
{
    private static class Empties<T>
    {
#pragma warning disable SA1401 // Fields should be private
        public static readonly ReadonlyList<T> Instance = new ReadonlyList<T>();
#pragma warning restore SA1401 // Fields should be private
    }

    public static ReadonlyList<T> Empty<T>()
    {
        return Empties<T>.Instance;
    }

    public static ReadonlyList<T> Create<T>(params T[]? items)
    {
        if (items == null || items.Length == 0)
        {
            return Empty<T>();
        }

        return new ReadonlyList<T>(items.ToList());
    }

    public static ReadonlyList<T> ToReadonlyList<T>(this IEnumerable<T> source)
    {
        var inner = source.ToList();

        if (inner.Count == 0)
        {
            return Empty<T>();
        }

        return new ReadonlyList<T>(inner);
    }
}
