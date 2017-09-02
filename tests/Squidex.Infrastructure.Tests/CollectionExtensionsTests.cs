// ==========================================================================
//  CollectionExtensionsTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Xunit;

namespace Squidex.Infrastructure
{
    public class CollectionExtensionTests
    {
        private readonly Dictionary<int, int> valueDictionary = new Dictionary<int, int>();
        private readonly Dictionary<int, List<int>> listDictionary = new Dictionary<int, List<int>>();

        [Fact]
        public void GetOrDefault_should_return_value_if_key_exists()
        {
            valueDictionary[12] = 34;

            Assert.Equal(34, valueDictionary.GetOrDefault(12));
        }

        [Fact]
        public void GetOrDefault_should_return_default_and_not_add_it_if_key_not_exists()
        {
            Assert.Equal(0, valueDictionary.GetOrDefault(12));
            Assert.False(valueDictionary.ContainsKey(12));
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

            Assert.Equal(34, valueDictionary.GetOrAdd(12, x => 34));
        }

        [Fact]
        public void GetOrAdd_should_return_default_and_add_it_if_key_not_exists()
        {
            Assert.Equal(24, valueDictionary.GetOrAdd(12, x => 24));
            Assert.Equal(24, valueDictionary[12]);
        }

        [Fact]
        public void GetOrNew_should_return_value_if_key_exists()
        {
            var list = new List<int>();
            listDictionary[12] = list;

            Assert.Equal(list, listDictionary.GetOrNew(12));
        }

        [Fact]
        public void GetOrNew_should_return_default_but_not_add_it_if_key_not_exists()
        {
            var list = new List<int>();

            Assert.Equal(list, listDictionary.GetOrNew(12));
            Assert.False(listDictionary.ContainsKey(12));
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
            var collection = new string[] { null, null };

            Assert.Equal(17, collection.SequentialHashCode());
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
        public void OrderedHashCode_should_return_same_hash_codes_for_list_with_same_order()
        {
            var collection1 = new[] { 3, 5, 6 };
            var collection2 = new[] { 3, 5, 6 };

            Assert.Equal(collection2.OrderedHashCode(), collection1.OrderedHashCode());
        }

        [Fact]
        public void OrderedHashCode_should_return_different_hash_codes_for_list_with_different_items()
        {
            var collection1 = new[] { 3, 5, 6 };
            var collection2 = new[] { 3, 4, 1 };

            Assert.NotEqual(collection2.OrderedHashCode(), collection1.OrderedHashCode());
        }

        [Fact]
        public void OrderedHashCode_should_return_same_hash_codes_for_list_with_different_order()
        {
            var collection1 = new[] { 3, 5, 6 };
            var collection2 = new[] { 6, 5, 3 };

            Assert.Equal(collection2.OrderedHashCode(), collection1.OrderedHashCode());
        }

        [Fact]
        public void EqualsDictionary_should_return_true_for_equal_dictionaries()
        {
            var lhs = new Dictionary<int, int>
            {
                [1] = 1,
                [2] = 2
            };
            var rhs = new Dictionary<int, int>
            {
                [1] = 1,
                [2] = 2
            };

            Assert.True(lhs.EqualsDictionary(rhs));
        }

        [Fact]
        public void EqualsDictionary_should_return_false_for_different_sizes()
        {
            var lhs = new Dictionary<int, int>
            {
                [1] = 1,
                [2] = 2
            };
            var rhs = new Dictionary<int, int>
            {
                [1] = 1
            };

            Assert.False(lhs.EqualsDictionary(rhs));
        }

        [Fact]
        public void EqualsDictionary_should_return_false_for_different_values()
        {
            var lhs = new Dictionary<int, int>
            {
                [1] = 1,
                [2] = 2
            };
            var rhs = new Dictionary<int, int>
            {
                [1] = 1,
                [3] = 3
            };

            Assert.False(lhs.EqualsDictionary(rhs));
        }

        [Fact]
        public void Dictionary_should_return_same_hashcode_for_equal_dictionaries()
        {
            var lhs = new Dictionary<int, int>
            {
                [1] = 1,
                [2] = 2
            };
            var rhs = new Dictionary<int, int>
            {
                [1] = 1,
                [2] = 2
            };

            Assert.Equal(lhs.DictionaryHashCode(), rhs.DictionaryHashCode());
        }

        [Fact]
        public void Dictionary_should_return_different_hashcode_for_different_dictionaries()
        {
            var lhs = new Dictionary<int, int>
            {
                [1] = 1,
                [2] = 2
            };
            var rhs = new Dictionary<int, int>
            {
                [1] = 1,
                [3] = 3
            };

            Assert.NotEqual(lhs.DictionaryHashCode(), rhs.DictionaryHashCode());
        }

        [Fact]
        public void Foreach_should_call_action_foreach_item()
        {
            var source = new List<int> { 3, 5, 1 };
            var target = new List<int>();

            source.Foreach(target.Add);

            Assert.Equal(source, target);
        }
    }
}