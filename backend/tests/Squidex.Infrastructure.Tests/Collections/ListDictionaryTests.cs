// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using Squidex.Infrastructure.TestHelpers;

#pragma warning disable xUnit2017 // Do not use Contains() to check if a value exists in a collection
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable CA1841 // Prefer Dictionary.Contains methods

namespace Squidex.Infrastructure.Collections;

public class ListDictionaryTests
{
    [Fact]
    public void Should_create_empty()
    {
        var sut = new ListDictionary<int, int>();

        Assert.Empty(sut);
        Assert.Equal(1, sut.Capacity);
    }

    [Fact]
    public void Should_create_with_capacity()
    {
        var sut = new ListDictionary<int, int>(20);

        Assert.Empty(sut);
        Assert.Equal(20, sut.Capacity);
    }

    [Fact]
    public void Should_create_as_copy()
    {
        var source = new ListDictionary<int, int>();

        source.Add(1, 10);
        source.Add(2, 20);

        var sut = new ListDictionary<int, int>(source);

        Assert.Equal(2, sut.Count);
    }

    [Fact]
    public void Should_not_be_readonly()
    {
        var sut = new ListDictionary<int, int>();

        Assert.False(sut.IsReadOnly);
        Assert.False(sut.Keys.IsReadOnly);
        Assert.False(sut.Values.IsReadOnly);
    }

    [Fact]
    public void Should_add_item()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);

        Assert.Single(sut);
        Assert.Equal(10, sut[1]);
    }

    [Fact]
    public void Should_add_item_unsafe()
    {
        var sut = new ListDictionary<int, int>();

        sut.AddUnsafe(1, 10);

        Assert.Single(sut);
        Assert.Equal(10, sut[1]);
    }

    [Fact]
    public void Should_add_item_as_pair()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(new KeyValuePair<int, int>(1, 10));

        Assert.Single(sut);
        Assert.Equal(10, sut[1]);
    }

    [Fact]
    public void Should_throw_exception_if_adding_existing_key()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);

        Assert.Throws<ArgumentException>(() => sut.Add(1, 20));
    }

    [Fact]
    public void Should_throw_exception_if_adding_pair_with_existing_key()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);

        Assert.Throws<ArgumentException>(() => sut.Add(new KeyValuePair<int, int>(1, 20)));
    }

    [Fact]
    public void Should_set_item()
    {
        var sut = new ListDictionary<int, int>();

        sut[1] = 10;

        Assert.Single(sut);
        Assert.Equal(10, sut[1]);
    }

    [Fact]
    public void Should_override_item()
    {
        var sut = new ListDictionary<int, int>();

        sut[1] = 20;

        Assert.Single(sut);
        Assert.Equal(20, sut[1]);
    }

    [Fact]
    public void Should_return_true_when_dictionary_contains_value()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);

        Assert.True(sut.Contains(new KeyValuePair<int, int>(1, 10)));
        Assert.True(sut.ContainsKey(1));
        Assert.True(sut.Keys.Contains(1));
        Assert.True(sut.Values.Contains(10));
    }

    [Fact]
    public void Should_return_false_when_dictionary_does_not_contains_value()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);

        Assert.False(sut.Contains(new KeyValuePair<int, int>(1, 20)));
        Assert.False(sut.ContainsKey(2));
        Assert.False(sut.Keys.Contains(2));
        Assert.False(sut.Values.Contains(20));
    }

    [Fact]
    public void Should_get_count()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);
        sut.Add(3, 30);

        Assert.Equal(3, sut.Count);
        Assert.Equal(3, sut.Keys.Count);
        Assert.Equal(3, sut.Values.Count);
    }

    [Fact]
    public void Should_clear()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);
        sut.Add(3, 30);
        sut.Clear();

        Assert.Empty(sut);
    }

    [Fact]
    public void Should_remove_key()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);
        sut.Add(3, 30);

        Assert.True(sut.Remove(2));
        Assert.False(sut.ContainsKey(2));
    }

    [Fact]
    public void Should_not_remove_key_if_not_found()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);
        sut.Add(3, 30);

        Assert.False(sut.Remove(4));
    }

    [Fact]
    public void Should_remove_item()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);
        sut.Add(3, 30);

        Assert.True(sut.Remove(new KeyValuePair<int, int>(2, 20)));
        Assert.False(sut.ContainsKey(2));
    }

    [Fact]
    public void Should_not_remove_item_if_key_not_found()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);
        sut.Add(3, 30);

        Assert.False(sut.Remove(new KeyValuePair<int, int>(4, 40)));
    }

    [Fact]
    public void Should_not_remove_item_if_value_not_equal()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);
        sut.Add(3, 30);

        Assert.False(sut.Remove(new KeyValuePair<int, int>(2, 40)));
    }

    [Fact]
    public void Should_get_value_by_method_if_found()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);

        Assert.True(sut.TryGetValue(2, out var found));
        Assert.Equal(20, found);
    }

    [Fact]
    public void Should_not_get_value_by_method_if_not_found()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(3, 30);

        Assert.False(sut.TryGetValue(4, out var found));
        Assert.Equal(0, found);
    }

    [Fact]
    public void Should_get_value_by_indexer_if_found()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);

        Assert.Equal(20, sut[2]);
    }

    [Fact]
    public void Should_not_get_value_by_indexer_if_not_found()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);

        Assert.Throws<KeyNotFoundException>(() => sut[4]);
    }

    [Fact]
    public void Should_loop_over_entries()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);

        var actual = new List<KeyValuePair<int, int>>();

        foreach (var entry in sut)
        {
            actual.Add(entry);
        }

        Assert.Equal(new[]
        {
            new KeyValuePair<int, int>(1, 10),
            new KeyValuePair<int, int>(2, 20)
        }, actual.ToArray());
    }

    [Fact]
    public void Should_loop_over_entries_with_old_enumerator()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);

        var actual = new List<KeyValuePair<int, int>>();

        foreach (KeyValuePair<int, int> entry in (IEnumerable)sut)
        {
            actual.Add(entry);
        }

        Assert.Equal(new[]
        {
            new KeyValuePair<int, int>(1, 10),
            new KeyValuePair<int, int>(2, 20)
        }, actual.ToArray());
    }

    [Fact]
    public void Should_copy_entries_to_array()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);

        Assert.Equal(new[]
        {
            new KeyValuePair<int, int>(1, 10),
            new KeyValuePair<int, int>(2, 20)
        }, sut.ToArray());
    }

    [Fact]
    public void Should_loop_over_keys()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);

        var actual = new List<int>();

        foreach (var entry in sut.Keys)
        {
            actual.Add(entry);
        }

        Assert.Equal(new[] { 1, 2 }, actual.ToArray());
    }

    [Fact]
    public void Should_loop_over_keys_with_old_enumerator()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);

        var actual = new List<int>();

        foreach (int entry in (IEnumerable)sut.Keys)
        {
            actual.Add(entry);
        }

        Assert.Equal(new[] { 1, 2 }, actual.ToArray());
    }

    [Fact]
    public void Should_copy_keys_to_array()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);

        Assert.Equal(new[] { 1, 2 }, sut.Keys.ToArray());
    }

    [Fact]
    public void Should_loop_over_values()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);

        var actual = new List<int>();

        foreach (var entry in sut.Values)
        {
            actual.Add(entry);
        }

        Assert.Equal(new[] { 10, 20 }, actual.ToArray());
    }

    [Fact]
    public void Should_loop_over_values_with_old_enumerator()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);

        var actual = new List<int>();

        foreach (int entry in (IEnumerable)sut.Values)
        {
            actual.Add(entry);
        }

        Assert.Equal(new[] { 10, 20 }, actual.ToArray());
    }

    [Fact]
    public void Should_copy_values_to_array()
    {
        var sut = new ListDictionary<int, int>();

        sut.Add(1, 10);
        sut.Add(2, 20);

        Assert.Equal(new[] { 10, 20 }, sut.Values.ToArray());
    }

    [Fact]
    public void Should_trim()
    {
        var sut = new ListDictionary<int, int>(20);

        sut.Add(1, 10);
        sut.Add(2, 20);

        Assert.Equal(20, sut.Capacity);

        sut.TrimExcess();

        Assert.Equal(2, sut.Capacity);
    }

    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var sut = new Dictionary<int, int>
        {
            [11] = 1,
            [12] = 2,
            [13] = 3
        }.ToReadonlyDictionary();

        var serialized = sut.SerializeAndDeserialize();

        Assert.Equal(sut, serialized);
    }
}
