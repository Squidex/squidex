// ==========================================================================
//  RetryWindowTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Xunit;

namespace Squidex.Infrastructure
{
    public class RetryWindowTests
    {
        private const int WindowSize = 5;

        [Fact]
        public void Should_allow_to_retry_after_reset()
        {
            var sut = new RetryWindow(TimeSpan.FromSeconds(1), WindowSize);

            for (var i = 0; i < WindowSize * 2; i++)
            {
                sut.CanRetryAfterFailure();
            }

            sut.Reset();

            Assert.True(sut.CanRetryAfterFailure());
        }

        [Theory]
        [InlineData(6)]
        [InlineData(7)]
        public void Should_not_allow_to_retry_after_many_errors(int errors)
        {
            var sut = new RetryWindow(TimeSpan.FromSeconds(1), WindowSize);
            var now = DateTime.UtcNow;

            for (var i = 0; i < WindowSize; i++)
            {
                Assert.True(sut.CanRetryAfterFailure(now));
            }

            var remaining = errors - WindowSize;

            for (var i = 0; i < remaining; i++)
            {
                Assert.False(sut.CanRetryAfterFailure(now));
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void Should_allow_to_retry_after_few_errors(int errors)
        {
            var sut = new RetryWindow(TimeSpan.FromSeconds(1), WindowSize);
            var now = DateTime.UtcNow;

            for (var i = 0; i < errors; i++)
            {
                Assert.True(sut.CanRetryAfterFailure(now));
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void Should_allow_to_retry_after_few_errors_in_window(int errors)
        {
            var sut = new RetryWindow(TimeSpan.FromSeconds(1), WindowSize);
            var now = DateTime.UtcNow;

            for (var i = 0; i < errors; i++)
            {
                Assert.True(sut.CanRetryAfterFailure(now.AddMilliseconds(i * 300)));
            }
        }
    }
}
