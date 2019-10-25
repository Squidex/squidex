// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure
{
    public static class HashSet
    {
        public static HashSet<T> Of<T>(params T[] items)
        {
            return new HashSet<T>(items);
        }

        public static HashSet<T> Of<T>(T item1)
        {
            return new HashSet<T> { item1 };
        }

        public static HashSet<T> Of<T>(T item1, T item2)
        {
            return new HashSet<T> { item1, item2 };
        }
    }
}
