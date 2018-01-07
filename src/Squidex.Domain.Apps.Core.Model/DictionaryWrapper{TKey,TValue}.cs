// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Squidex.Domain.Apps.Core
{
    public abstract class DictionaryWrapper<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly ImmutableDictionary<TKey, TValue> inner;

        public TValue this[TKey key]
        {
            get { return inner[key]; }
        }

        public IEnumerable<TKey> Keys
        {
            get { return inner.Keys; }
        }

        public IEnumerable<TValue> Values
        {
            get { return inner.Values; }
        }

        public int Count
        {
            get { return inner.Count; }
        }

        protected ImmutableDictionary<TKey, TValue> Inner
        {
            get { return inner; }
        }

        protected DictionaryWrapper(ImmutableDictionary<TKey, TValue> inner)
        {
            this.inner = inner;
        }

        public bool ContainsKey(TKey key)
        {
            return inner.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return inner.TryGetValue(key, out value);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return inner.GetEnumerator();
        }
    }
}
