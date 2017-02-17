// ==========================================================================
//  DefaultMemoryEventNotifierTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class DefaultMemoryEventNotifierTests
    {
        private readonly DefaultMemoryEventNotifier sut = new DefaultMemoryEventNotifier(new InMemoryPubSub());

        [Fact]
        public void Should_invalidate_all_actions()
        {
            var handler1Handled = 0;
            var handler2Handled = 0;

            sut.Subscribe(() =>
            {
                handler1Handled++;
            });

            sut.NotifyEventsStored();

            sut.Subscribe(() =>
            {
                handler2Handled++;
            });

            sut.NotifyEventsStored();

            Assert.Equal(2, handler1Handled);
            Assert.Equal(1, handler2Handled);
        }
    }
}
