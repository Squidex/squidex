// ==========================================================================
//  DictionaryWrapper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure
{
    public sealed class DictionaryWrapper<TKey, TValue, TSuper> : IReadOnlyDictionary<TKey, TValue> where TSuper : class, TValue where TValue : class
    {
        private readonly Func<Dictionary<TKey, TSuper>> inner;

        public DictionaryWrapper(Func<Dictionary<TKey, TSuper>> inner)
        {
            Guard.NotNull(inner, nameof(inner));

            this.inner = inner;
        }

        public IEnumerable<TKey> Keys
        {
            get { return inner().Keys; }
        }

        public IEnumerable<TValue> Values
        {
            get { return inner().Values.OfType<TValue>(); }
        }

        public int Count
        {
            get { return inner().Count; }
        }

        public TValue this[TKey key]
        {
            get { return inner()[key]; }
        }

        public bool ContainsKey(TKey key)
        {
            return inner().ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (inner().TryGetValue(key, out var temp))
            {
                value = temp as TValue;

                return value != null;
            }

            value = null;

            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<TKey, TValue>> Enumerate()
        {
            foreach (var kvp in inner())
            {
                yield return new KeyValuePair<TKey, TValue>(kvp.Key, (TValue)kvp.Value);
            }
        }
    }
}