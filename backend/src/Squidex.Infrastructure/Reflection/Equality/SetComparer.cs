// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure.Reflection.Equality
{
    internal sealed class SetComparer<T> : IDeepComparer
    {
        private readonly IEqualityComparer<T> equalityComparer;

        public SetComparer(IDeepComparer comparer)
        {
            equalityComparer = new DeepEqualityComparer<T>(comparer);
        }

        public bool IsEquals(object? x, object? y)
        {
            var lhs = (ISet<T>)x!;
            var rhs = (ISet<T>)y!;

            if (lhs.Count != rhs.Count)
            {
                return false;
            }

            return lhs.Intersect(rhs, equalityComparer).Count() == rhs.Count;
        }
    }
}
