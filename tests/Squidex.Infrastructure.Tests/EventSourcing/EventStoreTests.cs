// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    public abstract class EventStoreTests<T> where T : IEventStore
    {
        private readonly Lazy<T> sut;

        public sealed class EventSubscriber : IEventSubscriber
        {
            public List<StoredEvent> Events { get; } = new List<StoredEvent>();

            public Task OnErrorAsync(IEventSubscription subscription, Exception exception)
            {
                throw new NotSupportedException();
            }

            public Task OnEventAsync(IEventSubscription subscription, StoredEvent storedEvent)
            {
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

            var events = new EventData[]
            {
                new EventData("Type1", new EnvelopeHeaders(), "1"),
                new EventData("Type2", new EnvelopeHeaders(), "2"),
            };

            await Assert.ThrowsAsync<WrongEventVersionException>(() => Sut.AppendAsync(Guid.NewGuid(), streamName, 0, events));
        }

        [Fact]
        public async Task Should_throw_exception_for_version_mismatch_and_update()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var events = new EventData[]
            {
                new EventData("Type1", new EnvelopeHeaders(), "1"),
                new EventData("Type2", new EnvelopeHeaders(), "2"),
            };

            await Sut.AppendAsync(Guid.NewGuid(), streamName, events);

            await Assert.ThrowsAsync<WrongEventVersionException>(() => Sut.AppendAsync(Guid.NewGuid(), streamName, 0, events));
        }

        [Fact]
        public async Task Should_append_events()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var events = new EventData[]
            {
                new EventData("Type1", new EnvelopeHeaders(), "1"),
                new EventData("Type2", new EnvelopeHeaders(), "2"),
            };

            await Sut.AppendAsync(Guid.NewGuid(), streamName, events);

            var readEvents1 = await QueryAsync(streamName);
            var readEvents2 = await QueryWithCallbackAsync(streamName);

            var expected = new StoredEvent[]
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

            var events = new EventData[]
            {
                new EventData("Type1", new EnvelopeHeaders(), "1"),
                new EventData("Type2", new EnvelopeHeaders(), "2"),
            };

            var subscriber = new EventSubscriber();

            IEventSubscription subscription = null;
            try
            {
                subscription = Sut.CreateSubscription(subscriber, streamName);

                await Sut.AppendAsync(Guid.NewGuid(), streamName, events);

                subscription.WakeUp();

                await Task.Delay(SubscriptionDelayInMs);

                var expected = new StoredEvent[]
                {
                    new StoredEvent(streamName, "Position", 0, events[0]),
                    new StoredEvent(streamName, "Position", 1, events[1])
                };

                ShouldBeEquivalentTo(subscriber.Events, expected);
            }
            finally
            {
                await subscription.StopAsync();
            }
        }

        [Fact]
        public async Task Should_read_events_from_offset()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var events = new EventData[]
            {
                new EventData("Type1", new EnvelopeHeaders(), "1"),
                new EventData("Type2", new EnvelopeHeaders(), "2"),
            };

            await Sut.AppendAsync(Guid.NewGuid(), streamName, events);

            var firstRead = await QueryAsync(streamName);

            var readEvents1 = await QueryAsync(streamName, 1);
            var readEvents2 = await QueryWithCallbackAsync(streamName, firstRead[0].EventPosition);

            var expected = new StoredEvent[]
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

            var events = new EventData[]
            {
                new EventData("Type1", new EnvelopeHeaders(), "1"),
                new EventData("Type2", new EnvelopeHeaders(), "2"),
            };

            await Sut.AppendAsync(Guid.NewGuid(), streamName, events);
            await Sut.DeleteStreamAsync(streamName);

            var readEvents1 = await QueryAsync(streamName);
            var readEvents2 = await QueryWithCallbackAsync(streamName);

            Assert.Empty(readEvents1);
            Assert.Empty(readEvents2);
        }

        [Fact]
        public async Task Should_query_events_by_property()
        {
            var keyed1 = new EnvelopeHeaders();
            var keyed2 = new EnvelopeHeaders();

            keyed1.Add("key", "1");
            keyed2.Add("key", "2");

            var streamName1 = $"test-{Guid.NewGuid()}";
            var streamName2 = $"test-{Guid.NewGuid()}";

            var events1 = new EventData[]
            {
                new EventData("Type1", keyed1, "1"),
                new EventData("Type2", keyed2, "2"),
            };

            var events2 = new EventData[]
            {
                new EventData("Type3", keyed2, "3"),
                new EventData("Type4", keyed1, "4"),
            };

            await Sut.CreateIndexAsync("key");

            await Sut.AppendAsync(Guid.NewGuid(), streamName1, events1);
            await Sut.AppendAsync(Guid.NewGuid(), streamName2, events2);

            var readEvents = await QueryWithFilterAsync("key", "2");

            var expected = new StoredEvent[]
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

        private async Task<IReadOnlyList<StoredEvent>> QueryWithFilterAsync(string property, object value)
        {
            var readEvents = new List<StoredEvent>();

            await Sut.QueryAsync(x => { readEvents.Add(x); return TaskHelper.Done; }, property, value);

            return readEvents;
        }

        private async Task<IReadOnlyList<StoredEvent>> QueryWithCallbackAsync(string streamFilter = null, string position = null)
        {
            var readEvents = new List<StoredEvent>();

            await Sut.QueryAsync(x => { readEvents.Add(x); return TaskHelper.Done; }, streamFilter, position);

            return readEvents;
        }

        private void ShouldBeEquivalentTo(IEnumerable<StoredEvent> actual, params StoredEvent[] expected)
        {
            var actualArray = actual.Select(x => new StoredEvent(x.StreamName, "Position", x.EventStreamNumber, x.Data)).ToArray();

            actualArray.Should().BeEquivalentTo(expected);
        }
    }
}
