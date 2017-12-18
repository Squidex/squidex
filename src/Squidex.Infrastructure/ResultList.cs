// ==========================================================================
//  ResultList.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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

        public static IResultList<T> Create<T>(IEnumerable<T> items, long total)
        {
            return new Impl<T>(items, total);
        }
    }
}
