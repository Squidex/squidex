// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure
{
    public static class ResultList
    {
        private sealed class Impl<T> : List<T>, IResultList<T>
        {
            public long Total { get; }

            public Impl(IEnumerable<T> items, long total)
                : base(items)
            {
                Total = total;
            }
        }

        public static IResultList<T> Create<T>(IEnumerable<T> items, long total)
        {
            return new Impl<T>(items, total);
        }

        public static IResultList<T> Empty<T>()
        {
            return Create(Enumerable.Empty<T>(), 0);
        }
    }
}
