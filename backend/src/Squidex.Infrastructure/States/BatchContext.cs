// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Infrastructure.States;

public sealed class BatchContext<T> : IBatchContext<T>
{
    private static readonly List<Envelope<IEvent>> EmptyStream = new List<Envelope<IEvent>>();
    private readonly Type owner;
    private readonly IEventFormatter eventFormatter;
    private readonly IEventStore eventStore;
    private readonly IEventStreamNames eventStreamNames;
    private readonly ISnapshotStore<T> snapshotStore;
    private readonly Dictionary<DomainId, (long, List<Envelope<IEvent>>)> events = new Dictionary<DomainId, (long, List<Envelope<IEvent>>)>();
    private Dictionary<DomainId, SnapshotWriteJob<T>>? snapshots;

    public ISnapshotStore<T> Snapshots => snapshotStore;

    internal BatchContext(
        Type owner,
        IEventFormatter eventFormatter,
        IEventStore eventStore,
        IEventStreamNames eventStreamNames,
        ISnapshotStore<T> snapshotStore)
    {
        this.owner = owner;
        this.eventFormatter = eventFormatter;
        this.eventStore = eventStore;
        this.eventStreamNames = eventStreamNames;
        this.snapshotStore = snapshotStore;
    }

    internal void Add(DomainId key, T snapshot, long version)
    {
        snapshots ??= new ();

        if (!snapshots.TryGetValue(key, out var existing) || existing.NewVersion < version)
        {
            snapshots[key] = new SnapshotWriteJob<T>(key, snapshot, version);
        }
    }

    public async Task LoadAsync(IEnumerable<DomainId> ids)
    {
        var streamNames = ids.ToDictionary(x => x, x => eventStreamNames.GetStreamName(owner, x.ToString()));

        if (streamNames.Count == 0)
        {
            return;
        }

        var streams = await eventStore.QueryManyAsync(streamNames.Values);

        foreach (var (id, streamName) in streamNames)
        {
            if (streams.TryGetValue(streamName, out var data))
            {
                var stream = data.Select(eventFormatter.ParseIfKnown).NotNull().ToList();

                events[id] = (data.Count - 1, stream);
            }
            else
            {
                events[id] = (EtagVersion.Empty, EmptyStream);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await CommitAsync();
    }

    public Task CommitAsync()
    {
        var current = Interlocked.Exchange(ref snapshots, null!);

        if (current == null || current.Count == 0)
        {
            return Task.CompletedTask;
        }

        return snapshotStore.WriteManyAsync(current.Values);
    }

    public IPersistence<T> WithEventSourcing(Type owner, DomainId key, HandleEvent? applyEvent)
    {
        var (version, streamEvents) = events[key];

        return new BatchPersistence<T>(key, this, version, streamEvents, applyEvent);
    }

    public IPersistence<T> WithSnapshotsAndEventSourcing(Type owner, DomainId key, HandleSnapshot<T>? applySnapshot, HandleEvent? applyEvent)
    {
        var (version, streamEvents) = events[key];

        return new BatchPersistence<T>(key, this, version, streamEvents, applyEvent);
    }

    public IPersistence<T> WithSnapshots(Type owner, DomainId key, HandleSnapshot<T>? applySnapshot)
    {
        throw new NotSupportedException();
    }
}
