// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure.Collections
{
    public static class ImmutableDictionary
    {
        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this IEnumerable<TValue> source, Func<TValue, TKey> keyExtractor) where TKey : notnull
        {
            return new ImmutableDictionary<TKey, TValue>(source.ToDictionary(keyExtractor));
        }
    }
}
