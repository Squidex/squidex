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
using FluentAssertions;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    public abstract class EventStoreTests<T> where T : IEventStore
    {
        private readonly Lazy<T> sut;
        private string subscriptionPosition;

        public sealed class EventSubscriber : IEventSubscriber
        {
            public List<StoredEvent> Events { get; } = new List<StoredEvent>();

            public string LastPosition { get; set; }

            public Task OnErrorAsync(IEventSubscription subscription, Exception exception)
            {
                throw new NotSupportedException();
            }

            public Task OnEventAsync(IEventSubscription subscription, StoredEvent storedEvent)
            {
                LastPosition = storedEvent.EventPosition;

                Events.Add(storedEvent);

                return TaskHelper.Done;
            }
        }

        protected T Sut
        {
            get { return sut.Value; }
        }

        protected abstract int SubscriptionDelayInMs { get; }

        protected EventStoreTests()
        {
            sut = new Lazy<T>(CreateStore);
        }

        public abstract T CreateStore();

        [Fact]
        public async Task Should_throw_exception_for_version_mismatch()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var events = new[]
            {
                new EventData("Type1", new EnvelopeHeaders(), "1"),
                new EventData("Type2", new EnvelopeHeaders(), "2")
            };

            await Assert.ThrowsAsync<WrongEventVersionException>(() => Sut.AppendAsync(Guid.NewGuid(), streamName, 0, events));
        }

        [Fact]
        public async Task Should_throw_exception_for_version_mismatch_and_update()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var events = new[]
            {
                new EventData("Type1", new EnvelopeHeaders(), "1"),
                new EventData("Type2", new EnvelopeHeaders(), "2")
            };

            await Sut.AppendAsync(Guid.NewGuid(), streamName, events);

            await Assert.ThrowsAsync<WrongEventVersionException>(() => Sut.AppendAsync(Guid.NewGuid(), streamName, 0, events));
        }

        [Fact]
        public async Task Should_append_events()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var events = new[]
            {
                new EventData("Type1", new EnvelopeHeaders(), "1"),
                new EventData("Type2", new EnvelopeHeaders(), "2")
            };

            await Sut.AppendAsync(Guid.NewGuid(), streamName, events);

            var readEvents1 = await QueryAsync(streamName);
            var readEvents2 = await QueryWithCallbackAsync(streamName);

            var expected = new[]
            {
                new StoredEvent(streamName, "Position", 0, events[0]),
                new StoredEvent(streamName, "Position", 1, events[1])
            };

            ShouldBeEquivalentTo(readEvents1, expected);
            ShouldBeEquivalentTo(readEvents2, expected);
        }

        [Fact]
        public async Task Should_subscribe_to_events()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var events = new[]
            {
                new EventData("Type1", new EnvelopeHeaders(), "1"),
                new EventData("Type2", new EnvelopeHeaders(), "2")
            };

            var readEvents = await QueryWithSubscriptionAsync(streamName, async () =>
            {
                await Sut.AppendAsync(Guid.NewGuid(), streamName, events);
            });

            var expected = new[]
            {
                new StoredEvent(streamName, "Position", 0, events[0]),
                new StoredEvent(streamName, "Position", 1, events[1])
            };

            ShouldBeEquivalentTo(readEvents, expected);
        }

        [Fact]
        public async Task Should_subscribe_to_next_events()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var events1 = new[]
            {
                new EventData("Type1", new EnvelopeHeaders(), "1"),
                new EventData("Type2", new EnvelopeHeaders(), "2")
            };

            await QueryWithSubscriptionAsync(streamName, async () =>
            {
                await Sut.AppendAsync(Guid.NewGuid(), streamName, events1);
            });

            var events2 = new[]
            {
                new EventData("Type1", new EnvelopeHeaders(), "1"),
                new EventData("Type2", new EnvelopeHeaders(), "2")
            };

            var readEventsFromPosition = await QueryWithSubscriptionAsync(streamName, async () =>
            {
                await Sut.AppendAsync(Guid.NewGuid(), streamName, events2);
            });

            var expectedFromPosition = new[]
            {
                new StoredEvent(streamName, "Position", 2, events2[0]),
                new StoredEvent(streamName, "Position", 3, events2[1])
            };

            var readEventsFromBeginning = await QueryWithSubscriptionAsync(streamName, fromBeginning: true);

            var expectedFromBeginning = new[]
            {
                new StoredEvent(streamName, "Position", 0, events1[0]),
                new StoredEvent(streamName, "Position", 1, events1[1]),
                new StoredEvent(streamName, "Position", 2, events2[0]),
                new StoredEvent(streamName, "Position", 3, events2[1])
            };

            ShouldBeEquivalentTo(readEventsFromPosition, expectedFromPosition);

            ShouldBeEquivalentTo(readEventsFromBeginning, expectedFromBeginning);
        }

        [Fact]
        public async Task Should_read_events_from_offset()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var events = new[]
            {
                new EventData("Type1", new EnvelopeHeaders(), "1"),
                new EventData("Type2", new EnvelopeHeaders(), "2")
            };

            await Sut.AppendAsync(Guid.NewGuid(), streamName, events);

            var firstRead = await QueryAsync(streamName);

            var readEvents1 = await QueryAsync(streamName, 1);
            var readEvents2 = await QueryWithCallbackAsync(streamName, firstRead[0].EventPosition);

            var expected = new[]
            {
                new StoredEvent(streamName, "Position", 1, events[1])
            };

            ShouldBeEquivalentTo(readEvents1, expected);
            ShouldBeEquivalentTo(readEvents2, expected);
        }

        [Fact]
        public async Task Should_delete_stream()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var events = new[]
            {
                new EventData("Type1", new EnvelopeHeaders(), "1"),
                new EventData("Type2", new EnvelopeHeaders(), "2")
            };

            await Sut.AppendAsync(Guid.NewGuid(), streamName, events);

            await Sut.DeleteStreamAsync(streamName);

            var readEvents = await QueryAsync(streamName);

            Assert.Empty(readEvents);
        }

        [Fact]
        public async Task Should_query_events_by_property()
        {
            var keyed1 = new EnvelopeHeaders();
            var keyed2 = new EnvelopeHeaders();

            keyed1.Add("key", Guid.NewGuid().ToString());
            keyed2.Add("key", Guid.NewGuid().ToString());

            var streamName1 = $"test-{Guid.NewGuid()}";
            var streamName2 = $"test-{Guid.NewGuid()}";

            var events1 = new[]
            {
                new EventData("Type1", keyed1, "1"),
                new EventData("Type2", keyed2, "2")
            };

            var events2 = new[]
            {
                new EventData("Type3", keyed2, "3"),
                new EventData("Type4", keyed1, "4")
            };

            await Sut.CreateIndexAsync("key");

            await Sut.AppendAsync(Guid.NewGuid(), streamName1, events1);
            await Sut.AppendAsync(Guid.NewGuid(), streamName2, events2);

            var readEvents = await QueryWithFilterAsync("key", keyed2["key"].ToString());

            var expected = new[]
            {
                new StoredEvent(streamName1, "Position", 1, events1[1]),
                new StoredEvent(streamName2, "Position", 0, events2[0])
            };

            ShouldBeEquivalentTo(readEvents, expected);
        }

        private Task<IReadOnlyList<StoredEvent>> QueryAsync(string streamName, long position = EtagVersion.Any)
        {
            return Sut.QueryAsync(streamName, position);
        }

        private async Task<IReadOnlyList<StoredEvent>?> QueryWithFilterAsync(string property, object value)
        {
            using (var cts = new CancellationTokenSource(30000))
            {
                while (!cts.IsCancellationRequested)
                {
                    var readEvents = new List<StoredEvent>();

                    await Sut.QueryAsync(x => { readEvents.Add(x); return TaskHelper.Done; }, property, value, null, cts.Token);

                    await Task.Delay(500, cts.Token);

                    if (readEvents.Count > 0)
                    {
                        return readEvents;
                    }
                }

                cts.Token.ThrowIfCancellationRequested();

                return null;
            }
        }

        private async Task<IReadOnlyList<StoredEvent>?> QueryWithCallbackAsync(string? streamFilter = null, string? position = null)
        {
            using (var cts = new CancellationTokenSource(30000))
            {
                while (!cts.IsCancellationRequested)
                {
                    var readEvents = new List<StoredEvent>();

                    await Sut.QueryAsync(x => { readEvents.Add(x); return TaskHelper.Done; }, streamFilter, position, cts.Token);

                    await Task.Delay(500, cts.Token);

                    if (readEvents.Count > 0)
                    {
                        return readEvents;
                    }
                }

                cts.Token.ThrowIfCancellationRequested();

                return null;
            }
        }

        private async Task<IReadOnlyList<StoredEvent>?> QueryWithSubscriptionAsync(string streamFilter, Func<Task>? action = null, bool fromBeginning = false)
        {
            var subscriber = new EventSubscriber();

            IEventSubscription? subscription = null;
            try
            {
                subscription = Sut.CreateSubscription(subscriber, streamFilter, fromBeginning ? null : subscriptionPosition);

                if (action != null)
                {
                    await action();
                }

                using (var cts = new CancellationTokenSource(30000))
                {
                    while (!cts.IsCancellationRequested)
                    {
                        subscription.WakeUp();

                        await Task.Delay(500, cts.Token);

                        if (subscriber.Events.Count > 0)
                        {
                            subscriptionPosition = subscriber.LastPosition;

                            return subscriber.Events;
                        }
                    }

                    cts.Token.ThrowIfCancellationRequested();

                    return null;
                }
            }
            finally
            {
                if (subscription != null)
                {
                    await subscription.StopAsync();
                }
            }
        }

        private static void ShouldBeEquivalentTo(IEnumerable<StoredEvent>? actual, params StoredEvent[] expected)
        {
            var actualArray = actual.Select(x => new StoredEvent(x.StreamName, "Position", x.EventStreamNumber, x.Data)).ToArray();

            actualArray.Should().BeEquivalentTo(expected);
        }
    }
}
