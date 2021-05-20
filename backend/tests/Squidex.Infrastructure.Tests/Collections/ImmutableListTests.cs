// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Collections
{
    public class ImmutableListTests
    {
        [Fact]
        public void Should_return_empty_instance_for_empty_array()
        {
            var result = ImmutableList.Create<int>();

            Assert.Same(ImmutableList.Empty<int>(), result);
        }

        [Fact]
        public void Should_return_empty_instance_for_null_array()
        {
            var result = ImmutableList.Create((int[]?)null);

            Assert.Same(ImmutableList.Empty<int>(), result);
        }

        [Fact]
        public void Should_return_empty_instance_for_empty_enumerable()
        {
            var result = Enumerable.Empty<int>().ToImmutableList();

            Assert.Same(ImmutableList.Empty<int>(), result);
        }

        [Fact]
        public void Should_make_correct_equal_comparisons()
        {
            var list1a = ImmutableList.Create(1);
            var list1b = ImmutableList.Create(1);

            var listOtherValue = ImmutableList.Create(2);
            var listOtherSize = ImmutableList.Create(1, 2);

            Assert.Equal(list1a, list1b);
            Assert.Equal(list1a.GetHashCode(), list1b.GetHashCode());
            Assert.True(list1a.Equals((object)list1b));

            Assert.NotEqual(list1a, listOtherValue);
            Assert.NotEqual(list1a.GetHashCode(), listOtherValue.GetHashCode());
            Assert.False(list1a.Equals((object)listOtherValue));

            Assert.NotEqual(list1a, listOtherSize);
            Assert.NotEqual(list1a.GetHashCode(), listOtherSize.GetHashCode());
            Assert.False(list1a.Equals((object)listOtherSize));
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var sut = ImmutableList.Create(1, 2, 3);

            var serialized = sut.SerializeAndDeserialize();

            Assert.Equal(sut, serialized);
        }
    }
}
