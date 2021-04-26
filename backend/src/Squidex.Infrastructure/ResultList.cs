// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

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

        public static IResultList<T> Create<T>(long total, IEnumerable<T> items)
        {
            return new Impl<T>(items, total);
        }

        public static IResultList<T> CreateFrom<T>(long total, params T[] items)
        {
            return new Impl<T>(items, total);
        }
    }
}
