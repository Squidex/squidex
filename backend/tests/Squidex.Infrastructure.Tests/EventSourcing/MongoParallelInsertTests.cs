// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Squidex.Infrastructure.EventSourcing
{
    [Trait("Category", "Dependencies")]
    public sealed class MongoParallelInsertTests : IClassFixture<MongoEventStoreFixture>
    {
        private readonly IGrainState<EventConsumerState> grainState = A.Fake<IGrainState<EventConsumerState>>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly IEventDataFormatter eventDataFormatter;

        public MongoEventStoreFixture _ { get; }

        public sealed class MyEventConsumerGrain : EventConsumerGrain
        {
            public MyEventConsumerGrain(
                EventConsumerFactory eventConsumerFactory,
                IGrainState<EventConsumerState> state,
                IEventStore eventStore,
                IEventDataFormatter eventDataFormatter,
                ISemanticLog log)
                : base(eventConsumerFactory, state, eventStore, eventDataFormatter, log)
            {
            }

            protected override IEventConsumerGrain GetSelf()
            {
                return this;
            }

            protected override IEventSubscription CreateSubscription(IEventStore store, IEventSubscriber subscriber, string? filter, string? position)
            {
                return store.CreateSubscription(subscriber, filter, position);
            }
        }

        public class MyEvent : IEvent
        {
        }

        public sealed class MyEventConsumer : IEventConsumer
        {
            private readonly HashSet<Guid> uniqueReceivedEvents = new HashSet<Guid>();
            private readonly TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            private readonly int expectedCount;

            public string Name => "Test";

            public string EventsFilter => ".*";

            public int Received { get; set; }

            public Task Completed => tcs.Task;

            public MyEventConsumer(int expectedCount)
            {
                this.expectedCount = expectedCount;
            }

            public Task ClearAsync()
            {
                return Task.CompletedTask;
            }

            public bool Handles(StoredEvent @event)
            {
                return true;
            }

            public Task On(Envelope<IEvent> @event)
            {
                Received++;

                uniqueReceivedEvents.Add(@event.Headers.CommitId());

                if (uniqueReceivedEvents.Count == expectedCount)
                {
                    tcs.TrySetResult(true);
                }

                return Task.CompletedTask;
            }
        }

        public MongoParallelInsertTests(MongoEventStoreFixture fixture)
        {
            _ = fixture;

            var typeNameRegistry = new TypeNameRegistry().Map(typeof(MyEvent), "My");

            eventDataFormatter = new DefaultEventDataFormatter(typeNameRegistry, JsonHelper.DefaultSerializer);
        }

        [Fact]
        public async Task Should_insert_and_retrieve_parallel()
        {
            var expectedEvents = 10 * 1000;

            var consumer = new MyEventConsumer(expectedEvents);
            var consumerGrain = new MyEventConsumerGrain(_ => consumer, grainState, _.EventStore, eventDataFormatter, log);

            await consumerGrain.ActivateAsync(consumer.Name);
            await consumerGrain.ActivateAsync();

            Parallel.For(0, 10, x =>
            {
                for (var i = 0; i < 1000; i++)
                {
                    var commitId = Guid.NewGuid();

                    var data = eventDataFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId);

                    _.EventStore.AppendAsync(commitId, commitId.ToString(), new[] { data }).Wait();
                }
            });

            var timeout = Task.Delay(5 * 1000 * 60);

            var result = Task.WhenAny(timeout, consumer.Completed);

            await result;

            Assert.NotSame(result, timeout);
            Assert.Equal(expectedEvents, consumer.Received);
        }
    }
}
