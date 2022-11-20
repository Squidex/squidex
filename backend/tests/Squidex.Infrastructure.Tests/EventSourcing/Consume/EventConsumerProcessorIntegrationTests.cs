// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Squidex.Caching;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.EventSourcing.Consume;

public abstract class EventConsumerProcessorIntegrationTests
{
    private readonly Lazy<IEventStore> store;

    public sealed class EventConsumer : IEventConsumer
    {
        public List<Guid> Events { get; } = new List<Guid>();

        public string Name => "Consumer";

        public Func<int, Task> EventReceived { get; set; }

        public async Task On(Envelope<IEvent> @event)
        {
            Events.Add(@event.Headers.EventId());

            if (EventReceived != null)
            {
                await EventReceived(Events.Count);
            }
        }
    }

    protected IEventStore EventStore
    {
        get => store.Value;
    }

    protected EventConsumerProcessorIntegrationTests()
    {
#pragma warning disable MA0056 // Do not call overridable members in constructor
        store = new Lazy<IEventStore>(CreateStore);
#pragma warning restore MA0056 // Do not call overridable members in constructor
    }

    public abstract IEventStore CreateStore();

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task Should_subscribe_with_parallel_writes(int startStop)
    {
        var numTasks = 100;
        var numEvents = 50;

        var eventConsumer = new EventConsumer();

        var mongoClient = new MongoClient(TestConfig.Configuration["mongodb:configuration"]);
        var mongoDatabase = mongoClient.GetDatabase(TestConfig.Configuration["mongodb:database"]);

        var typeRegistry = new TypeRegistry().Add<IEvent, MyEvent>("MyEvent");

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(TestUtils.DefaultSerializer)
            .AddSingleton(EventStore)
            .AddSingleton(eventConsumer)
            .AddSingleton(mongoClient)
            .AddSingleton(mongoDatabase)
            .AddSingleton(typeRegistry)
            .AddSingleton(typeof(IPersistenceFactory<>), typeof(Store<>))
            .AddSingleton<EventConsumerProcessor>()
            .AddSingleton<IEventConsumer>(eventConsumer)
            .AddSingleton<IEventFormatter, DefaultEventFormatter>()
            .AddSingleton<IEventStreamNames, DefaultEventStreamNames>()
            .AddSingleton(typeof(ISnapshotStore<>), typeof(MongoSnapshotStore<>))
            .BuildServiceProvider();

        var processor = services.GetRequiredService<EventConsumerProcessor>();

        var persistenceFactory = services.GetRequiredService<IPersistenceFactory<None>>();

        // Also start the event consumer, because it might be stopped from previous run.
        await processor.InitializeAsync(default);
        await processor.ActivateAsync();
        await processor.StartAsync();

        async Task StartStop()
        {
            await processor.StopAsync();
            await processor.StartAsync();
        }

        eventConsumer.EventReceived = i =>
        {
            if (startStop > 0 && i % startStop == 0)
            {
                // Do not await the task here, other wise we could create deadlock.
                StartStop().Forget();
            }

            return Task.CompletedTask;
        };

        // Create events in parallel.
        await Parallel.ForEachAsync(Enumerable.Range(0, numTasks), async (i, ct) =>
        {
            var persistence = persistenceFactory.WithEventSourcing(typeof(None), DomainId.NewGuid(), null);

            for (var j = 0; j < numEvents; j++)
            {
                await persistence.WriteEventsAsync(new List<Envelope<IEvent>>
                {
                    Envelope.Create(new MyEvent())
                });
            }
        });

        var expectedEvents = numEvents * numTasks;

        // Wait for all events to arrive.
        using (var cts = new CancellationTokenSource(20000))
        {
            while (!cts.IsCancellationRequested && eventConsumer.Events.Count < expectedEvents)
            {
                await Task.Delay(100);
            }
        }

        await processor.StopAsync();

        Assert.Equal(expectedEvents, eventConsumer.Events.Count);
    }
}
