// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Microsoft.Extensions.Logging;
using Orleans.Core;
using Orleans.Internal;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Squidex.Infrastructure.EventSourcing
{
    [Trait("Category", "Dependencies")]
    public sealed class MongoParallelInsertTests : IClassFixture<MongoEventStoreReplicaSetFixture>
    {
        private readonly IGrainState<EventConsumerState> state = A.Fake<IGrainState<EventConsumerState>>();
        private readonly ILogger<EventConsumerGrain> log = A.Fake<ILogger<EventConsumerGrain>>();
        private readonly IEventFormatter eventFormatter;

        public MongoEventStoreFixture _ { get; }

        public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
        {
            [ThreadStatic]
            private static bool currentThreadIsProcessingItems;

            private readonly LinkedList<Task> tasks = new LinkedList<Task>();
            private readonly int maxDegreeOfParallelism;
            private int delegatesQueuedOrRunning;

            public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
            {
                this.maxDegreeOfParallelism = maxDegreeOfParallelism;
            }

            protected sealed override void QueueTask(Task task)
            {
                lock (tasks)
                {
                    tasks.AddLast(task);

                    if (delegatesQueuedOrRunning < maxDegreeOfParallelism)
                    {
                        ++delegatesQueuedOrRunning;

                        NotifyThreadPoolOfPendingWork();
                    }
                }
            }

            private void NotifyThreadPoolOfPendingWork()
            {
                ThreadPool.UnsafeQueueUserWorkItem(_ =>
                {
                    currentThreadIsProcessingItems = true;
                    try
                    {
                        while (true)
                        {
                            Task item;
                            lock (tasks)
                            {
                                if (tasks.Count == 0)
                                {
                                    --delegatesQueuedOrRunning;
                                    break;
                                }

                                item = tasks.First!.Value;

                                tasks.RemoveFirst();
                            }

                            TryExecuteTask(item);
                        }
                    }
                    finally
                    {
                        currentThreadIsProcessingItems = false;
                    }
                }, null);
            }

            protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                if (!currentThreadIsProcessingItems)
                {
                    return false;
                }

                if (taskWasPreviouslyQueued)
                {
                    TryDequeue(task);
                }

                return TryExecuteTask(task);
            }

            protected sealed override bool TryDequeue(Task task)
            {
                lock (tasks)
                {
                    return tasks.Remove(task);
                }
            }

            public sealed override int MaximumConcurrencyLevel
            {
                get => maxDegreeOfParallelism;
            }

            protected sealed override IEnumerable<Task> GetScheduledTasks()
            {
                var lockTaken = false;
                try
                {
                    Monitor.TryEnter(tasks, ref lockTaken);

                    if (lockTaken)
                    {
                        return tasks.ToArray();
                    }
                    else
                    {
                        ThrowHelper.NotSupportedException();
                        return default!;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(tasks);
                    }
                }
            }
        }

        public sealed class MyEventConsumerGrain : EventConsumerGrain
        {
            private readonly TaskScheduler scheduler = new LimitedConcurrencyLevelTaskScheduler(1);

            public TaskScheduler Scheduler => scheduler;

            public MyEventConsumerGrain(
                IGrainIdentity identity,
                IGrainState<EventConsumerState> state,
                IEventConsumerFactory eventConsumerFactory,
                IEventFormatter eventFormatter,
                IEventStore eventStore,
                ILogger<EventConsumerGrain> log)
                : base(identity, state, eventConsumerFactory, eventFormatter, eventStore, log)
            {
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

            public Func<int, Task> EventReceived { get; set; }

            public string Name => "Test";

            public string EventsFilter => ".*";

            public int Received { get; set; }

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
            _.Cleanup();

            var typeNameRegistry = new TypeNameRegistry().Map(typeof(MyEvent), "My");

            eventFormatter = new DefaultEventFormatter(typeNameRegistry, TestUtils.DefaultSerializer);
        }

        [Fact]
        public async Task Should_insert_and_retrieve_parallel()
        {
            var expectedEvents = 10 * 1000;

            var eventConsumer = new MyEventConsumer(expectedEvents);
            var eventConsumerGrain = BuildGrain(eventConsumer);

            await eventConsumerGrain.ActivateAsync();

            await Parallel.ForEachAsync(Enumerable.Range(0, 20), async (_, _) =>
            {
                for (var i = 0; i < 500; i++)
                {
                    var commitId = Guid.NewGuid();

                    var data = eventFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId);

                    await _.EventStore.AppendAsync(commitId, commitId.ToString(), new[] { data });
                }
            });

            await AssertConsumerAsync(expectedEvents, eventConsumer);
        }

        [Fact]
        public async Task Should_insert_and_retrieve_parallel_with_multiple_events_per_commit()
        {
            var expectedEvents = 10 * 1000;

            var eventConsumer = new MyEventConsumer(expectedEvents);
            var eventConsumerGrain = BuildGrain(eventConsumer);

            await eventConsumerGrain.ActivateAsync();

            await Parallel.ForEachAsync(Enumerable.Range(0, 10), async (_, _) =>
            {
                for (var i = 0; i < 500; i++)
                {
                    var commitId = Guid.NewGuid();

                    var data1 = eventFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId);
                    var data2 = eventFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId);

                    await _.EventStore.AppendAsync(commitId, commitId.ToString(), new[] { data1, data2 });
                }
            });

            await AssertConsumerAsync(expectedEvents, eventConsumer);
        }

        [Fact]
        public async Task Should_insert_and_retrieve_afterwards()
        {
            var expectedEvents = 10 * 1000;

            var eventConsumer = new MyEventConsumer(expectedEvents);
            var eventConsumerGrain = BuildGrain(eventConsumer);

            await Parallel.ForEachAsync(Enumerable.Range(0, 10), async (_, _) =>
            {
                for (var i = 0; i < 1000; i++)
                {
                    var commitId = Guid.NewGuid();

                    var data = eventFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId);

                    await _.EventStore.AppendAsync(commitId, commitId.ToString(), new[] { data });
                }
            });

            await eventConsumerGrain.ActivateAsync();

            await AssertConsumerAsync(expectedEvents, eventConsumer);
        }

        [Fact]
        public async Task Should_insert_and_retrieve_partially_afterwards()
        {
            var expectedEvents = 10 * 1000;

            var eventConsumer = new MyEventConsumer(expectedEvents);
            var eventConsumerGrain = BuildGrain(eventConsumer);

            await Parallel.ForEachAsync(Enumerable.Range(0, 10), async (_, _) =>
            {
                for (var i = 0; i < 500; i++)
                {
                    var commitId = Guid.NewGuid();

                    var data = eventFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId);

                    await _.EventStore.AppendAsync(commitId, commitId.ToString(), new[] { data });
                }
            });

            await eventConsumerGrain.ActivateAsync();

            await Parallel.ForEachAsync(Enumerable.Range(0, 10), async (_, _) =>
            {
                for (var i = 0; i < 500; i++)
                {
                    var commitId = Guid.NewGuid();

                    var data = eventFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId);

                    await _.EventStore.AppendAsync(commitId, commitId.ToString(), new[] { data });
                }
            });

            await AssertConsumerAsync(expectedEvents, eventConsumer);
        }

        [Fact]
        public async Task Should_insert_and_retrieve_parallel_with_waits()
        {
            var expectedEvents = 10 * 1000;

            var eventConsumer = new MyEventConsumer(expectedEvents);
            var eventConsumerGrain = BuildGrain(eventConsumer);

            await eventConsumerGrain.ActivateAsync();

            await Parallel.ForEachAsync(Enumerable.Range(0, 10), async (_, _) =>
            {
                for (var j = 0; j < 10; j++)
                {
                    for (var i = 0; i < 100; i++)
                    {
                        var commitId = Guid.NewGuid();

                        var data = eventFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId);

                        await _.EventStore.AppendAsync(commitId, commitId.ToString(), new[] { data });
                    }

                    await Task.Delay(1000);
                }
            });

            await AssertConsumerAsync(expectedEvents, eventConsumer);
        }

        [Fact]
        public async Task Should_insert_and_retrieve_parallel_with_stops_and_starts()
        {
            var expectedEvents = 10 * 1000;

            var eventConsumer = new MyEventConsumer(expectedEvents);
            var eventConsumerGrain = BuildGrain(eventConsumer);

            var scheduler = eventConsumerGrain.Scheduler;

            eventConsumer.EventReceived = count =>
            {
                if (count % 1000 == 0)
                {
                    Task.Factory.StartNew(async () =>
                    {
                        await eventConsumerGrain.StopAsync();
                        await eventConsumerGrain.StartAsync();
                    }, default, default, scheduler).Forget();
                }

                return Task.CompletedTask;
            };

            await eventConsumerGrain.ActivateAsync();

            await Parallel.ForEachAsync(Enumerable.Range(0, 10), async (_, _) =>
            {
                for (var i = 0; i < 1000; i++)
                {
                    var commitId = Guid.NewGuid();

                    var data = eventFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId);

                    await _.EventStore.AppendAsync(commitId, commitId.ToString(), new[] { data });
                }
            });

            await AssertConsumerAsync(expectedEvents, eventConsumer);
        }

        private MyEventConsumerGrain BuildGrain(IEventConsumer eventConsumer)
        {
            var identity = A.Fake<IGrainIdentity>();

            A.CallTo(() => identity.PrimaryKeyString)
                .Returns(eventConsumer.Name);

            var eventConsumerFactory = A.Fake<IEventConsumerFactory>();

            A.CallTo(() => eventConsumerFactory.Create(eventConsumer.Name))
                .Returns(eventConsumer);

            return new MyEventConsumerGrain(identity, state, eventConsumerFactory, eventFormatter, _.EventStore, log);
        }

        private static async Task AssertConsumerAsync(int expectedEvents, MyEventConsumer eventConsumer)
        {
            await eventConsumer.Completed.WithTimeout(TimeSpan.FromSeconds(100));

            await Task.Delay(2000);

            Assert.Equal(expectedEvents, eventConsumer.Received);
        }
    }
}
