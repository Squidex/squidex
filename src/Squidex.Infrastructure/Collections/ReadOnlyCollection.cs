// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Squidex.Infrastructure.Collections
{
    public static class ReadOnlyCollection
    {
        private static class Empties<T>
        {
            public static readonly ReadOnlyCollection<T> Collection = new ReadOnlyCollection<T>(new List<T>());
        }

        public static ReadOnlyCollection<T> Create<T>(params T[] items)
        {
            return new ReadOnlyCollection<T>(items.ToList());
        }

        public static ReadOnlyCollection<T> Empty<T>()
        {
            return Empties<T>.Collection;
        }

        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
        {
            return new ReadOnlyCollection<T>(source.ToList());
        }
    }
}
