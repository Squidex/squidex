// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Squidex.Infrastructure
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable)
        {
            var random = new Random();

            return enumerable.OrderBy(x => random.Next()).ToList();
        }

        public static ImmutableDictionary<TKey, TValue> SetItem<TKey, TValue>(this ImmutableDictionary<TKey, TValue> dictionary, TKey key, Func<TValue, TValue> updater)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                var newValue = updater(value);

                if (!Equals(newValue, value))
                {
                    return dictionary.SetItem(key, newValue);
                }
            }

            return dictionary;
        }

        public static bool TryGetValue<TKey, TValue, TBase>(this IReadOnlyDictionary<TKey, TValue> values, TKey key, out TBase item) where TValue : TBase
        {
            if (values.TryGetValue(key, out var value))
            {
                item = value;

                return true;
            }
            else
            {
                item = default(TBase);

                return false;
            }
        }

        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T value)
        {
            return source.Concat(Enumerable.Repeat(value, 1));
        }

        public static int SequentialHashCode<T>(this IEnumerable<T> collection)
        {
            return collection.SequentialHashCode(EqualityComparer<T>.Default);
        }

        public static int SequentialHashCode<T>(this IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            var hashCode = 17;

            foreach (var item in collection)
            {
                if (!Equals(item, null))
                {
                    hashCode = (hashCode * 23) + comparer.GetHashCode(item);
                }
            }

            return hashCode;
        }

        public static int OrderedHashCode<T>(this IEnumerable<T> collection)
        {
            return collection.OrderedHashCode(EqualityComparer<T>.Default);
        }

        public static int OrderedHashCode<T>(this IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            Guard.NotNull(comparer, nameof(comparer));

            var hashCodes = collection.Where(x => !Equals(x, null)).Select(x => x.GetHashCode()).OrderBy(x => x).ToArray();

            var hashCode = 17;

            foreach (var code in hashCodes)
            {
                hashCode = (hashCode * 23) + code;
            }

            return hashCode;
        }

        public static int DictionaryHashCode<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return DictionaryHashCode(dictionary, EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);
        }

        public static int DictionaryHashCode<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            var hashCode = 17;

            foreach (var kvp in dictionary.OrderBy(x => x.Key))
            {
                hashCode = (hashCode * 23) + keyComparer.GetHashCode(kvp.Key);

                if (!Equals(kvp.Value, null))
                {
                    hashCode = (hashCode * 23) + valueComparer.GetHashCode(kvp.Value);
                }
            }

            return hashCode;
        }

        public static bool EqualsDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, IReadOnlyDictionary<TKey, TValue> other)
        {
            return EqualsDictionary(dictionary, other, EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);
        }

        public static bool EqualsDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, IReadOnlyDictionary<TKey, TValue> other, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            var comparer = new KeyValuePairComparer<TKey, TValue>(keyComparer, valueComparer);

            return other != null && dictionary.Count == other.Count && !dictionary.Except(other, comparer).Any();
        }

        public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.GetOrCreate(key, _ => default(TValue));
        }

        public static TValue GetOrAddDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.GetOrAdd(key, _ => default(TValue));
        }

        public static TValue GetOrNew<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) where TValue : class, new()
        {
            return dictionary.GetOrCreate(key, _ => new TValue());
        }

        public static TValue GetOrAddNew<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : class, new()
        {
            return dictionary.GetOrAdd(key, _ => new TValue());
        }

        public static TValue GetOrCreate<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> creator)
        {
            if (!dictionary.TryGetValue(key, out var result))
            {
                result = creator(key);
            }

            return result;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> creator)
        {
            if (!dictionary.TryGetValue(key, out var result))
            {
                result = creator(key);

                dictionary.Add(key, result);
            }

            return result;
        }

        public static TValue GetOrAdd<TKey, TContext, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TContext context, Func<TKey, TContext, TValue> creator)
        {
            if (!dictionary.TryGetValue(key, out var result))
            {
                result = creator(key, context);

                dictionary.Add(key, result);
            }

            return result;
        }

        public static void Foreach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }

        public sealed class KeyValuePairComparer<TKey, TValue> : IEqualityComparer<KeyValuePair<TKey, TValue>>
        {
            private readonly IEqualityComparer<TKey> keyComparer;
            private readonly IEqualityComparer<TValue> valueComparer;

            public KeyValuePairComparer(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
            {
                this.keyComparer = keyComparer;
                this.valueComparer = valueComparer;
            }

            public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
            {
                return keyComparer.Equals(x.Key, y.Key) && valueComparer.Equals(x.Value, y.Value);
            }

            public int GetHashCode(KeyValuePair<TKey, TValue> obj)
            {
                return keyComparer.GetHashCode(obj.Key) ^ valueComparer.GetHashCode(obj.Value);
            }
        }
    }
}