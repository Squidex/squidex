// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    public class PollingSubscriptionTests
    {
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IEventSubscriber<StoredEvent> eventSubscriber = A.Fake<IEventSubscriber<StoredEvent>>();
        private SubscriptionQuery query;

        public PollingSubscriptionTests()
        {
            query.Position = Guid.NewGuid().ToString();
            query.StreamFilter = "^my-stream";
        }

        [Fact]
        public async Task Should_subscribe_on_start()
        {
            await SubscribeAsync();

            A.CallTo(() => eventStore.QueryAllAsync(query.StreamFilter, query.Position, A<int>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_forward_exception_to_subscriber()
        {
            var ex = new InvalidOperationException();

            A.CallTo(() => eventStore.QueryAllAsync(query.StreamFilter, query.Position, A<int>._, A<CancellationToken>._))
                .Throws(ex);

            var sut = await SubscribeAsync(false);

            A.CallTo(() => eventSubscriber.OnErrorAsync(sut, ex))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_operation_cancelled_exception_to_subscriber()
        {
            var ex = new OperationCanceledException();

            A.CallTo(() => eventStore.QueryAllAsync(query.StreamFilter, query.Position, A<int>._, A<CancellationToken>._))
                .Throws(ex);

            var sut = await SubscribeAsync(false);

            A.CallTo(() => eventSubscriber.OnErrorAsync(sut, ex))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_aggregate_operation_cancelled_exception_to_subscriber()
        {
            var ex = new AggregateException(new OperationCanceledException());

            A.CallTo(() => eventStore.QueryAllAsync(query.StreamFilter, query.Position, A<int>._, A<CancellationToken>._))
                .Throws(ex);

            var sut = await SubscribeAsync(false);

            A.CallTo(() => eventSubscriber.OnErrorAsync(sut, ex))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_wake_up()
        {
            var sut = await SubscribeAsync(true);

            A.CallTo(() => eventStore.QueryAllAsync(query.StreamFilter, A<string>._, A<int>._, A<CancellationToken>._))
                .MustHaveHappened(2, Times.Exactly);
        }

        [Fact]
        public async Task Should_forward_events_to_subscriber()
        {
            var events = Enumerable.Range(0, 50).Select(CreateEvent).ToArray();

            var receivedEvents = new List<StoredEvent>();

            A.CallTo(() => eventStore.QueryAllAsync(query.StreamFilter, query.Position, A<int>._, A<CancellationToken>._))
                .Returns(events.ToAsyncEnumerable());

            A.CallTo(() => eventSubscriber.OnNextAsync(A<IEventSubscription>._, A<StoredEvent>._))
                .Invokes(x => receivedEvents.Add(x.GetArgument<StoredEvent>(1)!));

            await SubscribeAsync(true);

            receivedEvents.Should().BeEquivalentTo(events, options => options.Excluding(x => x.Context));
        }

        [Fact]
        public async Task Should_receive_missing_events_with_second_pull()
        {
            var events1 = Enumerable.Range(0, 200).Where(x => x % 2 == 0).Select(CreateEvent).ToArray();
            var events2 = Enumerable.Range(0, 200).Where(x => x % 2 == 1).Select(CreateEvent).ToArray();

            var receivedEvents = new List<StoredEvent>();

            A.CallTo(() => eventStore.QueryAllAsync(query.StreamFilter, query.Position, A<int>._, A<CancellationToken>._))
                .Returns(events1.ToAsyncEnumerable());

            A.CallTo(() => eventStore.QueryAllAsync(query.StreamFilter, "100", A<int>._, A<CancellationToken>._))
                .Returns(events2.ToAsyncEnumerable());

            A.CallTo(() => eventSubscriber.OnNextAsync(A<IEventSubscription>._, A<StoredEvent>._))
                .Invokes(x => receivedEvents.Add(x.GetArgument<StoredEvent>(1)!));

            await SubscribeAsync(true);

            receivedEvents.Should().BeEquivalentTo(events1.Union(events2), options => options.Excluding(x => x.Context));
        }

        [Fact]
        public async Task Should_receive_missing_events_with_next_subscription()
        {
            var events1 = Enumerable.Range(0, 200).Where(x => x % 2 == 0).Select(CreateEvent).ToArray();
            var events2 = Enumerable.Range(0, 200).Where(x => x % 2 == 1).Select(CreateEvent).ToArray();

            var receivedEvents = new List<StoredEvent>();

            A.CallTo(() => eventStore.QueryAllAsync(query.StreamFilter, query.Position, A<int>._, A<CancellationToken>._))
                .Returns(events1.ToAsyncEnumerable());

            A.CallTo(() => eventStore.QueryAllAsync(query.StreamFilter, "100", A<int>._, A<CancellationToken>._))
                .Returns(events2.ToAsyncEnumerable());

            A.CallTo(() => eventSubscriber.OnNextAsync(A<IEventSubscription>._, A<StoredEvent>._))
                .Invokes(x => receivedEvents.Add(x.GetArgument<StoredEvent>(1)!));

            await SubscribeAsync(false, true);

            query.Context = receivedEvents[^1].Context;

            await SubscribeAsync(false);

            receivedEvents.Should().BeEquivalentTo(events1.Union(events2), options => options.Excluding(x => x.Context));
        }

        [Fact]
        public async Task Should_not_receive_same_events_again_with_second_subscription()
        {
            var events1 = Enumerable.Range(0, 200).Where(x => x % 2 == 0).Select(CreateEvent).ToArray();
            var events2 = Enumerable.Range(0, 200).Where(x => x % 2 == 1).Select(CreateEvent).ToArray();

            var receivedEvents = new List<StoredEvent>();

            A.CallTo(() => eventStore.QueryAllAsync(query.StreamFilter, query.Position, A<int>._, A<CancellationToken>._))
                .Returns(events1.ToAsyncEnumerable());

            A.CallTo(() => eventStore.QueryAllAsync(query.StreamFilter, "100", A<int>._, A<CancellationToken>._))
                .Returns(events2.ToAsyncEnumerable());

            A.CallTo(() => eventSubscriber.OnNextAsync(A<IEventSubscription>._, A<StoredEvent>._))
                .Invokes(x => receivedEvents.Add(x.GetArgument<StoredEvent>(1)!));

            await SubscribeAsync(false);

            query.Context = receivedEvents[^1].Context;

            await SubscribeAsync(false);

            receivedEvents.Should().BeEquivalentTo(events1.Union(events2), options => options.Excluding(x => x.Context));
        }

        private StoredEvent CreateEvent(int position)
        {
            return new StoredEvent(
                "my-stream",
                position.ToString(CultureInfo.InvariantCulture)!,
                position,
                new EventData(
                    "type",
                    new EnvelopeHeaders
                    {
                        [CommonHeaders.EventId] = Guid.NewGuid().ToString()
                    },
                    "payload"));
        }

        private async Task<IEventSubscription> SubscribeAsync(bool wakeup = true, bool queryOnce = false)
        {
            var sut = new PollingSubscription(eventStore, eventSubscriber, query, queryOnce);

            try
            {
                if (wakeup)
                {
                    sut.WakeUp();
                }

                await Task.Delay(200);
            }
            finally
            {
                sut.Dispose();
            }

            return sut;
        }
    }
}
