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
        IPersistence WithEventSourcing(TKey key, Func<Envelope<IEvent>, Task> applyEvent);

        IPersistence<T> WithSnapshots<T>(TKey key, Func<T, Task> applySnapshot);

        IPersistence<T> WithSnapshotsAndEventSourcing<T>(TKey key, Func<T, Task> applySnapshot, Func<Envelope<IEvent>, Task> applyEvent);
    }
}
