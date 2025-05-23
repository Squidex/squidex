﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure;

public class CollectionExtensionsTests
{
    private readonly Dictionary<int, int> valueDictionary = [];
    private readonly Dictionary<int, List<int>> listDictionary = [];

    [Fact]
    public void SetEquals_should_return_false_if_subset()
    {
        var set1 = new[] { 1, 2 };
        var set2 = new[] { 1, 2, 3 };

        Assert.False(set1.SetEquals(set2));
        Assert.False(set2.SetEquals(set1));
    }

    [Fact]
    public void SetEquals_should_return_true_for_same_items_in_different_order()
    {
        var set1 = new[] { 1, 2, 3 };
        var set2 = new[] { 3, 2, 1 };

        Assert.True(set1.SetEquals(set2));
        Assert.True(set2.SetEquals(set1));
    }

    [Fact]
    public void IndexOf_should_return_index_if_found()
    {
        var source = new List<(int Value, int Other)>
        {
            (5, 5),
            (4, 4),
        };

        var index = source.IndexOf(x => x.Other == 4);

        Assert.Equal(1, index);
    }

    [Fact]
    public void IndexOf_should_return_negative_value_if_not_found()
    {
        var source = new List<(int Value, int Other)>
        {
            (5, 5),
            (4, 4),
        };

        var index = source.IndexOf(x => x.Other == 2);

        Assert.Equal(-1, index);
    }

    [Fact]
    public void GetOrAddDefault_should_return_value_if_key_exists()
    {
        valueDictionary[12] = 34;

        Assert.Equal(34, valueDictionary.GetOrAddDefault(12));
    }

    [Fact]
    public void GetOrAddDefault_should_return_default_and_add_it_if_key_not_exists()
    {
        Assert.Equal(0, valueDictionary.GetOrAddDefault(12));
        Assert.Equal(0, valueDictionary[12]);
    }

    [Fact]
    public void GetOrCreate_should_return_value_if_key_exists()
    {
        valueDictionary[12] = 34;

        Assert.Equal(34, valueDictionary.GetOrCreate(12, x => 34));
    }

    [Fact]
    public void GetOrCreate_should_return_default_but_not_add_it_if_key_not_exists()
    {
        Assert.Equal(24, valueDictionary.GetOrCreate(12, x => 24));
        Assert.False(valueDictionary.ContainsKey(12));
    }

    [Fact]
    public void GetOrAdd_should_return_value_if_key_exists()
    {
        valueDictionary[12] = 34;

        Assert.Equal(34, valueDictionary.GetOrAdd(12, x => 44));
    }

    [Fact]
    public void GetOrAdd_should_return_default_and_add_it_if_key_not_exists()
    {
        Assert.Equal(24, valueDictionary.GetOrAdd(12, 24));
        Assert.Equal(24, valueDictionary[12]);
    }

    [Fact]
    public void GetOrAdd_should_return_default_and_add_it_with_fallback_if_key_not_exists()
    {
        Assert.Equal(24, valueDictionary.GetOrAdd(12, x => 24));
        Assert.Equal(24, valueDictionary[12]);
    }

    [Fact]
    public void GetOrAddNew_should_return_value_if_key_exists()
    {
        var list = new List<int>();
        listDictionary[12] = list;

        Assert.Equal(list, listDictionary.GetOrAddNew(12));
    }

    [Fact]
    public void GetOrAddNew_should_return_default_but_not_add_it_if_key_not_exists()
    {
        var list = new List<int>();

        Assert.Equal(list, listDictionary.GetOrAddNew(12));
        Assert.Equal(list, listDictionary[12]);
    }

    [Fact]
    public void SequentialHashCode_should_ignore_null_values()
    {
        var collection1 = new string?[] { null, null };
        var collection2 = new string?[] { null };

        Assert.Equal(collection2.SequentialHashCode(), collection1.SequentialHashCode());
    }

    [Fact]
    public void SequentialHashCode_should_return_same_hash_codes_for_list_with_same_order()
    {
        var collection1 = new[] { 3, 5, 6 };
        var collection2 = new[] { 3, 5, 6 };

        Assert.Equal(collection2.SequentialHashCode(), collection1.SequentialHashCode());
    }

    [Fact]
    public void SequentialHashCode_should_return_different_hash_codes_for_list_with_different_items()
    {
        var collection1 = new[] { 3, 5, 6 };
        var collection2 = new[] { 3, 4, 1 };

        Assert.NotEqual(collection2.SequentialHashCode(), collection1.SequentialHashCode());
    }

    [Fact]
    public void SequentialHashCode_should_return_different_hash_codes_for_list_with_different_order()
    {
        var collection1 = new[] { 3, 5, 6 };
        var collection2 = new[] { 6, 5, 3 };

        Assert.NotEqual(collection2.SequentialHashCode(), collection1.SequentialHashCode());
    }

    [Fact]
    public void EqualsDictionary_should_return_true_for_equal_dictionaries()
    {
        var lhs = new Dictionary<int, int>
        {
            [1] = 1,
            [2] = 2,
        };
        var rhs = new Dictionary<int, int>
        {
            [1] = 1,
            [2] = 2,
        };

        Assert.True(lhs.EqualsDictionary(rhs));
    }

    [Fact]
    public void EqualsDictionary_should_return_false_for_different_sizes()
    {
        var lhs = new Dictionary<int, int>
        {
            [1] = 1,
            [2] = 2,
        };
        var rhs = new Dictionary<int, int>
        {
            [1] = 1,
        };

        Assert.False(lhs.EqualsDictionary(rhs));
    }

    [Fact]
    public void EqualsDictionary_should_return_false_for_different_values()
    {
        var lhs = new Dictionary<int, int>
        {
            [1] = 1,
            [2] = 2,
        };
        var rhs = new Dictionary<int, int>
        {
            [1] = 1,
            [3] = 3,
        };

        Assert.False(lhs.EqualsDictionary(rhs));
    }

    [Fact]
    public void Dictionary_should_return_same_hashcode_for_equal_dictionaries()
    {
        var lhs = new Dictionary<int, int>
        {
            [1] = 1,
            [2] = 2,
        };
        var rhs = new Dictionary<int, int>
        {
            [1] = 1,
            [2] = 2,
        };

        Assert.Equal(lhs.DictionaryHashCode(), rhs.DictionaryHashCode());
    }

    [Fact]
    public void Dictionary_should_return_different_hashcode_for_different_dictionaries()
    {
        var lhs = new Dictionary<int, int>
        {
            [1] = 1,
            [2] = 2,
        };
        var rhs = new Dictionary<int, int>
        {
            [1] = 1,
            [3] = 3,
        };

        Assert.NotEqual(lhs.DictionaryHashCode(), rhs.DictionaryHashCode());
    }

    [Fact]
    public void EqualsList_should_return_true_for_equal_lists()
    {
        var lhs = new List<int>
        {
            1,
            2,
        };
        var rhs = new List<int>
        {
            1,
            2,
        };

        Assert.True(lhs.EqualsList(rhs));
    }

    [Fact]
    public void EqualsList_should_return_false_for_different_sizes()
    {
        var lhs = new List<int>
        {
            1,
            2,
        };
        var rhs = new List<int>
        {
            1,
        };

        Assert.False(lhs.EqualsList(rhs));
    }

    [Fact]
    public void EqualsList_should_return_false_for_different_values()
    {
        var lhs = new List<int>
        {
            1,
            2,
        };
        var rhs = new List<int>
        {
            1,
            3,
        };

        Assert.False(lhs.EqualsList(rhs));
    }

    [Fact]
    public void EqualsList_should_return_false_for_different_order()
    {
        var lhs = new List<int>
        {
            1,
            2,
        };
        var rhs = new List<int>
        {
            2,
            1,
        };

        Assert.False(lhs.EqualsList(rhs));
    }

    [Fact]
    public void Foreach_should_call_action_foreach_item_with_index()
    {
        var source = new List<int> { 3, 5, 1 };

        var targetItems = new List<int>();
        var targetIndexes = new List<int>();

        source.Foreach((x, i) =>
        {
            targetItems.Add(x);
            targetIndexes.Add(i);
        });

        Assert.Equal(source, targetItems);
    }

    [Fact]
    public void Should_batch()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var actual = source.Batch(2).ToArray();

        actual.Should().BeEquivalentTo(
            new List<int>[]
            {
                [1, 2],
                [3, 4],
                [5],
            });
    }

    [Fact]
    public async Task Should_batch_async()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var actual = await source.ToAsyncEnumerable().Batch(2).ToArrayAsync();

        actual.Should().BeEquivalentTo(
            new List<int>[]
            {
                [1, 2],
                [3, 4],
                [5],
            });
    }
}
