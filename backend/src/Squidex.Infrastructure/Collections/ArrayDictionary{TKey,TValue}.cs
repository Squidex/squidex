// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#pragma warning disable IDE0044 // Add readonly modifier

namespace Squidex.Infrastructure.Collections
{
    public class ArrayDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue> where TKey : notnull
    {
        private readonly IEqualityComparer<TKey> keyComparer;
        private KeyValuePair<TKey, TValue>[] items;

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
            Guard.NotNull(items);
            Guard.NotNull(keyComparer);

            this.items = items;

            this.keyComparer = keyComparer;
        }

        public bool IsUnchanged(KeyValuePair<TKey, TValue>[] values)
        {
            return ReferenceEquals(values, items);
        }

        public ArrayDictionary<TKey, TValue> With(TKey key, TValue value, IEqualityComparer<TValue>? valueComparer = null)
        {
            return With<ArrayDictionary<TKey, TValue>>(key, value, valueComparer);
        }

        public TArray With<TArray>(TKey key, TValue value, IEqualityComparer<TValue>? valueComparer = null) where TArray : ArrayDictionary<TKey, TValue>
        {
            var index = IndexOf(key);

            if (index < 0)
            {
                var result = new KeyValuePair<TKey, TValue>[items.Length + 1];

                Array.Copy(items, 0, result, 0, items.Length);

                result[^1] = new KeyValuePair<TKey, TValue>(key, value);

                return Create<TArray>(result);
            }

            var existing = items[index].Value;

            if (valueComparer == null || !valueComparer.Equals(value, existing))
            {
                var result = new KeyValuePair<TKey, TValue>[items.Length];

                Array.Copy(items, 0, result, 0, items.Length);

                result[index] = new KeyValuePair<TKey, TValue>(key, value);

                return Create<TArray>(result);
            }

            return Self<TArray>();
        }

        public ArrayDictionary<TKey, TValue> Without(TKey key)
        {
            return Without<ArrayDictionary<TKey, TValue>>(key);
        }

        public TArray Without<TArray>(TKey key) where TArray : ArrayDictionary<TKey, TValue>
        {
            var index = IndexOf(key);

            if (index < 0)
            {
                return Self<TArray>();
            }

            var result = Array.Empty<KeyValuePair<TKey, TValue>>();

            if (items.Length > 1)
            {
                result = new KeyValuePair<TKey, TValue>[items.Length - 1];

                var afterIndex = items.Length - index - 1;

                Array.Copy(items, 0, result, 0, index);
                Array.Copy(items, index, result, index, afterIndex);
            }

            return Create<TArray>(result);
        }

        private TArray Self<TArray>() where TArray : ArrayDictionary<TKey, TValue>
        {
            return (this as TArray)!;
        }

        private TArray Create<TArray>(KeyValuePair<TKey, TValue>[] newItems) where TArray : ArrayDictionary<TKey, TValue>
        {
            if (ReferenceEquals(items, newItems))
            {
                return Self<TArray>();
            }

            var newClone = (TArray)MemberwiseClone();

            newClone.items = newItems;

            return newClone;
        }

        public bool ContainsKey(TKey key)
        {
            var index = IndexOf(key);

            return index >= 0;
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            var index = IndexOf(key);

            if (index >= 0)
            {
                value = items[index].Value;

                return true;
            }
            else
            {
                value = default!;

                return false;
            }
        }

        private int IndexOf(TKey key)
        {
            for (var i = 0; i < items.Length; i++)
            {
                if (keyComparer.Equals(items[i].Key, key))
                {
                    return i;
                }
            }

            return -1;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerable(items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        private static IEnumerable<T2> GetEnumerable<T2>(IEnumerable<T2> array)
        {
            return array;
        }
    }
}
