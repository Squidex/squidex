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
    internal sealed class Persistence<TKey> : Persistence<None, TKey>, IPersistence
    {
        public Persistence(TKey ownerKey, Type ownerType,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            ISnapshotStore<None, TKey> snapshotStore,
            IStreamNameResolver streamNameResolver,
            HandleEvent applyEvent)
            : base(ownerKey, ownerType, eventStore, eventDataFormatter, snapshotStore, streamNameResolver, PersistenceMode.EventSourcing, null, applyEvent)
        {
        }
    }
}
