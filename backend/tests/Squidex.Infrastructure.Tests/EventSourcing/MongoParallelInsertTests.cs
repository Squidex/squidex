// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Infrastructure.EventSourcing.Consume;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.TestHelpers;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Squidex.Infrastructure.EventSourcing;

[Trait("Category", "Dependencies")]
public sealed class MongoParallelInsertTests : IClassFixture<MongoEventStoreReplicaSetFixture>
{
    private readonly TestState<EventConsumerState> state = new TestState<EventConsumerState>(DomainId.Empty);
    private readonly IEventFormatter eventFormatter;

    public MongoEventStoreFixture _ { get; }

    public class MyEvent : IEvent
    {
    }

    public sealed class MyEventConsumer : IEventConsumer
    {
        private readonly HashSet<Guid> uniqueReceivedEvents = new HashSet<Guid>();
        private readonly TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        private readonly int expectedCount;

        public Func<int, Task> EventReceived { get; set; }

        public int Received { get; set; }

        public string Name { get; } = RandomHash.Simple();

        public string EventsFilter => $"^{Name}";

        public Task Completed => tcs.Task;

        public MyEventConsumer(int expectedCount)
        {
            this.expectedCount = expectedCount;
        }

        public async Task On(Envelope<IEvent> @event)
        {
            Received++;

            uniqueReceivedEvents.Add(@event.Headers.EventId());

            if (uniqueReceivedEvents.Count == expectedCount)
            {
                tcs.TrySetResult(true);
            }

            if (EventReceived != null)
            {
                await EventReceived(Received);
            }
        }
    }

    public MongoParallelInsertTests(MongoEventStoreReplicaSetFixture fixture)
    {
        _ = fixture;

        var typeRegistry = new TypeRegistry().Add<IEvent, MyEvent>("MyEvent");

        eventFormatter = new DefaultEventFormatter(typeRegistry, TestUtils.DefaultSerializer);
    }

    [Fact]
    public async Task Should_insert_and_retrieve_parallel()
    {
        const int expectedEvents = 2_000;

        var eventConsumer = new MyEventConsumer(expectedEvents);
        var eventProcessor = BuildProcessor(eventConsumer);

        await eventProcessor.InitializeAsync(default);
        await eventProcessor.ActivateAsync();

        await InsertAsync(eventConsumer, expectedEvents, parallelism: 20);

        await AssertConsumerAsync(expectedEvents, eventConsumer);
    }

    [Fact]
    public async Task Should_insert_and_retrieve_parallel_with_multiple_events_per_commit()
    {
        const int expectedEvents = 2_000;

        var eventConsumer = new MyEventConsumer(expectedEvents);
        var eventProcessor = BuildProcessor(eventConsumer);

        await eventProcessor.InitializeAsync(default);
        await eventProcessor.ActivateAsync();

        await InsertAsync(eventConsumer, expectedEvents, messagesPerCommit: 2);

        await AssertConsumerAsync(expectedEvents, eventConsumer);
    }

    [Fact]
    public async Task Should_insert_and_retrieve_afterwards()
    {
        const int expectedEvents = 2_000;

        var eventConsumer = new MyEventConsumer(expectedEvents);
        var eventProcessor = BuildProcessor(eventConsumer);

        await InsertAsync(eventConsumer, expectedEvents);

        await eventProcessor.InitializeAsync(default);
        await eventProcessor.ActivateAsync();

        await AssertConsumerAsync(expectedEvents, eventConsumer);
    }

    [Fact]
    public async Task Should_insert_and_retrieve_partially_afterwards()
    {
        const int expectedEvents = 2_000;

        var eventConsumer = new MyEventConsumer(expectedEvents);
        var eventProcessor = BuildProcessor(eventConsumer);

        await InsertAsync(eventConsumer, expectedEvents / 2);

        await eventProcessor.InitializeAsync(default);
        await eventProcessor.ActivateAsync();

        await InsertAsync(eventConsumer, expectedEvents / 2);

        await AssertConsumerAsync(expectedEvents, eventConsumer);
    }

    [Fact]
    public async Task Should_insert_and_retrieve_parallel_with_waits()
    {
        const int expectedEvents = 2_000;

        var eventConsumer = new MyEventConsumer(expectedEvents);
        var eventProcessor = BuildProcessor(eventConsumer);

        await eventProcessor.InitializeAsync(default);
        await eventProcessor.ActivateAsync();

        await InsertAsync(eventConsumer, expectedEvents, iterations: 10);

        await AssertConsumerAsync(expectedEvents, eventConsumer);
    }

    [Fact]
    public async Task Should_insert_and_retrieve_parallel_with_stops_and_starts()
    {
        const int expectedEvents = 2_000;

        var eventConsumer = new MyEventConsumer(expectedEvents);
        var eventProcessor = BuildProcessor(eventConsumer);

        eventConsumer.EventReceived = async count =>
        {
            if (count % 1000 == 0)
            {
                await eventProcessor.StopAsync();
                await eventProcessor.StartAsync();
            }
        };

        await eventProcessor.InitializeAsync(default);
        await eventProcessor.ActivateAsync();

        await InsertAsync(eventConsumer, expectedEvents);

        await AssertConsumerAsync(expectedEvents, eventConsumer);
    }

    private EventConsumerProcessor BuildProcessor(IEventConsumer eventConsumer)
    {
        return new EventConsumerProcessor(
            state.PersistenceFactory,
            eventConsumer,
            eventFormatter,
            _.EventStore,
            A.Fake<ILogger<EventConsumerProcessor>>());
    }

    private Task InsertAsync(IEventConsumer consumer, int numItems, int parallelism = 5, int messagesPerCommit = 1, int iterations = 1)
    {
        var perTask = numItems / (parallelism * messagesPerCommit * iterations);

        return Parallel.ForEachAsync(Enumerable.Range(0, parallelism), async (_, _) =>
        {
            for (var i = 0; i < iterations; i++)
            {
                for (var j = 0; j < perTask; j++)
                {
                    var streamName = $"{consumer.Name}-{Guid.NewGuid()}";

                    var commitId = Guid.NewGuid();
                    var commitList = new List<EventData>();

                    for (var k = 0; k < messagesPerCommit; k++)
                    {
                        commitList.Add(eventFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId));
                    }

                    await _.EventStore.AppendAsync(commitId, streamName, commitList);
                }

                if (i < iterations - 1)
                {
                    await Task.Delay(1000);
                }
            }
        });
    }

    private static async Task AssertConsumerAsync(int expectedEvents, MyEventConsumer eventConsumer)
    {
        await Task.WhenAny(eventConsumer.Completed, Task.Delay(TimeSpan.FromSeconds(20)));
        await Task.Delay(2000);

        Assert.Equal(expectedEvents, eventConsumer.Received);
    }
}
