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
    internal sealed class DictionaryComparer<TKey, TValue> : IDeepComparer where TKey : notnull
    {
        private readonly IEqualityComparer<KeyValuePair<TKey, TValue>> comparer;

        public DictionaryComparer(IDeepComparer comparer)
        {
            this.comparer = new CollectionExtensions.KeyValuePairComparer<TKey, TValue>(
                new DeepEqualityComparer<TKey>(comparer),
                new DeepEqualityComparer<TValue>(comparer));
        }

        public bool IsEquals(object? x, object? y)
        {
            var lhs = (IReadOnlyDictionary<TKey, TValue>)x!;
            var rhs = (IReadOnlyDictionary<TKey, TValue>)y!;

            if (lhs.Count != rhs.Count)
            {
                return false;
            }

            return !lhs.Except(rhs, comparer).Any();
        }
    }
}
