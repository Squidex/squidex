// ==========================================================================
//  PollingSubscriptionTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class PollingSubscriptionTests
    {
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IEventNotifier eventNotifier = new DefaultEventNotifier(new InMemoryPubSub());
        private readonly IEventSubscriber eventSubscriber = A.Fake<IEventSubscriber>();
        private readonly PollingSubscription sut;
        private readonly string position = Guid.NewGuid().ToString();

        public PollingSubscriptionTests()
        {
            sut = new PollingSubscription(eventStore, eventNotifier, eventSubscriber, "^my-stream", position);
        }

        [Fact]
        public async Task Should_subscribe_on_start()
        {
            await WaitAndStopAsync();

            A.CallTo(() => eventStore.GetEventsAsync(A<Func<StoredEvent, Task>>.Ignored, A<CancellationToken>.Ignored, "^my-stream", position))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_not_subscribe_on_notify_when_stream_matches()
        {
            eventNotifier.NotifyEventsStored("other-stream-123");

            await WaitAndStopAsync();

            A.CallTo(() => eventStore.GetEventsAsync(A<Func<StoredEvent, Task>>.Ignored, A<CancellationToken>.Ignored, "^my-stream", position))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_subscribe_on_notify_when_stream_matches()
        {
            eventNotifier.NotifyEventsStored("my-stream-123");

            await WaitAndStopAsync();

            A.CallTo(() => eventStore.GetEventsAsync(A<Func<StoredEvent, Task>>.Ignored, A<CancellationToken>.Ignored, "^my-stream", position))
                .MustHaveHappened(Repeated.Exactly.Twice);
        }

        private async Task WaitAndStopAsync()
        {
            await Task.Delay(200);

            await sut.StopAsync();
        }
    }
}
