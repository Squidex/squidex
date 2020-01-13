// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using DeepEqual.Syntax;

namespace Squidex.Domain.Apps.Core
{
    public sealed class DeepComparer<T> : IEqualityComparer<T>
    {
        public static readonly DeepComparer<T> Instance = new DeepComparer<T>();

        private DeepComparer()
        {
        }

        public bool Equals(T x, T y)
        {
            return x.IsDeepEqual(y);
        }

        public int GetHashCode(T obj)
        {
            return 0;
        }
    }
}
