// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.States;

internal sealed class BatchPersistence<T> : IPersistence<T>
{
    private readonly DomainId ownerKey;
    private readonly BatchContext<T> context;
    private readonly IReadOnlyList<Envelope<IEvent>> events;
    private readonly HandleEvent? applyEvent;

    public long Version { get; }

    public bool IsSnapshotStale => false;

    internal BatchPersistence(DomainId ownerKey, BatchContext<T> context, long version, IReadOnlyList<Envelope<IEvent>> events,
        HandleEvent? applyEvent)
    {
        this.ownerKey = ownerKey;
        this.context = context;
        this.events = events;
        this.applyEvent = applyEvent;

        Version = version;
    }

    public Task DeleteAsync(
        CancellationToken ct = default)
    {
        throw new NotSupportedException();
    }

    public Task WriteEventsAsync(IReadOnlyList<Envelope<IEvent>> events,
        CancellationToken ct = default)
    {
        throw new NotSupportedException();
    }

    public Task ReadAsync(long expectedVersion = -2,
        CancellationToken ct = default)
    {
        if (applyEvent != null)
        {
            foreach (var @event in events)
            {
                if (!applyEvent(@event))
                {
                    break;
                }
            }
        }

        if (expectedVersion > EtagVersion.Any && expectedVersion != Version)
        {
            if (Version == EtagVersion.Empty)
            {
                throw new DomainObjectNotFoundException(ownerKey.ToString()!);
            }
            else
            {
                throw new InconsistentStateException(Version, expectedVersion);
            }
        }

        return Task.CompletedTask;
    }

    public Task WriteSnapshotAsync(T state,
        CancellationToken ct = default)
    {
        context.Add(ownerKey, state, Version);

        return Task.CompletedTask;
    }
}
