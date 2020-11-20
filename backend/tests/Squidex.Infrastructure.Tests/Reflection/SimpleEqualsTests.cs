// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection.Equality;
using Xunit;

#pragma warning disable CA1822 // Mark members as static

namespace Squidex.Infrastructure.Reflection
{
    public class SimpleEqualsTests
    {
        public class Class : ClassBase
        {
            public int Scalar { get; set; }

            public int ReadOnly
            {
                set { Debug.WriteLine(value); }
            }
        }

        public class CustomEquals : IEquatable<CustomEquals>
        {
            private readonly int value;

            public CustomEquals(int value)
            {
                this.value = value;
            }

            public override bool Equals(object? obj)
            {
                return Equals(obj as CustomEquals);
            }

            public bool Equals(CustomEquals? other)
            {
                return other != null && other.value == value;
            }

            public override int GetHashCode()
            {
                return value;
            }
        }

        public class ClassBase
        {
            [IgnoreEquals]
            public int Ignored { get; set; }

            public List<int> Complex { get; set; }
        }

        public static IEnumerable<object[]> RandomValues()
        {
            yield return new object[]
            {
                Guid.NewGuid(),
                Guid.NewGuid()
            };

            yield return new object[]
            {
                12,
                22
            };

            yield return new object[]
            {
                DateTime.UtcNow,
                DateTime.UtcNow.AddSeconds(2)
            };

            yield return new object[]
            {
                TimeSpan.FromMilliseconds(123),
                TimeSpan.FromMilliseconds(55)
            };

            yield return new object[]
            {
                new Uri("/url1", UriKind.Relative),
                new Uri("/url2", UriKind.Relative)
            };
        }

        [Theory]
        [MemberData(nameof(RandomValues))]
        public void Should_compare_values(object lhs, object rhs)
        {
            Assert.True(SimpleEquals.IsEquals(lhs, lhs));
            Assert.True(DeepEqualityComparer<object>.Default.Equals(lhs, lhs));

            Assert.False(SimpleEquals.IsEquals(lhs, rhs));
            Assert.True(DeepEqualityComparer<object>.Default.Equals(lhs, rhs));
        }

        [Fact]
        public void Should_compare_equal_customs()
        {
            var customA_1 = new CustomEquals(1);
            var customA_2 = new CustomEquals(1);

            Assert.True(SimpleEquals.IsEquals(customA_1, customA_1));
            Assert.True(SimpleEquals.IsEquals(customA_1, customA_2));
        }

        [Fact]
        public void Should_compare_non_equal_customs()
        {
            var customA_1 = new CustomEquals(1);
            var customB_1 = new CustomEquals(2);

            Assert.False(SimpleEquals.IsEquals(customA_1, customB_1));
            Assert.False(SimpleEquals.IsEquals(customA_1, null!));
        }

        [Fact]
        public void Should_compare_equal_strings()
        {
            var stringA_1 = "a";
            var stringA_2 = new string(new[] { 'a' });

            Assert.True(SimpleEquals.IsEquals(stringA_1, stringA_1));
            Assert.True(SimpleEquals.IsEquals(stringA_1, stringA_2));
        }

        [Fact]
        public void Should_compare_non_equal_strings()
        {
            var stringA_1 = "a";
            var stringB_2 = new string(new[] { 'b' });

            Assert.False(SimpleEquals.IsEquals(stringA_1, stringB_2));
            Assert.False(SimpleEquals.IsEquals(stringA_1, null!));
        }

        [Fact]
        public void Should_compare_equal_lists()
        {
            var listA_1 = new List<string> { "a" };
            var listA_2 = new List<string> { "a" };

            Assert.True(SimpleEquals.IsEquals(listA_1, listA_1));
            Assert.True(SimpleEquals.IsEquals(listA_1, listA_2));
        }

        [Fact]
        public void Should_compare_non_equal_lists()
        {
            var listA_1 = new List<string> { "a" };
            var listB_1 = new List<string> { "b" };
            var listC_1 = new List<string> { "b", "c" };

            Assert.False(SimpleEquals.IsEquals(listA_1, listB_1));
            Assert.False(SimpleEquals.IsEquals(listA_1, listC_1));
            Assert.False(SimpleEquals.IsEquals(listA_1, null!));
        }

        [Fact]
        public void Should_compare_equal_sets()
        {
            var setA_1 = new HashSet<string> { "a", "b" };
            var setA_2 = new HashSet<string> { "b", "a" };

            Assert.True(SimpleEquals.IsEquals(setA_1, setA_1));
            Assert.True(SimpleEquals.IsEquals(setA_1, setA_2));
        }

        [Fact]
        public void Should_compare_non_equal_sets()
        {
            var setA_1 = new HashSet<string> { "a" };
            var setB_1 = new HashSet<string> { "b" };

            Assert.False(SimpleEquals.IsEquals(setA_1, setB_1));
            Assert.False(SimpleEquals.IsEquals(setA_1, null!));
        }

        [Fact]
        public void Should_compare_equal_collections()
        {
            var listA_1 = ReadOnlyCollection.Create("a");
            var listA_2 = ReadOnlyCollection.Create("a");

            Assert.True(SimpleEquals.IsEquals(listA_1, listA_1));
            Assert.True(SimpleEquals.IsEquals(listA_1, listA_2));
        }

        [Fact]
        public void Should_compare_non_equal_collections()
        {
            var listA_1 = ReadOnlyCollection.Create("a");
            var listB_1 = ReadOnlyCollection.Create("b");
            var listC_1 = ReadOnlyCollection.Create("b");

            Assert.False(SimpleEquals.IsEquals(listA_1, listB_1));
            Assert.False(SimpleEquals.IsEquals(listA_1, listC_1));
            Assert.False(SimpleEquals.IsEquals(listA_1, null!));
        }

        [Fact]
        public void Should_compare_equal_dictionaries()
        {
            var dictionaryA_1 = new Dictionary<string, int> { ["key1"] = 123 };
            var dictionaryA_2 = new Dictionary<string, int> { ["key1"] = 123 };

            Assert.True(SimpleEquals.IsEquals(dictionaryA_1, dictionaryA_1));
            Assert.True(SimpleEquals.IsEquals(dictionaryA_1, dictionaryA_2));
        }

        [Fact]
        public void Should_compare_non_equal_dictionaries()
        {
            var listA_1 = new Dictionary<string, int> { ["key1"] = 123 };
            var listB_1 = new Dictionary<string, int> { ["key2"] = 123 };
            var listC_1 = new Dictionary<string, int> { ["key1"] = 555 };
            var listD_1 = new Dictionary<string, int> { ["key1"] = 123, ["key2"] = 55 };

            Assert.False(SimpleEquals.IsEquals(listA_1, listB_1));
            Assert.False(SimpleEquals.IsEquals(listA_1, listC_1));
            Assert.False(SimpleEquals.IsEquals(listA_1, listD_1));
            Assert.False(SimpleEquals.IsEquals(listA_1, null!));
        }

        [Fact]
        public void Should_compare_equal_objects()
        {
            var objectA_1 = new Class { Scalar = 1, Complex = new List<int> { 1, 4 } };
            var objectA_2 = new Class { Scalar = 1, Complex = new List<int> { 1, 4 } };

            Assert.True(SimpleEquals.IsEquals(objectA_1, objectA_1));
            Assert.True(SimpleEquals.IsEquals(objectA_1, objectA_2));
        }

        [Fact]
        public void Should_compare_equal_objects_with_ignored_properties()
        {
            var objectA_1 = new Class { Ignored = 1 };
            var objectA_2 = new Class { Ignored = 2 };

            Assert.True(SimpleEquals.IsEquals(objectA_1, objectA_1));
            Assert.True(SimpleEquals.IsEquals(objectA_1, objectA_2));
        }

        [Fact]
        public void Should_compare_non_equal_objects()
        {
            var objectA_1 = new Class { Scalar = 1, Complex = new List<int> { 1, 4 } };
            var objectB_1 = new Class { Scalar = 1, Complex = new List<int> { 1, 2 } };
            var objectC_1 = new Class { Scalar = 1, Complex = new List<int> { 1, 2 } };
            var objectD_1 = new Class { Scalar = 2, Complex = null! };

            Assert.False(SimpleEquals.IsEquals(objectA_1, objectB_1));
            Assert.False(SimpleEquals.IsEquals(objectA_1, objectC_1));
            Assert.False(SimpleEquals.IsEquals(objectA_1, objectD_1));
            Assert.False(SimpleEquals.IsEquals(objectA_1, null!));
        }
    }
}
