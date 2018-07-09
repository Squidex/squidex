// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.States
{
    public interface IStore<TKey>
    {
        IPersistence WithEventSourcing(Type owner, TKey key, Func<Envelope<IEvent>, Task> applyEvent);

        IPersistence<TState> WithSnapshots<TState>(Type owner, TKey key, Func<TState, Task> applySnapshot);

        IPersistence<TState> WithSnapshotsAndEventSourcing<TState>(Type owner, TKey key, Func<TState, Task> applySnapshot, Func<Envelope<IEvent>, Task> applyEvent);

        ISnapshotStore<TState, TKey> GetSnapshotStore<TState>();

        Task ClearSnapshotsAsync<TState>();
    }
}
