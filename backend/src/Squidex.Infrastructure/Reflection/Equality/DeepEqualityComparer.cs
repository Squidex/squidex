// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure.Reflection.Equality
{
    public sealed class DeepEqualityComparer<T> : IEqualityComparer<T>
    {
        public static readonly DeepEqualityComparer<T> Default = new DeepEqualityComparer<T>();
        private readonly IDeepComparer comparer;

        public DeepEqualityComparer(IDeepComparer? comparer = null)
        {
            this.comparer = comparer ?? SimpleEquals.Build(typeof(T));
        }

        public bool Equals(T? x, T? y)
        {
            return comparer.IsEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return 0;
        }
    }
}
