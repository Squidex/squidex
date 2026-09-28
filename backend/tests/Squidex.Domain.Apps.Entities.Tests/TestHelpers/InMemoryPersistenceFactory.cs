// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.TestHelpers;

public sealed class InMemoryPersistenceFactory<T> : IPersistenceFactory<T> where T : class
{
    private readonly Dictionary<(Type, DomainId), (string Json, long Version)> snapshots = [];

    public ISnapshotStore<T> Snapshots => throw new NotSupportedException();

    public IPersistence<T> WithSnapshots(Type owner, DomainId id, HandleSnapshot<T>? applySnapshot)
    {
        return new Persistence(this, (owner, id), applySnapshot);
    }

    public IPersistence<T> WithEventSourcing(Type owner, DomainId id, HandleEvent? applyEvent)
    {
        throw new NotSupportedException();
    }

    public IPersistence<T> WithSnapshotsAndEventSourcing(Type owner, DomainId id, HandleSnapshot<T>? applySnapshot, HandleEvent? applyEvent)
    {
        throw new NotSupportedException();
    }

    private sealed class Persistence(InMemoryPersistenceFactory<T> factory, (Type, DomainId) key, HandleSnapshot<T>? applySnapshot) : IPersistence<T>
    {
        public long Version { get; private set; } = EtagVersion.Empty;

        public bool IsSnapshotStale => false;

        public Task DeleteAsync(
            CancellationToken ct = default)
        {
            lock (factory.snapshots)
            {
                factory.snapshots.Remove(key);
            }

            Version = EtagVersion.Empty;
            return Task.CompletedTask;
        }

        public Task ReadAsync(long expectedVersion = -2,
            CancellationToken ct = default)
        {
            lock (factory.snapshots)
            {
                if (!factory.snapshots.TryGetValue(key, out var snapshot))
                {
                    Version = EtagVersion.Empty;
                    return Task.CompletedTask;
                }

                // Serialize the state to simulate a real database and to verify the serialization.
                applySnapshot?.Invoke(TestUtils.DefaultSerializer.Deserialize<T>(snapshot.Json), snapshot.Version);

                Version = snapshot.Version;
            }

            return Task.CompletedTask;
        }

        public Task WriteSnapshotAsync(T state,
            CancellationToken ct = default)
        {
            lock (factory.snapshots)
            {
                var current = factory.snapshots.TryGetValue(key, out var snapshot) ? snapshot.Version : EtagVersion.Empty;

                if (current != Version)
                {
                    throw new InconsistentStateException(current, Version);
                }

                Version++;

                factory.snapshots[key] = (TestUtils.DefaultSerializer.Serialize(state), Version);
            }

            return Task.CompletedTask;
        }

        public Task WriteEventsAsync(IReadOnlyList<Envelope<IEvent>> events,
            CancellationToken ct = default)
        {
            throw new NotSupportedException();
        }
    }
}
