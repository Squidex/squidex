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

namespace Squidex.Infrastructure.States
{
    internal sealed class Persistence<TOwner, TKey> : Persistence<TOwner, object, TKey>, IPersistence
    {
        public Persistence(TKey ownerKey,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            ISnapshotStore<object, TKey> snapshotStore,
            IStreamNameResolver streamNameResolver,
            Func<Envelope<IEvent>, Task> applyEvent)
            : base(ownerKey, eventStore, eventDataFormatter, snapshotStore, streamNameResolver, PersistenceMode.EventSourcing, null, applyEvent)
        {
        }
    }
}
