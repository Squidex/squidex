// ==========================================================================
//  IStore.cs
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
    public interface IStore
    {
        IPersistence<object> WithEventSourcing<TOwner>(string key, Func<Envelope<IEvent>, Task> applyEvent);

        IPersistence<TState> WithSnapshots<TOwner, TState>(string key, Func<TState, Task> applySnapshot);

        IPersistence<TState> WithSnapshotsAndEventSourcing<TOwner, TState>(string key, Func<TState, Task> applySnapshot, Func<Envelope<IEvent>, Task> applyEvent);
    }
}
