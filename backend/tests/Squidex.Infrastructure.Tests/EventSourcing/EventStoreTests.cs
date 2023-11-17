// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;

namespace Squidex.Infrastructure.EventSourcing;

public abstract class EventStoreTests<T> where T : IEventStore
{
    private readonly Lazy<T> sut;
    private string subscriptionPosition;

    public sealed class EventSubscriber : IEventSubscriber<StoredEvent>
    {
        public List<StoredEvent> LastEvents { get; } = [];

        public string LastPosition { get; set; }

        public void Dispose()
        {
        }

        public void WakeUp()
        {
        }

        public ValueTask OnErrorAsync(IEventSubscription subscription, Exception exception)
        {
            throw exception;
        }

        public ValueTask OnNextAsync(IEventSubscription subscription, StoredEvent @event)
        {
            LastPosition = @event.EventPosition;
            LastEvents.Add(@event);
            return default;
        }
    }

    protected T Sut
    {
        get => sut.Value;
    }

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

        await Sut.AppendAsync(Guid.NewGuid(), streamName, EtagVersion.Any, commit);

        await Assert.ThrowsAsync<WrongEventVersionException>(() => Sut.AppendAsync(Guid.NewGuid(), streamName, 0, commit));
    }

    [Fact]
    public async Task Should_append_events()
    {
        var streamName = $"test-{Guid.NewGuid()}";
        var streamFilter = StreamFilter.Name(streamName);

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

        await Sut.AppendAsync(Guid.NewGuid(), streamName, EtagVersion.Any, commit1);
        await Sut.AppendAsync(Guid.NewGuid(), streamName, EtagVersion.Any, commit2);

        var readEvents1 = await QueryAsync(streamName);
        var readEvents2 = await QueryAllAsync(streamFilter);

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
        var streamFilter = StreamFilter.Name(streamName);

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
        var readEvents2 = await QueryAllAsync(streamFilter);

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
        var streamFilter = StreamFilter.Name(streamName);

        var commit1 = new[]
        {
            CreateEventData(1),
            CreateEventData(2)
        };

        var readEvents = await QueryWithSubscriptionAsync(streamFilter, async () =>
        {
            await Sut.AppendAsync(Guid.NewGuid(), streamName, EtagVersion.Any, commit1);
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
        var streamFilter = StreamFilter.Name(streamName);

        var commit1 = new[]
        {
            CreateEventData(1),
            CreateEventData(2)
        };

        // Append and read in parallel.
        await QueryWithSubscriptionAsync(streamFilter, async () =>
        {
            await Sut.AppendAsync(Guid.NewGuid(), streamName, EtagVersion.Any, commit1);
        });

        var commit2 = new[]
        {
            CreateEventData(1),
            CreateEventData(2)
        };

        // Append and read in parallel.
        var readEventsFromPosition = await QueryWithSubscriptionAsync(streamFilter, async () =>
        {
            await Sut.AppendAsync(Guid.NewGuid(), streamName, EtagVersion.Any, commit2);
        });

        var expectedFromPosition = new[]
        {
            new StoredEvent(streamName, "Position", 2, commit2[0]),
            new StoredEvent(streamName, "Position", 3, commit2[1])
        };

        var readEventsFromBeginning = await QueryWithSubscriptionAsync(streamFilter, fromBeginning: true);

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
    public async Task Should_subscribe_with_parallel_writes()
    {
        var streamName = $"test-{Guid.NewGuid()}";
        var streamFilter = StreamFilter.Prefix(streamName);

        var numTasks = 50;
        var numEvents = 100;

        // Append and read in parallel.
        var readEvents = await QueryWithSubscriptionAsync(streamFilter, async () =>
        {
            await Parallel.ForEachAsync(Enumerable.Range(0, numTasks), async (i, ct) =>
            {
                var fullStreamName = $"{streamName}-{Guid.NewGuid()}";

                for (var j = 0; j < numEvents; j++)
                {
                    var commit1 = new[]
                    {
                        CreateEventData(i * j)
                    };

#pragma warning disable MA0040 // Forward the CancellationToken parameter to methods that take one
                    await Sut.AppendAsync(Guid.NewGuid(), fullStreamName, EtagVersion.Any, commit1);
#pragma warning restore MA0040 // Forward the CancellationToken parameter to methods that take one
                }
            });
        });

        Assert.Equal(numEvents * numTasks, readEvents?.Count);
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

        await Sut.AppendAsync(Guid.NewGuid(), streamName1, EtagVersion.Any, stream1Commit);
        await Sut.AppendAsync(Guid.NewGuid(), streamName2, EtagVersion.Any, stream2Commit);

        var readEvents = await Sut.QueryAllAsync(StreamFilter.Name(streamName1, streamName2)).ToListAsync();

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

        ShouldBeEquivalentTo(readEvents.Where(x => x.StreamName == streamName1), expected1);
        ShouldBeEquivalentTo(readEvents.Where(x => x.StreamName == streamName2), expected2);
    }

    [Theory]
    [InlineData(1, 30)]
    [InlineData(5, 30)]
    [InlineData(5, 300)]
    [InlineData(5, 3000)]
    public async Task Should_query_events_from_offset(int commits, int count)
    {
        var streamName = $"test-{Guid.NewGuid()}";

        var eventsWritten = await AppendEventsAsync(streamName, count, commits);

        var readEvents0 = await QueryAsync(streamName);
        var readEvents1 = await QueryAsync(streamName, count - 2);
        var readEvents2 = await QueryAllAsync(default, readEvents0[^2].EventPosition);

        var expected = new[]
        {
            new StoredEvent(streamName, "Position", count - 1, eventsWritten[^1])
        };

        ShouldBeEquivalentTo(readEvents1, expected);
        ShouldBeEquivalentTo(readEvents2, expected);
    }

    [Theory]
    [InlineData(5, 30)]
    [InlineData(5, 300)]
    [InlineData(5, 3000)]
    public async Task Should_query_all_reverse_by_names(int commits, int count)
    {
        var streamName = $"test-{Guid.NewGuid()}";
        var streamFilter = StreamFilter.Name(streamName, "invalid");

        var eventsWritten = await AppendEventsAsync(streamName, count, commits);
        var eventsStored = eventsWritten.Select((x, i) => new StoredEvent(streamName, "Position", i, x)).ToArray();

        for (var take = 0; take < count; take += count / 10)
        {
            var eventsExpected = eventsStored.Reverse().Take(take).ToArray();
            var eventsQueried = await Sut.QueryAllReverseAsync(streamFilter, default, take).ToArrayAsync();

            ShouldBeEquivalentTo(eventsQueried, eventsExpected);
        }
    }

    [Theory]
    [InlineData(5, 30)]
    [InlineData(5, 300)]
    [InlineData(5, 3000)]
    public async Task Should_query_all_reverse_by_filter(int commits, int count)
    {
        var streamName = $"test-{Guid.NewGuid()}-suffix";
        var streamFilter = StreamFilter.Prefix(streamName[..^7], "invalid");

        var eventsWritten = await AppendEventsAsync(streamName, count, commits);
        var eventsStored = eventsWritten.Select((x, i) => new StoredEvent(streamName, "Position", i, x)).ToArray();

        for (var take = 0; take < count; take += count / 10)
        {
            var eventsExpected = eventsStored.Reverse().Take(take).ToArray();
            var eventsQueried = await Sut.QueryAllReverseAsync(streamFilter, default, take).ToArrayAsync();

            ShouldBeEquivalentTo(eventsQueried, eventsExpected);
        }
    }

    [Theory]
    [InlineData(5, 30)]
    [InlineData(5, 300)]
    [InlineData(5, 3000)]
    public async Task Should_read_all_reverse(int commits, int count)
    {
        var streamName = $"test-{Guid.NewGuid()}-suffix";
        var streamFilter = default(StreamFilter);

        await AppendEventsAsync(streamName, count, commits);

        for (var take = 0; take < count; take += count / 10)
        {
            var eventsQueried = await Sut.QueryAllReverseAsync(streamFilter, default, take).ToArrayAsync();

            Assert.Equal(take, eventsQueried.Length);
        }
    }

    [Fact]
    public async Task Should_delete_by_filter()
    {
        var streamName = $"test-{Guid.NewGuid()}";
        var streamFilter = StreamFilter.Prefix($"{streamName[..10]}");

        await AppendEventsAsync(streamName, 2, 1);

        IReadOnlyList<StoredEvent>? readEvents = null;

        for (var i = 0; i < 5; i++)
        {
            await Sut.DeleteAsync(streamFilter);

            readEvents = await QueryAsync(streamName);

            if (readEvents.Count == 0)
            {
                break;
            }

            // Get event store needs a little bit of time for the projections.
            await Task.Delay(1000);
        }

        Assert.Empty(readEvents!);
    }

    [Fact]
    public async Task Should_delete_by_name()
    {
        var streamName = $"test-{Guid.NewGuid()}";
        var streamFilter = StreamFilter.Name(streamName);

        await AppendEventsAsync(streamName, 2, 1);

        IReadOnlyList<StoredEvent>? readEvents = null;

        for (var i = 0; i < 5; i++)
        {
            await Sut.DeleteAsync(streamFilter);

            readEvents = await QueryAsync(streamName);

            if (readEvents.Count == 0)
            {
                break;
            }

            // Get event store needs a little bit of time for the projections.
            await Task.Delay(1000);
        }

        Assert.Empty(readEvents!);
    }

    private async Task<IReadOnlyList<StoredEvent>> QueryAsync(string streamName, long position = EtagVersion.Any)
    {
        return await Sut.QueryStreamAsync(streamName, position);
    }

    private async Task<IReadOnlyList<StoredEvent>?> QueryAllAsync(StreamFilter filter, string? position = null)
    {
        return await Sut.QueryAllAsync(filter, position).ToListAsync();
    }

    private static EventData CreateEventData(int i)
    {
        var headers = new EnvelopeHeaders
        {
            [CommonHeaders.EventId] = Guid.NewGuid().ToString()
        };

        return new EventData($"Type{i}", headers, i.ToString(CultureInfo.InvariantCulture));
    }

    private async Task<IReadOnlyList<StoredEvent>?> QueryWithSubscriptionAsync(StreamFilter streamFilter,
        Func<Task>? subscriptionRunning = null, bool fromBeginning = false)
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

                    if (subscriber.LastEvents.Count > 0)
                    {
                        subscriptionPosition = subscriber.LastPosition;

                        return subscriber.LastEvents;
                    }
                }

                cts.Token.ThrowIfCancellationRequested();

                return null;
            }
        }
        finally
        {
            subscription?.Dispose();
        }
    }

    private async Task<List<EventData>> AppendEventsAsync(string streamName, int count, int commits = 1)
    {
        var events = new List<EventData>();

        for (var i = 0; i < count; i++)
        {
            events.Add(CreateEventData(i));
        }

        for (var i = 0; i < events.Count / commits; i++)
        {
            var commit = events.Skip(i * commits).Take(commits).ToArray();

            await Sut.AppendAsync(Guid.NewGuid(), streamName, EtagVersion.Any, commit);
        }

        return events;
    }

    private static void ShouldBeEquivalentTo(IEnumerable<StoredEvent>? actual, params StoredEvent[] expected)
    {
        actual.Should().BeEquivalentTo(expected, opts => opts.ComparingByMembers<StoredEvent>().Excluding(x => x.EventPosition));
    }
}
