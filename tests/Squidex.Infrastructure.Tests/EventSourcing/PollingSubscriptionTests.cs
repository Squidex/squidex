// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    public class PollingSubscriptionTests
    {
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IEventSubscriber eventSubscriber = A.Fake<IEventSubscriber>();
        private readonly string position = Guid.NewGuid().ToString();

        [Fact]
        public async Task Should_subscribe_on_start()
        {
            var sut = new PollingSubscription(eventStore, eventSubscriber, "^my-stream", position);

            await WaitAndStopAsync(sut);

            A.CallTo(() => eventStore.QueryAsync(A<Func<StoredEvent, Task>>.Ignored, "^my-stream", position, A<CancellationToken>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_propagate_exception_to_subscriber()
        {
            var ex = new InvalidOperationException();

            A.CallTo(() => eventStore.QueryAsync(A<Func<StoredEvent, Task>>.Ignored, "^my-stream", position, A<CancellationToken>.Ignored))
                .Throws(ex);

            var sut = new PollingSubscription(eventStore, eventSubscriber, "^my-stream", position);

            await WaitAndStopAsync(sut);

            A.CallTo(() => eventSubscriber.OnErrorAsync(sut, ex))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_propagate_operation_cancelled_exception_to_subscriber()
        {
            var ex = new OperationCanceledException();

            A.CallTo(() => eventStore.QueryAsync(A<Func<StoredEvent, Task>>.Ignored, "^my-stream", position, A<CancellationToken>.Ignored))
                .Throws(ex);

            var sut = new PollingSubscription(eventStore, eventSubscriber, "^my-stream", position);

            await WaitAndStopAsync(sut);

            A.CallTo(() => eventSubscriber.OnErrorAsync(sut, ex))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_propagate_aggregate_operation_cancelled_exception_to_subscriber()
        {
            var ex = new AggregateException(new OperationCanceledException());

            A.CallTo(() => eventStore.QueryAsync(A<Func<StoredEvent, Task>>.Ignored, "^my-stream", position, A<CancellationToken>.Ignored))
                .Throws(ex);

            var sut = new PollingSubscription(eventStore, eventSubscriber, "^my-stream", position);

            await WaitAndStopAsync(sut);

            A.CallTo(() => eventSubscriber.OnErrorAsync(sut, ex))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_wake_up()
        {
            var sut = new PollingSubscription(eventStore, eventSubscriber, "^my-stream", position);

            sut.WakeUp();

            await WaitAndStopAsync(sut);

            A.CallTo(() => eventStore.QueryAsync(A<Func<StoredEvent, Task>>.Ignored, "^my-stream", position, A<CancellationToken>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Twice);
        }

        private static async Task WaitAndStopAsync(IEventSubscription sut)
        {
            await Task.Delay(200);

            await sut.StopAsync();
        }
    }
}
