// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Domain.Apps.Core.Model
{
    public class PartitioningTests
    {
        [Fact]
        public void Should_consider_null_as_valid_partitioning()
        {
            string? partitioning = null;

            Assert.True(partitioning.IsValidPartitioning());
        }

        [Fact]
        public void Should_consider_invariant_as_valid_partitioning()
        {
            var partitioning = "invariant";

            Assert.True(partitioning.IsValidPartitioning());
        }

        [Fact]
        public void Should_consider_language_as_valid_partitioning()
        {
            var partitioning = "language";

            Assert.True(partitioning.IsValidPartitioning());
        }

        [Fact]
        public void Should_not_consider_empty_as_valid_partitioning()
        {
            var partitioning = string.Empty;

            Assert.False(partitioning.IsValidPartitioning());
        }

        [Fact]
        public void Should_not_consider_other_string_as_valid_partitioning()
        {
            var partitioning = "invalid";

            Assert.False(partitioning.IsValidPartitioning());
        }

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
            var partitioning1_a = new Partitioning("partitioning1");
            var partitioning1_b = new Partitioning("partitioning1");

            var partitioning2 = new Partitioning("partitioning2");

            Assert.Equal(partitioning1_a, partitioning1_b);
            Assert.Equal(partitioning1_a.GetHashCode(), partitioning1_b.GetHashCode());
            Assert.True(partitioning1_a.Equals((object)partitioning1_b));

            Assert.NotEqual(partitioning1_a, partitioning2);
            Assert.NotEqual(partitioning1_a.GetHashCode(), partitioning2.GetHashCode());
            Assert.False(partitioning1_a.Equals((object)partitioning2));
        }
    }
}
