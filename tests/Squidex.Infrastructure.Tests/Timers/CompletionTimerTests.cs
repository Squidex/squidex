// ==========================================================================
//  CompletionTimerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

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

            timer.Wakeup();
            timer.Dispose();

            Assert.True(called);
        }
    }
}
