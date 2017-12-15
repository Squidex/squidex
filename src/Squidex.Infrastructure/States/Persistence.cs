// ==========================================================================
//  Persistence.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;

#pragma warning disable RECS0012 // 'if' statement can be re-written as 'switch' statement

namespace Squidex.Infrastructure.States
{
    internal sealed class Persistence<TOwner, TKey> : Persistence<TOwner, object, TKey>, IPersistence
    {
        public Persistence(TKey ownerKey,
            Action invalidate,
            Action failed,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            ISnapshotStore<object, TKey> snapshotStore,
            IStreamNameResolver streamNameResolver,
            Func<Envelope<IEvent>, Task> applyEvent)
            : base(ownerKey, invalidate, failed, eventStore, eventDataFormatter, snapshotStore, streamNameResolver, PersistenceMode.EventSourcing, null, applyEvent)
        {
        }
    }
}
