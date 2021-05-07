﻿// ==========================================================================
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
        private readonly IDictionary<TKey, TValue> inner;

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
