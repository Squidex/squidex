// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Squidex.Infrastructure
{
    public static class CollectionExtensions
    {
        public static bool TryAdd<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source, TKey key, TValue value, [MaybeNullWhen(false)] out Dictionary<TKey, TValue> result) where TKey : notnull
        {
            result = null;

            if (!source.ContainsKey(key))
            {
                var clone = new Dictionary<TKey, TValue>(source)
                {
                    [key] = value
                };

                result = clone;

                return true;
            }

            return false;
        }

        public static bool TrySet<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source, TKey key, TValue value, [MaybeNullWhen(false)] out Dictionary<TKey, TValue> result) where TKey : notnull
        {
            result = null;

            if (!source.TryGetValue(key, out var found) || !Equals(found, value))
            {
                var clone = new Dictionary<TKey, TValue>(source)
                {
                    [key] = value
                };

                result = clone;

                return true;
            }

            return false;
        }

        public static bool TryUpdate<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source, TKey key, TValue value, [MaybeNullWhen(false)] out Dictionary<TKey, TValue> result) where TKey : notnull
        {
            result = null;

            if (source.TryGetValue(key, out var found) && !Equals(found, value))
            {
                var clone = new Dictionary<TKey, TValue>(source)
                {
                    [key] = value
                };

                result = clone;

                return true;
            }

            return false;
        }

        public static bool TryRemove<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source, TKey key, [MaybeNullWhen(false)] out Dictionary<TKey, TValue> result) where TKey : notnull
        {
            result = null;

            if (source.ContainsKey(key))
            {
                var clone = new Dictionary<TKey, TValue>(source);

                result = clone;
                result.Remove(key);

                return true;
            }

            return false;
        }

        public static bool SetEquals<T>(this IReadOnlyCollection<T> source, IReadOnlyCollection<T> other)
        {
            return source.Count == other.Count && source.Intersect(other).Count() == other.Count;
        }

        public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
        {
            TSource[]? bucket = null;

            var bucketIndex = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                {
                    bucket = new TSource[size];
                }

                bucket[bucketIndex++] = item;

                if (bucketIndex != size)
                {
                    continue;
                }

                yield return bucket;

                bucket = null;
                bucketIndex = 0;
            }

            if (bucket != null && bucketIndex > 0)
            {
                yield return bucket.Take(bucketIndex);
            }
        }

        public static bool SetEquals<T>(this IReadOnlyCollection<T> source, IReadOnlyCollection<T> other, IEqualityComparer<T> comparer)
        {
            return source.Count == other.Count && source.Intersect(other, comparer).Count() == other.Count;
        }

        public static IEnumerable<T> Reverse<T>(this IEnumerable<T> source, bool reverse)
        {
            return reverse ? source.Reverse() : source;
        }

        public static IResultList<T> SortSet<T, TKey>(this IResultList<T> input, Func<T, TKey> idProvider, IReadOnlyList<TKey> ids) where T : class
        {
            return ResultList.Create(input.Total, SortList(input, idProvider, ids));
        }

        public static IEnumerable<T> SortList<T, TKey>(this IEnumerable<T> input, Func<T, TKey> idProvider, IReadOnlyList<TKey> ids) where T : class
        {
            return ids.Select(id => input.FirstOrDefault(x => Equals(idProvider(x), id))).NotNull();
        }

        public static IEnumerable<T> Duplicates<T>(this IEnumerable<T> input)
        {
            return input.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key);
        }

        public static int IndexOf<T>(this IEnumerable<T> input, Func<T, bool> predicate)
        {
            var i = 0;

            foreach (var item in input)
            {
                if (predicate(item))
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        public static IEnumerable<TResult> Duplicates<TResult, T>(this IEnumerable<T> input, Func<T, TResult> selector)
        {
            return input.GroupBy(selector).Where(x => x.Count() > 1).Select(x => x.Key);
        }

        public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source)
        {
            foreach (var value in source)
            {
                target.Add(value);
            }
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable)
        {
            var random = new Random();

            return enumerable.OrderBy(x => random.Next()).ToList();
        }

        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source) where T : class
        {
            return source.Where(x => x != null)!;
        }

        public static IEnumerable<TOut> NotNull<TIn, TOut>(this IEnumerable<TIn> source, Func<TIn, TOut?> selector) where TOut : class
        {
            return source.Select(selector).Where(x => x != null)!;
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

        public static int DictionaryHashCode<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary) where TKey : notnull
        {
            return DictionaryHashCode(dictionary, EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);
        }

        public static int DictionaryHashCode<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer) where TKey : notnull
        {
            var hashCode = 17;

            foreach (var (key, value) in dictionary.OrderBy(x => x.Key))
            {
                hashCode = (hashCode * 23) + keyComparer.GetHashCode(key);

                if (!Equals(value, null))
                {
                    hashCode = (hashCode * 23) + valueComparer.GetHashCode(value);
                }
            }

            return hashCode;
        }

        public static bool EqualsDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, IReadOnlyDictionary<TKey, TValue>? other) where TKey : notnull
        {
            return EqualsDictionary(dictionary, other, EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);
        }

        public static bool EqualsDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, IReadOnlyDictionary<TKey, TValue>? other, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer) where TKey : notnull
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(dictionary, other))
            {
                return true;
            }

            if (dictionary.Count != other.Count)
            {
                return false;
            }

            var comparer = new KeyValuePairComparer<TKey, TValue>(keyComparer, valueComparer);

            return !dictionary.Except(other, comparer).Any();
        }

        public static bool EqualsList<T>(this IReadOnlyList<T> list, IReadOnlyList<T>? other)
        {
            return EqualsList(list, other, EqualityComparer<T>.Default);
        }

        public static bool EqualsList<T>(this IReadOnlyList<T> list, IReadOnlyList<T>? other, IEqualityComparer<T> comparer)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(list, other))
            {
                return true;
            }

            if (list.Count != other.Count)
            {
                return false;
            }

            for (var i = 0; i < list.Count; i++)
            {
                if (!comparer.Equals(list[i], other[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary) where TKey : notnull
        {
            return dictionary.ToDictionary(x => x.Key, x => x.Value);
        }

        public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
        {
            return dictionary.GetOrCreate(key, _ => default!);
        }

        public static TValue GetOrAddDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
        {
            return dictionary.GetOrAdd(key, _ => default!);
        }

        public static TValue GetOrNew<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull where TValue : class, new()
        {
            return dictionary.GetOrCreate(key, _ => new TValue());
        }

        public static TValue GetOrAddNew<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull where TValue : class, new()
        {
            return dictionary.GetOrAdd(key, _ => new TValue());
        }

        public static TValue GetOrCreate<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> creator) where TKey : notnull
        {
            if (!dictionary.TryGetValue(key, out var result))
            {
                result = creator(key);
            }

            return result;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue fallback) where TKey : notnull
        {
            if (!dictionary.TryGetValue(key, out var result))
            {
                result = fallback;

                dictionary.Add(key, result);
            }

            return result;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> creator) where TKey : notnull
        {
            if (!dictionary.TryGetValue(key, out var result))
            {
                result = creator(key);

                dictionary.Add(key, result);
            }

            return result;
        }

        public static TValue GetOrAdd<TKey, TContext, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TContext context, Func<TKey, TContext, TValue> creator) where TKey : notnull
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
            collection.Foreach((x, i) => action(x));
        }

        public static void Foreach<T>(this IEnumerable<T> collection, Action<T, int> action)
        {
            var index = 0;

            foreach (var item in collection)
            {
                action(item, index);

                index++;
            }
        }

        public sealed class KeyValuePairComparer<TKey, TValue> : IEqualityComparer<KeyValuePair<TKey, TValue>> where TKey : notnull
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
                return keyComparer.GetHashCode(obj.Key) ^ ValueHashCode(obj);
            }

            private int ValueHashCode(KeyValuePair<TKey, TValue> obj)
            {
                if (Equals(obj.Value, null))
                {
                    return 0;
                }

                return valueComparer.GetHashCode(obj.Value);
            }
        }
    }
}
