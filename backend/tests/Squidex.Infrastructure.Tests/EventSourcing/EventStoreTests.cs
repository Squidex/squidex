// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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
                throw exception;
            }

            public Task OnEventAsync(IEventSubscription subscription, StoredEvent storedEvent)
            {
                LastPosition = storedEvent.EventPosition;

                Events.Add(storedEvent);

                return Task.CompletedTask;
            }
        }

        protected T Sut
        {
            get => sut.Value;
        }

        protected abstract int SubscriptionDelayInMs { get; }

        protected EventStoreTests()
        {
#pragma warning disable MA0056 // Do not call overridable members in constructor
            sut = new Lazy<T>(CreateStore);
#pragma warning restore MA0056 // Do not call overridable members in constructor
        }

        public abstract T CreateStore();

        [Fact]
        public async Task Should_throw_exception_for_version_mismatch()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var commit = new[]
            {
                CreateEventData(1),
                CreateEventData(2)
            };

            await Assert.ThrowsAsync<WrongEventVersionException>(() => Sut.AppendAsync(Guid.NewGuid(), streamName, 0, commit));
        }

        [Fact]
        public async Task Should_throw_exception_for_version_mismatch_and_update()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var commit = new[]
            {
                CreateEventData(1),
                CreateEventData(2)
            };

            await Sut.AppendAsync(Guid.NewGuid(), streamName, commit);

            await Assert.ThrowsAsync<WrongEventVersionException>(() => Sut.AppendAsync(Guid.NewGuid(), streamName, 0, commit));
        }

        [Fact]
        public async Task Should_append_events()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var commit1 = new[]
            {
                CreateEventData(1),
                CreateEventData(2)
            };

            var commit2 = new[]
            {
                CreateEventData(1),
                CreateEventData(2)
            };

            await Sut.AppendAsync(Guid.NewGuid(), streamName, commit1);
            await Sut.AppendAsync(Guid.NewGuid(), streamName, commit2);

            var readEvents1 = await QueryAsync(streamName);
            var readEvents2 = await QueryAllAsync(streamName);

            var expected = new[]
            {
                new StoredEvent(streamName, "Position", 0, commit1[0]),
                new StoredEvent(streamName, "Position", 1, commit1[1]),
                new StoredEvent(streamName, "Position", 2, commit2[0]),
                new StoredEvent(streamName, "Position", 3, commit2[1])
            };

            ShouldBeEquivalentTo(readEvents1, expected);
            ShouldBeEquivalentTo(readEvents2, expected);
        }

        [Fact]
        public async Task Should_append_events_unsafe()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var commit1 = new[]
            {
                CreateEventData(1),
                CreateEventData(2)
            };

            await Sut.AppendUnsafeAsync(new List<EventCommit>
            {
                new EventCommit(Guid.NewGuid(), streamName, -1, commit1)
            });

            var readEvents1 = await QueryAsync(streamName);
            var readEvents2 = await QueryAllAsync(streamName);

            var expected = new[]
            {
                new StoredEvent(streamName, "Position", 0, commit1[0]),
                new StoredEvent(streamName, "Position", 1, commit1[1])
            };

            ShouldBeEquivalentTo(readEvents1, expected);
            ShouldBeEquivalentTo(readEvents2, expected);
        }

        [Fact]
        public async Task Should_subscribe_to_events()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var commit1 = new[]
            {
                CreateEventData(1),
                CreateEventData(2)
            };

            var readEvents = await QueryWithSubscriptionAsync(streamName, async () =>
            {
                await Sut.AppendAsync(Guid.NewGuid(), streamName, commit1);
            });

            var expected = new[]
            {
                new StoredEvent(streamName, "Position", 0, commit1[0]),
                new StoredEvent(streamName, "Position", 1, commit1[1])
            };

            ShouldBeEquivalentTo(readEvents, expected);
        }

        [Fact]
        public async Task Should_subscribe_to_next_events()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var commit1 = new[]
            {
                CreateEventData(1),
                CreateEventData(2)
            };

            // Append and read in parallel.
            await QueryWithSubscriptionAsync(streamName, async () =>
            {
                await Sut.AppendAsync(Guid.NewGuid(), streamName, commit1);
            });

            var commit2 = new[]
            {
                CreateEventData(1),
                CreateEventData(2)
            };

            // Append and read in parallel.
            var readEventsFromPosition = await QueryWithSubscriptionAsync(streamName, async () =>
            {
                await Sut.AppendAsync(Guid.NewGuid(), streamName, commit2);
            });

            var expectedFromPosition = new[]
            {
                new StoredEvent(streamName, "Position", 2, commit2[0]),
                new StoredEvent(streamName, "Position", 3, commit2[1])
            };

            var readEventsFromBeginning = await QueryWithSubscriptionAsync(streamName, fromBeginning: true);

            var expectedFromBeginning = new[]
            {
                new StoredEvent(streamName, "Position", 0, commit1[0]),
                new StoredEvent(streamName, "Position", 1, commit1[1]),
                new StoredEvent(streamName, "Position", 2, commit2[0]),
                new StoredEvent(streamName, "Position", 3, commit2[1])
            };

            ShouldBeEquivalentTo(readEventsFromPosition?.TakeLast(2), expectedFromPosition);
            ShouldBeEquivalentTo(readEventsFromBeginning?.TakeLast(4), expectedFromBeginning);
        }

        [Fact]
        public async Task Should_read_events_from_offset()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var commit = new[]
            {
                CreateEventData(1),
                CreateEventData(2)
            };

            await Sut.AppendAsync(Guid.NewGuid(), streamName, commit);

            var firstRead = await QueryAsync(streamName);

            var readEvents1 = await QueryAsync(streamName, 1);
            var readEvents2 = await QueryAllAsync(streamName, firstRead[0].EventPosition);

            var expected = new[]
            {
                new StoredEvent(streamName, "Position", 1, commit[1])
            };

            ShouldBeEquivalentTo(readEvents1, expected);
            ShouldBeEquivalentTo(readEvents2, expected);
        }

        [Fact]
        public async Task Should_read_multiple_streams()
        {
            var streamName1 = $"test-{Guid.NewGuid()}";
            var streamName2 = $"test-{Guid.NewGuid()}";

            var stream1Commit = new[]
            {
                CreateEventData(1),
                CreateEventData(2)
            };

            var stream2Commit = new[]
            {
                CreateEventData(3),
                CreateEventData(4)
            };

            await Sut.AppendAsync(Guid.NewGuid(), streamName1, stream1Commit);
            await Sut.AppendAsync(Guid.NewGuid(), streamName2, stream2Commit);

            var readEvents = await Sut.QueryManyAsync(new[] { streamName1, streamName2 });

            var expected1 = new[]
            {
                new StoredEvent(streamName1, "Position", 0, stream1Commit[0]),
                new StoredEvent(streamName1, "Position", 1, stream1Commit[1])
            };

            var expected2 = new[]
            {
                new StoredEvent(streamName2, "Position", 0, stream2Commit[0]),
                new StoredEvent(streamName2, "Position", 1, stream2Commit[1])
            };

            ShouldBeEquivalentTo(readEvents[streamName1], expected1);
            ShouldBeEquivalentTo(readEvents[streamName2], expected2);
        }

        [Theory]
        [InlineData(5, 30)]
        [InlineData(5, 300)]
        public async Task Should_read_latest_events(int commitSize, int count)
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var events = new List<EventData>();

            for (var i = 0; i < count; i++)
            {
                events.Add(CreateEventData(i));
            }

            for (var i = 0; i < events.Count / commitSize; i++)
            {
                var commit = events.Skip(i * commitSize).Take(commitSize);

                await Sut.AppendAsync(Guid.NewGuid(), streamName, commit.ToArray());
            }

            var allExpected = events.Select((x, i) => new StoredEvent(streamName, "Position", i, events[i])).ToArray();

            var takeStep = count / 10;

            for (var take = 0; take < count; take += takeStep)
            {
                var expected = allExpected.TakeLast(take).ToArray();

                var readEvents = await Sut.QueryLatestAsync(streamName, take);

                ShouldBeEquivalentTo(readEvents, expected);
            }
        }

        [Theory]
        [InlineData(5, 30)]
        [InlineData(5, 300)]
        public async Task Should_read_reverse(int commitSize, int count)
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var events = new List<EventData>();

            for (var i = 0; i < count; i++)
            {
                events.Add(CreateEventData(i));
            }

            for (var i = 0; i < events.Count / commitSize; i++)
            {
                var commit = events.Skip(i * commitSize).Take(commitSize);

                await Sut.AppendAsync(Guid.NewGuid(), streamName, commit.ToArray());
            }

            var allExpected = events.Select((x, i) => new StoredEvent(streamName, "Position", i, events[i])).ToArray();

            var takeStep = count / 10;

            for (var take = 0; take < count; take += takeStep)
            {
                var expected = allExpected.Reverse().Take(take).ToArray();

                var readEvents = await Sut.QueryAllReverseAsync(streamName, default, take).ToArrayAsync();

                ShouldBeEquivalentTo(readEvents, expected);
            }
        }

        [Fact]
        public async Task Should_delete_by_filter()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var events = new[]
            {
                CreateEventData(1),
                CreateEventData(2)
            };

            await Sut.AppendAsync(Guid.NewGuid(), streamName, events);

            await Sut.DeleteAsync($"^{streamName.Substring(0, 10)}");

            var readEvents = await QueryAsync(streamName);

            Assert.Empty(readEvents);
        }

        [Fact]
        public async Task Should_delete_stream()
        {
            var streamName = $"test-{Guid.NewGuid()}";

            var events = new[]
            {
                CreateEventData(1),
                CreateEventData(2)
            };

            await Sut.AppendAsync(Guid.NewGuid(), streamName, events);

            await Sut.DeleteStreamAsync(streamName);

            var readEvents = await QueryAsync(streamName);

            Assert.Empty(readEvents);
        }

        private Task<IReadOnlyList<StoredEvent>> QueryAsync(string streamName, long position = EtagVersion.Any)
        {
            return Sut.QueryAsync(streamName, position);
        }

        private static EventData CreateEventData(int i)
        {
            return new EventData($"Type{i}", new EnvelopeHeaders(), i.ToString(CultureInfo.InvariantCulture));
        }

        private async Task<IReadOnlyList<StoredEvent>?> QueryAllAsync(string? streamFilter = null, string? position = null)
        {
            var readEvents = new List<StoredEvent>();

            await foreach (var storedEvent in Sut.QueryAllAsync(streamFilter, position))
            {
                readEvents.Add(storedEvent);
            }

            return readEvents;
        }

        private async Task<IReadOnlyList<StoredEvent>?> QueryWithSubscriptionAsync(string streamFilter, Func<Task>? subscriptionRunning = null, bool fromBeginning = false)
        {
            var subscriber = new EventSubscriber();

            IEventSubscription? subscription = null;
            try
            {
                subscription = Sut.CreateSubscription(subscriber, streamFilter, fromBeginning ? null : subscriptionPosition);

                if (subscriptionRunning != null)
                {
                    await subscriptionRunning();
                }

                using (var cts = new CancellationTokenSource(30000))
                {
                    while (!cts.IsCancellationRequested)
                    {
                        subscription.WakeUp();

                        await Task.Delay(2000, cts.Token);

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
                subscription?.Unsubscribe();
            }
        }

        private static void ShouldBeEquivalentTo(IEnumerable<StoredEvent>? actual, params StoredEvent[] expected)
        {
            actual.Should().BeEquivalentTo(expected, opts => opts.ComparingByMembers<StoredEvent>().Including(x => x.EventStreamNumber));
        }
    }
}
