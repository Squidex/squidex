// ==========================================================================
//  DefaultEventNotifierTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class DefaultEventNotifierTests
    {
        private readonly DefaultEventNotifier sut = new DefaultEventNotifier(new InMemoryPubSub());

        [Fact]
        public void Should_invalidate_all_actions()
        {
            var handler1Handled = 0;
            var handler2Handled = 0;

            var streamNames = new List<string>();

            sut.Subscribe(x =>
            {
                streamNames.Add(x);

                handler1Handled++;
            });

            sut.NotifyEventsStored("a");

            sut.Subscribe(x =>
            {
                streamNames.Add(x);

                handler2Handled++;
            });

            sut.NotifyEventsStored("b");

            Assert.Equal(2, handler1Handled);
            Assert.Equal(1, handler2Handled);

            Assert.Equal(streamNames.ToArray(), new[] { "a", "b", "b" });
        }
    }
}
