// ==========================================================================
//  DictionaryBase.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections;
using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core
{
    public abstract class DictionaryBase<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> inner = new Dictionary<TKey, TValue>();

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

        protected Dictionary<TKey, TValue> Inner
        {
            get { return inner; }
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
