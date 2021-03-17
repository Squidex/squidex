// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans.Internal;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.TestHelpers;
using Squidex.Log;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Squidex.Infrastructure.EventSourcing
{
    [Trait("Category", "Dependencies")]
    public sealed class MongoParallelInsertTests : IClassFixture<MongoEventStoreReplicaSetFixture>
    {
        private readonly IGrainState<EventConsumerState> grainState = A.Fake<IGrainState<EventConsumerState>>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly IEventDataFormatter eventDataFormatter;

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
                        throw new NotSupportedException();
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
                EventConsumerFactory eventConsumerFactory,
                IGrainState<EventConsumerState> state,
                IEventStore eventStore,
                IEventDataFormatter eventDataFormatter,
                ISemanticLog log)
                : base(eventConsumerFactory, state, eventStore, eventDataFormatter, log)
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

            eventDataFormatter = new DefaultEventDataFormatter(typeNameRegistry, TestUtils.DefaultSerializer);
        }

        [Fact]
        public async Task Should_insert_and_retrieve_parallel()
        {
            var expectedEvents = 10 * 1000;

            var consumer = new MyEventConsumer(expectedEvents);
            var consumerGrain = new MyEventConsumerGrain(_ => consumer, grainState, _.EventStore, eventDataFormatter, log);

            await consumerGrain.ActivateAsync(consumer.Name);
            await consumerGrain.ActivateAsync();

            Parallel.For(0, 20, x =>
            {
                for (var i = 0; i < 500; i++)
                {
                    var commitId = Guid.NewGuid();

                    var data = eventDataFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId);

                    _.EventStore.AppendAsync(commitId, commitId.ToString(), new[] { data }).Wait();
                }
            });

            await AssertConsumerAsync(expectedEvents, consumer);
        }

        [Fact]
        public async Task Should_insert_and_retrieve_parallel_with_multiple_events_per_commit()
        {
            var expectedEvents = 10 * 1000;

            var consumer = new MyEventConsumer(expectedEvents);
            var consumerGrain = new MyEventConsumerGrain(_ => consumer, grainState, _.EventStore, eventDataFormatter, log);

            await consumerGrain.ActivateAsync(consumer.Name);
            await consumerGrain.ActivateAsync();

            Parallel.For(0, 10, x =>
            {
                for (var i = 0; i < 500; i++)
                {
                    var commitId = Guid.NewGuid();

                    var data1 = eventDataFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId);
                    var data2 = eventDataFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId);

                    _.EventStore.AppendAsync(commitId, commitId.ToString(), new[] { data1, data2 }).Wait();
                }
            });

            await AssertConsumerAsync(expectedEvents, consumer);
        }

        [Fact]
        public async Task Should_insert_and_retrieve_afterwards()
        {
            var expectedEvents = 10 * 1000;

            var consumer = new MyEventConsumer(expectedEvents);
            var consumerGrain = new MyEventConsumerGrain(_ => consumer, grainState, _.EventStore, eventDataFormatter, log);

            Parallel.For(0, 10, x =>
            {
                for (var i = 0; i < 1000; i++)
                {
                    var commitId = Guid.NewGuid();

                    var data = eventDataFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId);

                    _.EventStore.AppendAsync(commitId, commitId.ToString(), new[] { data }).Wait();
                }
            });

            await consumerGrain.ActivateAsync(consumer.Name);
            await consumerGrain.ActivateAsync();

            await AssertConsumerAsync(expectedEvents, consumer);
        }

        [Fact]
        public async Task Should_insert_and_retrieve_partially_afterwards()
        {
            var expectedEvents = 10 * 1000;

            var consumer = new MyEventConsumer(expectedEvents);
            var consumerGrain = new MyEventConsumerGrain(_ => consumer, grainState, _.EventStore, eventDataFormatter, log);

            Parallel.For(0, 10, x =>
            {
                for (var i = 0; i < 500; i++)
                {
                    var commitId = Guid.NewGuid();

                    var data = eventDataFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId);

                    _.EventStore.AppendAsync(commitId, commitId.ToString(), new[] { data }).Wait();
                }
            });

            await consumerGrain.ActivateAsync(consumer.Name);
            await consumerGrain.ActivateAsync();

            Parallel.For(0, 10, x =>
            {
                for (var i = 0; i < 500; i++)
                {
                    var commitId = Guid.NewGuid();

                    var data = eventDataFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId);

                    _.EventStore.AppendAsync(commitId, commitId.ToString(), new[] { data }).Wait();
                }
            });

            await AssertConsumerAsync(expectedEvents, consumer);
        }

        [Fact]
        public async Task Should_insert_and_retrieve_parallel_with_waits()
        {
            var expectedEvents = 10 * 1000;

            var consumer = new MyEventConsumer(expectedEvents);
            var consumerGrain = new MyEventConsumerGrain(_ => consumer, grainState, _.EventStore, eventDataFormatter, log);

            await consumerGrain.ActivateAsync(consumer.Name);
            await consumerGrain.ActivateAsync();

            Parallel.For(0, 10, x =>
            {
                for (var j = 0; j < 10; j++)
                {
                    for (var i = 0; i < 100; i++)
                    {
                        var commitId = Guid.NewGuid();

                        var data = eventDataFormatter.ToEventData(Envelope.Create<IEvent>(new MyEvent()), commitId);

                        _.EventStore.AppendAsync(commitId, commitId.ToString(), new[] { data }).Wait();
                    }

                    Thread.Sleep(1000);
                }
            });

            await AssertConsumerAsync(expectedEvents, consumer);
        }

        [Fact]
        public async Task Should_insert_and_retrieve_parallel_with_stops_and_starts()
        {
            var expectedEvents = 10 * 1000;

            var consumer = new MyEventConsumer(expectedEvents);
            var consumerGrain = new MyEventConsumerGrain(_ => consumer, grainState, _.EventStore, eventDataFormatter, log);

            var scheduler = consumerGrain.Scheduler;

            consumer.EventReceived = count =>
            {
                if (count % 1000 == 0)
                {
                    Task.Factory.StartNew(async () =>
                    {
                        await consumerGrain.StopAsync();
                        await consumerGrain.StartAsync();
                    }, default, default, scheduler).Forget();
                }

                return Task.CompletedTask;
            };

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

            await AssertConsumerAsync(expectedEvents, consumer);
        }

        private static async Task AssertConsumerAsync(int expectedEvents, MyEventConsumer consumer)
        {
            await consumer.Completed.WithTimeout(TimeSpan.FromSeconds(100));

            await Task.Delay(2000);

            Assert.Equal(expectedEvents, consumer.Received);
        }
    }
}
