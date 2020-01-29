// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using Squidex.Infrastructure.Reflection.Internal;

namespace Squidex.Infrastructure.Reflection.Equality
{
    internal sealed class CollectionComparer : IDeepComparer
    {
        private readonly IDeepComparer itemComparer;
        private readonly PropertyAccessor? sizeProperty;

        public CollectionComparer(IDeepComparer itemComparer, PropertyAccessor? sizeProperty)
        {
            this.itemComparer = itemComparer;
            this.sizeProperty = sizeProperty;
        }

        public bool IsEquals(object? x, object? y)
        {
            var lhs = (IEnumerable)x!;
            var rhs = (IEnumerable)y!;

            if (sizeProperty != null)
            {
                var sizeLhs = sizeProperty.Get(lhs);
                var sizeRhs = sizeProperty.Get(rhs);

                if (!Equals(sizeLhs, sizeRhs))
                {
                    return false;
                }
            }

            var enumeratorLhs = lhs.GetEnumerator();
            var enumeratorRhs = rhs.GetEnumerator();

            while (true)
            {
                var movedLhs = enumeratorLhs.MoveNext();
                var movedRhs = enumeratorRhs.MoveNext();

                if (movedRhs != movedLhs)
                {
                    return false;
                }

                if (movedRhs)
                {
                    if (!itemComparer.IsEquals(enumeratorLhs.Current, enumeratorRhs.Current))
                    {
                        return false;
                    }
                }
                else
                {
                    break;
                }
            }

            return true;
        }
    }
}
