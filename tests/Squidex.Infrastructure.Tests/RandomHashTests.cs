// ==========================================================================
//  RandomHashTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure
{
    public class RandomHashTests
    {
        [Fact]
        public void Should_create_long_hash()
        {
            var hash = RandomHash.New();

            Assert.Equal(44, hash.Length);
        }

        [Fact]
        public void Should_create_new_hashs()
        {
            var hash1 = RandomHash.New();
            var hash2 = RandomHash.New();

            Assert.NotEqual(hash1, hash2);
        }
    }
}
