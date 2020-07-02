// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure
{
    public class BytesRangeTests
    {
        [Fact]
        public void Should_create_default()
        {
            var sut = (BytesRange)default;

            TestBytesRange(sut, null, null, long.MaxValue, false, null);
        }

        [Fact]
        public void Should_create_default_manually()
        {
            var sut = new BytesRange(null, null);

            TestBytesRange(sut, null, null, long.MaxValue, false, null);
        }

        [Fact]
        public void Should_create_with_from()
        {
            var sut = new BytesRange(12, null);

            TestBytesRange(sut, 12, null, long.MaxValue - 11, true, "bytes=12-");
        }

        [Fact]
        public void Should_create_with_to()
        {
            var sut = new BytesRange(null, 12);

            TestBytesRange(sut, null, 12, 13, true, "bytes=-12");
        }

        [Fact]
        public void Should_create_with_from_and_to()
        {
            var sut = new BytesRange(3, 15);

            TestBytesRange(sut, 3, 15, 13, true, "bytes=3-15");
        }

        [Fact]
        public void Should_create_with_single_byte()
        {
            var sut = new BytesRange(3, 3);

            TestBytesRange(sut, 3, 3, 1, true, "bytes=3-3");
        }

        [Fact]
        public void Should_fix_length()
        {
            var sut = new BytesRange(5, 3);

            TestBytesRange(sut, 5, 3, 0, false, null);
        }

        [Fact]
        public void Should_fix_length_for_negative_range()
        {
            var sut = new BytesRange(-5, -3);

            TestBytesRange(sut, -5, -3, 0, false, null);
        }

        private static void TestBytesRange(BytesRange sut, long? from, long? to, long length, bool defined, string? formatted)
        {
            Assert.Equal(from, sut.From);
            Assert.Equal(to, sut.To);
            Assert.Equal(length, sut.Length);
            Assert.Equal(defined, sut.IsDefined);
            Assert.Equal(formatted, sut.ToString());
        }
    }
}
