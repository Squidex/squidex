// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Squidex.Infrastructure;

public static class CollectionExtensions
{
    public static async Task<List<TResult>> SelectAsync<T, TResult>(this IEnumerable<T> source, Func<T, Task<TResult>> selector)
    {
        var initialCapacity = source is IReadOnlyCollection<T> collection ? collection.Count : 1;

        var result = new List<TResult>(initialCapacity);

        foreach (var item in source)
        {
            result.Add(await selector(item));
        }

        return result;
    }

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

    public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> source, int size)
    {
        List<T>? bucket = null;

        foreach (var item in source)
        {
            bucket ??= new List<T>(size);
            bucket.Add(item);

            if (bucket.Count == size)
            {
                yield return bucket;
                bucket = null;
            }
        }

        if (bucket?.Count > 0)
        {
            yield return bucket;
        }
    }

    public static async IAsyncEnumerable<List<T>> Batch<T>(this IAsyncEnumerable<T> source, int size,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        List<T>? bucket = null;

        await foreach (var item in source.WithCancellation(ct))
        {
            bucket ??= new List<T>(size);
            bucket.Add(item);

            if (bucket.Count == size)
            {
                yield return bucket;
                bucket = null;
            }
        }

        if (bucket?.Count > 0)
        {
            yield return bucket;
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
        return enumerable.OrderBy(x => Random.Shared.Next()).ToList();
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
        return EqualsDictionary(dictionary, other, EqualityComparer<TValue>.Default);
    }

    public static bool EqualsDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, IReadOnlyDictionary<TKey, TValue>? other, IEqualityComparer<TValue> valueComparer) where TKey : notnull
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

        foreach (var (key, value) in dictionary)
        {
            if (!other.TryGetValue(key, out var otherValue) || !valueComparer.Equals(value, otherValue))
            {
                return false;
            }
        }

        return true;
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

    public static TValue GetOrAddDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
    {
        return dictionary.GetOrAdd(key, _ => default!);
    }

    public static TValue GetOrAddNew<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull where TValue : class, new()
    {
        return dictionary.GetOrAdd(key, _ => new TValue());
    }

    public static TValue GetOrCreate<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory) where TKey : notnull
    {
        if (!dictionary.TryGetValue(key, out var result))
        {
            result = valueFactory(key);
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

    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory) where TKey : notnull
    {
        if (!dictionary.TryGetValue(key, out var result))
        {
            result = valueFactory(key);

            dictionary.Add(key, result);
        }

        return result;
    }

    public static TValue GetOrAdd<TKey, TArg, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument) where TKey : notnull
    {
        if (!dictionary.TryGetValue(key, out var result))
        {
            result = valueFactory(key, factoryArgument);

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

    public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> source, long take)
    {
        var sourceList = new LinkedList<T>(source);

        static LinkedListNode<T> TakeNode(LinkedList<T> source, int index)
        {
            var actual = source.First!;

            for (var i = 0; i < index; i++)
            {
                var next = actual?.Next;

                if (next == null)
                {
                    return actual!;
                }

                actual = next;
            }

            return actual;
        }

        for (var i = 0; i < take; i++)
        {
            if (sourceList.Count == 0)
            {
                break;
            }

            var takenIndex = Random.Shared.Next(sourceList.Count);
            var takenElement = TakeNode(sourceList, takenIndex);

            sourceList.Remove(takenElement);

            yield return takenElement.Value;
        }
    }

    public static IAsyncEnumerable<TSource> Catch<TSource>(this IAsyncEnumerable<TSource> source, Func<Exception, IEnumerable<TSource>> handler)
    {
        return Core(source, handler);

        static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source, Func<Exception, IEnumerable<TSource>> handler,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var error = default(IEnumerable<TSource>);

            await using (var e = source.GetAsyncEnumerator(ct))
            {
                while (true)
                {
                    TSource c;

                    try
                    {
                        if (!await e.MoveNextAsync(ct))
                        {
                            break;
                        }

                        c = e.Current;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        error = handler(ex);
                        break;
                    }

                    yield return c;
                }
            }

            if (error != null)
            {
                foreach (var item in error)
                {
                    yield return item;
                }
            }
        }
    }

    public static async Task<IReadOnlyCollection<TResult>> SelectManyAsync<T, TResult>(this IEnumerable<T> source, Func<T, int, CancellationToken, Task<IEnumerable<TResult>>> selector,
        CancellationToken ct = default)
    {
        var result = new ConcurrentBag<TResult>();

        var sourceWithIndex = source.Select((x, i) => (Item: x, Index: i));

        await Parallel.ForEachAsync(sourceWithIndex,
            ct,
            async (item, ct) =>
            {
                var createdItems = await selector(item.Item, item.Index, ct);

                foreach (var created in createdItems)
                {
                    result.Add(created);
                }
            });

        return result;
    }
}
