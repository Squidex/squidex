// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
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

                return Task.CompletedTask;
            }, 2000);

            timer.SkipCurrentDelay();
            timer.StopAsync().Wait();

            Assert.True(called);
        }
    }
}
