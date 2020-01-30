// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Reflection.Equality
{
    internal sealed class ArrayComparer<T> : IDeepComparer
    {
        private readonly IDeepComparer itemComparer;

        public ArrayComparer(IDeepComparer itemComparer)
        {
            this.itemComparer = itemComparer;
        }

        public bool IsEquals(object? x, object? y)
        {
            var lhs = (T[])x!;
            var rhs = (T[])y!;

            if (lhs.Length != rhs.Length)
            {
                return false;
            }

            for (var i = 0; i < lhs.Length; i++)
            {
                if (!itemComparer.IsEquals(lhs[i], rhs[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
