// ==========================================================================
//  PartitioningTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Xunit;

namespace Squidex.Domain.Apps.Core
{
    public sealed class PartitioningTests
    {
        [Fact]
        public void Should_provide_invariant_instance()
        {
            Assert.Equal("invariant", Partitioning.Invariant.Key);
            Assert.Equal("invariant", Partitioning.Invariant.ToString());
        }

        [Fact]
        public void Should_provide_language_instance()
        {
            Assert.Equal("language", Partitioning.Language.Key);
            Assert.Equal("language", Partitioning.Language.ToString());
        }

        [Fact]
        public void Should_make_correct_equal_comparisons()
        {
            var partitioning1a = new Partitioning("partitioning1");
            var partitioning1b = new Partitioning("partitioning1");
            var partitioning2a = new Partitioning("partitioning2");

            Assert.True(partitioning1a.Equals(partitioning1b));

            Assert.False(partitioning1a.Equals(partitioning2a));
        }

        [Fact]
        public void Should_make_correct_object_equal_comparisons()
        {
            object partitioning1a = new Partitioning("partitioning1");
            object partitioning1b = new Partitioning("partitioning1");
            object partitioning2a = new Partitioning("partitioning2");

            Assert.True(partitioning1a.Equals(partitioning1b));

            Assert.False(partitioning1a.Equals(partitioning2a));
        }

        [Fact]
        public void Should_provide_correct_hash_codes()
        {
            var partitioning1a = new Partitioning("partitioning1");
            var partitioning1b = new Partitioning("partitioning1");
            var partitioning2a = new Partitioning("partitioning2");

            Assert.Equal(partitioning1a.GetHashCode(), partitioning1b.GetHashCode());

            Assert.NotEqual(partitioning1a.GetHashCode(), partitioning2a.GetHashCode());
        }
    }
}
