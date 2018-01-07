// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.States
{
    public static class StoreExtensions
    {
        public static IPersistence WithEventSourcing<TKey>(this IStore<TKey> store, TKey key, Action<Envelope<IEvent>> applyEvent)
        {
            return store.WithEventSourcing(key, applyEvent.ToAsync());
        }

        public static IPersistence<TState> WithSnapshots<TState, TKey>(this IStore<TKey> store, TKey key, Action<TState> applySnapshot)
        {
            return store.WithSnapshots(key, applySnapshot.ToAsync());
        }

        public static IPersistence<TState> WithSnapshotsAndEventSourcing<TState, TKey>(this IStore<TKey> store, TKey key, Action<TState> applySnapshot, Action<Envelope<IEvent>> applyEvent)
        {
            return store.WithSnapshotsAndEventSourcing(key, applySnapshot.ToAsync(), applyEvent.ToAsync());
        }
    }
}
