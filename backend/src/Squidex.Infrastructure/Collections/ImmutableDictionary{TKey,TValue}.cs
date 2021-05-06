// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Squidex.Infrastructure.Collections
{
    public class ImmutableDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IEquatable<ImmutableDictionary<TKey, TValue>> where TKey : notnull
    {
        private static readonly Dictionary<TKey, TValue> EmptyInner = new Dictionary<TKey, TValue>();
        private IDictionary<TKey, TValue> inner;

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
            get => inner.Keys;
        }

        public IEnumerable<TValue> Values
        {
            get => inner.Values;
        }

        public int Count
        {
            get => inner.Count;
        }

        public ImmutableDictionary()
            : this(EmptyInner)
        {
        }

        public ImmutableDictionary(IDictionary<TKey, TValue> inner)
        {
            Guard.NotNull(inner, nameof(inner));

            this.inner = inner;
        }

        public ImmutableDictionary<TKey, TValue> Set(TKey key, TValue value, IEqualityComparer<TValue>? valueComparer = null)
        {
            return Set<ImmutableDictionary<TKey, TValue>>(key, value, valueComparer);
        }

        public TArray Set<TArray>(TKey key, TValue value, IEqualityComparer<TValue>? valueComparer = null) where TArray : ImmutableDictionary<TKey, TValue>
        {
            if (!TryGetValue(key, out var found) || !IsEqual(value, found, valueComparer))
            {
                var clone = new Dictionary<TKey, TValue>(inner)
                {
                    [key] = value
                };

                return Create<TArray>(clone);
            }

            return Self<TArray>();
        }

        private static bool IsEqual(TValue lhs, TValue rhs, IEqualityComparer<TValue>? comparer = null)
        {
            comparer ??= EqualityComparer<TValue>.Default;

            return comparer.Equals(lhs, rhs);
        }

        public ImmutableDictionary<TKey, TValue> RemoveKey(TKey key)
        {
            return RemoveKey<ImmutableDictionary<TKey, TValue>>(key);
        }

        public TArray RemoveKey<TArray>(TKey key) where TArray : ImmutableDictionary<TKey, TValue>
        {
            if (!inner.ContainsKey(key))
            {
                return Self<TArray>();
            }

            if (Count == 1)
            {
                return Create<TArray>(EmptyInner);
            }

            var clone = new Dictionary<TKey, TValue>(inner);

            clone.Remove(key);

            return Create<TArray>(clone);
        }

        private TArray Self<TArray>() where TArray : ImmutableDictionary<TKey, TValue>
        {
            return (this as TArray)!;
        }

        private TArray Create<TArray>(Dictionary<TKey, TValue> clone) where TArray : ImmutableDictionary<TKey, TValue>
        {
            var newClone = (TArray)MemberwiseClone();

            newClone.inner = clone;

            return newClone;
        }

        public bool ContainsKey(TKey key)
        {
            return inner.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return inner.TryGetValue(key, out value);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerable(inner).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        private static IEnumerable<TItem> GetEnumerable<TItem>(IEnumerable<TItem> collection)
        {
            return collection;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ImmutableDictionary<TKey, TValue>);
        }

        public bool Equals(ImmutableDictionary<TKey, TValue>? other)
        {
            return this.EqualsDictionary(other);
        }

        public override int GetHashCode()
        {
            return this.DictionaryHashCode();
        }
    }
}
