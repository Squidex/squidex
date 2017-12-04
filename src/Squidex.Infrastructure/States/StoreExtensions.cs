// ==========================================================================
//  StoreExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.States
{
    public static class StoreExtensions
    {
        public static IPersistence<object> WithEventSourcing<TOwner>(this IStore store, string key, Action<Envelope<IEvent>> applyEvent)
        {
            return store.WithEventSourcing<TOwner>(key, x =>
            {
                applyEvent(x);

                return TaskHelper.Done;
            });
        }

        public static IPersistence<TState> WithSnapshots<TOwner, TState>(this IStore store, string key, Action<TState> applySnapshot)
        {
            return store.WithSnapshots<TOwner, TState>(key, x =>
            {
                applySnapshot(x);

                return TaskHelper.Done;
            });
        }

        public static IPersistence<TState> WithSnapshotsAndEventSourcing<TOwner, TState>(this IStore store, string key, Action<TState> applySnapshot, Action<Envelope<IEvent>> applyEvent)
        {
            return store.WithSnapshotsAndEventSourcing<TOwner, TState>(key, x =>
            {
                applySnapshot(x);

                return TaskHelper.Done;
            }, x =>
            {
                applyEvent(x);

                return TaskHelper.Done;
            });
        }
    }
}
