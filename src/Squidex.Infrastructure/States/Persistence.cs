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
