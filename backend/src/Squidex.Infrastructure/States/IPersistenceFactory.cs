// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Infrastructure.States;

public delegate bool HandleEvent(Envelope<IEvent> @event);

public delegate void HandleSnapshot<in T>(T state, long version);

public interface IPersistenceFactory<T>
{
    ISnapshotStore<T> Snapshots { get; }

    IPersistence<T> WithEventSourcing(Type owner, DomainId id, HandleEvent? applyEvent);

    IPersistence<T> WithSnapshots(Type owner, DomainId id, HandleSnapshot<T>? applySnapshot);

    IPersistence<T> WithSnapshotsAndEventSourcing(Type owner, DomainId id, HandleSnapshot<T>? applySnapshot, HandleEvent? applyEvent);
}
