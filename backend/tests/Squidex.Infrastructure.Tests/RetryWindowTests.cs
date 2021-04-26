// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FakeItEasy;
using NodaTime;
using Xunit;

namespace Squidex.Infrastructure
{
    public class RetryWindowTests
    {
        private readonly IClock clock = A.Fake<IClock>();

        public RetryWindowTests()
        {
            A.CallTo(() => clock.GetCurrentInstant())
                .Returns(SystemClock.Instance.GetCurrentInstant().WithoutMs());
        }

        [Fact]
        public void Should_allow_to_retry_after_reset()
        {
            var sut = new RetryWindow(TimeSpan.FromSeconds(1), 5);

            for (var i = 0; i < 5 * 2; i++)
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
            var sut = new RetryWindow(TimeSpan.FromSeconds(1), 5, clock);

            for (var i = 0; i < 5; i++)
            {
                Assert.True(sut.CanRetryAfterFailure());
            }

            var remaining = errors - 5;

            for (var i = 0; i < remaining; i++)
            {
                Assert.False(sut.CanRetryAfterFailure());
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void Should_allow_to_retry_after_few_errors(int errors)
        {
            var sut = new RetryWindow(TimeSpan.FromSeconds(1), 5, clock);

            for (var i = 0; i < errors; i++)
            {
                Assert.True(sut.CanRetryAfterFailure());
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
            var sut = new RetryWindow(TimeSpan.FromSeconds(1), 5, clock);

            var now = SystemClock.Instance.GetCurrentInstant();

            A.CallTo(() => clock.GetCurrentInstant())
                .ReturnsLazily(() => now);

            for (var i = 0; i < errors; i++)
            {
                now = now.Plus(Duration.FromMilliseconds(300));

                Assert.True(sut.CanRetryAfterFailure());
            }
        }
    }
}
