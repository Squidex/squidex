// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.States
{
    public delegate bool HandleEvent(Envelope<IEvent> @event);

    public delegate void HandleSnapshot<in T>(T state, long version);

    public interface IPersistenceFactory<T>
    {
        IPersistence<T> WithEventSourcing(Type owner, DomainId id, HandleEvent? applyEvent);

        IPersistence<T> WithSnapshots(Type owner, DomainId id, HandleSnapshot<T>? applySnapshot);

        IPersistence<T> WithSnapshotsAndEventSourcing(Type owner, DomainId id, HandleSnapshot<T>? applySnapshot, HandleEvent? applyEvent);
    }
}