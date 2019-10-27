// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.States
{
    public delegate void HandleEvent(Envelope<IEvent> @event);

    public delegate void HandleSnapshot<in T>(T state);

    public interface IStore<in TKey>
    {
        IPersistence WithEventSourcing(Type owner, TKey key, HandleEvent? applyEvent);

        IPersistence<TState> WithSnapshots<TState>(Type owner, TKey key, HandleSnapshot<TState>? applySnapshot);

        IPersistence<TState> WithSnapshotsAndEventSourcing<TState>(Type owner, TKey key, HandleSnapshot<TState>? applySnapshot, HandleEvent? applyEvent);

        ISnapshotStore<TState, TKey> GetSnapshotStore<TState>();
    }
}
