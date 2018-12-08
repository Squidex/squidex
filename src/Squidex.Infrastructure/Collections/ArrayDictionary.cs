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
    public static class ArrayDictionary
    {
        public static ArrayDictionary<TKey, TValue> ToArrayDictionary<TKey, TValue>(this IEnumerable<TValue> source, Func<TValue, TKey> keyExtractor)
        {
            return new ArrayDictionary<TKey, TValue>(source.Select(x => new KeyValuePair<TKey, TValue>(keyExtractor(x), x)).ToArray());
        }
    }
}
