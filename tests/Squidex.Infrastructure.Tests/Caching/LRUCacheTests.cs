// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure.Caching
{
    public class LRUCacheTests
    {
        private readonly LRUCache sut = new LRUCache(10);
        private readonly string key = "Key";

        [Fact]
        public void Should_always_override_when_setting_value()
        {
            sut.Set(key, 1);
            sut.Set(key, 2);

            Assert.True(sut.TryGetValue(key, out var value));
            Assert.True(sut.Contains(key));

            Assert.Equal(2, value);
        }

        [Fact]
        public void Should_remove_old_items_when_capacity_reached()
        {
            for (int i = 0; i < 15; i++)
            {
                sut.Set(i.ToString(), i);
            }

            for (int i = 0; i < 5; i++)
            {
                Assert.False(sut.TryGetValue(i.ToString(), out var value));
                Assert.Null(value);
            }

            for (int i = 5; i < 15; i++)
            {
                Assert.True(sut.TryGetValue(i.ToString(), out var value));
                Assert.NotNull(value);
            }
        }

        [Fact]
        public void Should_return_false_when_item_to_remove_does_not_exist()
        {
            Assert.False(sut.Remove(key));
        }

        [Fact]
        public void Should_remove_inserted_item()
        {
            sut.Set(key, 2);

            Assert.True(sut.Remove(key));
            Assert.False(sut.Contains(key));
            Assert.False(sut.TryGetValue(key, out var value));
            Assert.Null(value);
        }
    }
}