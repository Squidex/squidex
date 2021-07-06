// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
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

            A.CallTo(() => eventStore.QueryAllAsync("^my-stream", position, A<int>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_propagate_exception_to_subscriber()
        {
            var ex = new InvalidOperationException();

            A.CallTo(() => eventStore.QueryAllAsync("^my-stream", position, A<int>._, A<CancellationToken>._))
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

            A.CallTo(() => eventStore.QueryAllAsync("^my-stream", position, A<int>._, A<CancellationToken>._))
                .Throws(ex);

            var sut = new PollingSubscription(eventStore, eventSubscriber, "^my-stream", position);

            await WaitAndStopAsync(sut);

            A.CallTo(() => eventSubscriber.OnErrorAsync(sut, ex))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_propagate_aggregate_operation_cancelled_exception_to_subscriber()
        {
            var ex = new AggregateException(new OperationCanceledException());

            A.CallTo(() => eventStore.QueryAllAsync("^my-stream", position, A<int>._, A<CancellationToken>._))
                .Throws(ex);

            var sut = new PollingSubscription(eventStore, eventSubscriber, "^my-stream", position);

            await WaitAndStopAsync(sut);

            A.CallTo(() => eventSubscriber.OnErrorAsync(sut, ex))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_wake_up()
        {
            var sut = new PollingSubscription(eventStore, eventSubscriber, "^my-stream", position);

            sut.WakeUp();

            await WaitAndStopAsync(sut);

            A.CallTo(() => eventStore.QueryAllAsync("^my-stream", position, A<int>._, A<CancellationToken>._))
                .MustHaveHappened(2, Times.Exactly);
        }

        private static async Task WaitAndStopAsync(IEventSubscription sut)
        {
            await Task.Delay(200);

            sut.Unsubscribe();
        }
    }
}
