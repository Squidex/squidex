// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure.Collections
{
    public class ArrayDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly IEqualityComparer<TKey> keyComparer;
        private readonly KeyValuePair<TKey, TValue>[] items;

        public TValue this[TKey key]
        {
            get
            {
                if (!TryGetValue(key, out var value))
                {
                    throw new KeyNotFoundException();
                }

                return value;
            }
        }

        public IEnumerable<TKey> Keys
        {
            get { return items.Select(x => x.Key); }
        }

        public IEnumerable<TValue> Values
        {
            get { return items.Select(x => x.Value); }
        }

        public int Count
        {
            get { return items.Length; }
        }

        public ArrayDictionary()
            : this(EqualityComparer<TKey>.Default, Array.Empty<KeyValuePair<TKey, TValue>>())
        {
        }

        public ArrayDictionary(KeyValuePair<TKey, TValue>[] items)
            : this(EqualityComparer<TKey>.Default, items)
        {
        }

        public ArrayDictionary(IEqualityComparer<TKey> keyComparer, KeyValuePair<TKey, TValue>[] items)
        {
            Guard.NotNull(items, nameof(items));
            Guard.NotNull(keyComparer, nameof(keyComparer));

            this.items = items;

            this.keyComparer = keyComparer;
        }

        public KeyValuePair<TKey, TValue>[] With(TKey key, TValue value)
        {
            var result = new List<KeyValuePair<TKey, TValue>>(Math.Max(items.Length, 1));

            var wasReplaced = false;

            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];

                if (wasReplaced || !keyComparer.Equals(item.Key, key))
                {
                    result.Add(item);
                }
                else
                {
                    result.Add(new KeyValuePair<TKey, TValue>(key, value));
                    wasReplaced = true;
                }
            }

            if (!wasReplaced)
            {
                result.Add(new KeyValuePair<TKey, TValue>(key, value));
            }

            return result.ToArray();
        }

        public KeyValuePair<TKey, TValue>[] Without(TKey key)
        {
            var result = new List<KeyValuePair<TKey, TValue>>(Math.Max(items.Length, 1));

            var wasRemoved = false;

            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];

                if (wasRemoved || !keyComparer.Equals(item.Key, key))
                {
                    result.Add(item);
                }
                else
                {
                    wasRemoved = true;
                }
            }

            return result.ToArray();
        }

        public bool ContainsKey(TKey key)
        {
            for (var i = 0; i < items.Length; i++)
            {
                if (keyComparer.Equals(items[i].Key, key))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            for (var i = 0; i < items.Length; i++)
            {
                if (keyComparer.Equals(items[i].Key, key))
                {
                    value = items[i].Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerable(items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        private static IEnumerable<T2> GetEnumerable<T2>(T2[] array)
        {
            return array;
        }
    }
}
