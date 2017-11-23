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
    public class EventSubscriptionTests
    {
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IEventSubscriber eventSubscriber = A.Fake<IEventSubscriber>();
        private readonly string position = Guid.NewGuid().ToString();

        [Fact]
        public async Task Should_subscribe_on_start()
        {
            var sut = new EventStoreSubscription(eventStore, eventSubscriber, "^my-stream", position);

            await WaitAndStopAsync(sut);

            A.CallTo(() => eventStore.GetEventsAsync(A<Func<StoredEvent, Task>>.Ignored, A<CancellationToken>.Ignored, "^my-stream", position))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_propagate_exception_to_subscriber()
        {
            var ex = new InvalidOperationException();

            A.CallTo(() => eventStore.GetEventsAsync(A<Func<StoredEvent, Task>>.Ignored, A<CancellationToken>.Ignored, "^my-stream", position))
                .Throws(ex);

            var sut = new EventStoreSubscription(eventStore, eventSubscriber, "^my-stream", position);

            await WaitAndStopAsync(sut);

            A.CallTo(() => eventSubscriber.OnErrorAsync(sut, ex))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_propagate_operation_cancelled_exception_to_subscriber()
        {
            var ex = new OperationCanceledException();

            A.CallTo(() => eventStore.GetEventsAsync(A<Func<StoredEvent, Task>>.Ignored, A<CancellationToken>.Ignored, "^my-stream", position))
                .Throws(ex);

            var sut = new EventStoreSubscription(eventStore, eventSubscriber, "^my-stream", position);

            await WaitAndStopAsync(sut);

            A.CallTo(() => eventSubscriber.OnErrorAsync(sut, ex))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_propagate_aggregate_operation_cancelled_exception_to_subscriber()
        {
            var ex = new AggregateException(new OperationCanceledException());

            A.CallTo(() => eventStore.GetEventsAsync(A<Func<StoredEvent, Task>>.Ignored, A<CancellationToken>.Ignored, "^my-stream", position))
                .Throws(ex);

            var sut = new EventStoreSubscription(eventStore, eventSubscriber, "^my-stream", position);

            await WaitAndStopAsync(sut);

            A.CallTo(() => eventSubscriber.OnErrorAsync(sut, ex))
                .MustNotHaveHappened();
        }

        private static async Task WaitAndStopAsync(IEventSubscription sut)
        {
            await Task.Delay(200);

            await sut.StopAsync();
        }
    }
}
