// ==========================================================================
//  CompletionTimerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure.Timers
{
    public class CompletionTimerTests
    {
        [Fact]
        public void Should_invoke_once_even_with_delay()
        {
            var called = false;

            var timer = new CompletionTimer(2000, ct =>
            {
                called = true;

                return TaskHelper.Done;
            }, 2000);

            timer.SkipCurrentDelay();
            timer.StopAsync().Wait();

            Assert.True(called);
        }

        public void Should_invoke_dispose_within_timer()
        {
            CompletionTimer timer = null;

            timer = new CompletionTimer(10, ct =>
            {
                timer?.StopAsync().Wait();

                return TaskHelper.Done;
            }, 10);

            Thread.Sleep(1000);

            timer.SkipCurrentDelay();
            timer.StopAsync().Wait();
        }
    }
}
