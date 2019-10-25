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
    internal sealed class Persistence<TKey> : Persistence<None, TKey>, IPersistence where TKey : notnull
    {
        public Persistence(TKey ownerKey, Type ownerType,
            IEventStore eventStore,
            IEventEnricher<TKey> eventEnricher,
            IEventDataFormatter eventDataFormatter,
            ISnapshotStore<None, TKey> snapshotStore,
            IStreamNameResolver streamNameResolver,
            HandleEvent? applyEvent)
            : base(ownerKey, ownerType, eventStore, eventEnricher, eventDataFormatter, snapshotStore, streamNameResolver, PersistenceMode.EventSourcing, null, applyEvent)
        {
        }
    }
}
