// ==========================================================================
//  CollectionExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable InvertIf
// ReSharper disable LoopCanBeConvertedToQuery

namespace Squidex.Infrastructure
{
    public static class CollectionExtensions
    {
        public static int SequentialHashCode<T>(this IEnumerable<T> collection)
        {
            return collection.SequentialHashCode(EqualityComparer<T>.Default);
        }

        public static int SequentialHashCode<T>(this IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            var hashCode = 17;

            foreach (var item in collection)
            {
                if (item != null)
                {
                    hashCode = hashCode * 23 + item.GetHashCode();
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

            var hashCodes = collection.Where(x => x != null).Select(x => x.GetHashCode()).OrderBy(x => x).ToArray();

            var hashCode = 17;

            foreach (var code in hashCodes)
            {
                hashCode = hashCode * 23 + code;
            }

            return hashCode;
        }

        public static int DictionaryHashCode<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return DictionaryHashCode(dictionary, EqualityComparer<TValue>.Default);
        }

        public static int DictionaryHashCode<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEqualityComparer<TValue> comparer)
        {
            Guard.NotNull(comparer, nameof(comparer));

            var hashCode = 17;

            foreach (var kvp in dictionary.OrderBy(x => x.Key))
            {
                hashCode = hashCode * 23 + kvp.Key.GetHashCode();

                if (kvp.Value != null)
                {
                    hashCode = hashCode * 23 + comparer.GetHashCode(kvp.Value);
                }
            }

            return hashCode;
        }

        public static bool EqualsDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> other)
        {
            return other != null && dictionary.Count == other.Count && !dictionary.Except(other).Any();
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
            if (!dictionary.TryGetValue(key, out TValue result))
            {
                result = creator(key);
            }

            return result;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> creator)
        {
            if (!dictionary.TryGetValue(key, out TValue result))
            {
                result = creator(key);

                dictionary.Add(key, result);
            }

            return result;
        }
    }
}