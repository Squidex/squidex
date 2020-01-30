// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Reflection.Equality
{
    internal sealed class DefaultComparer : IDeepComparer
    {
        public bool IsEquals(object? x, object? y)
        {
            if (Equals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            var type = x.GetType();

            if (type != y.GetType())
            {
                return false;
            }

            var inner = SimpleEquals.BuildInner(type);

            return inner.IsEquals(x, y);
        }
    }
}
