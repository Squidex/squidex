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
    }
}
