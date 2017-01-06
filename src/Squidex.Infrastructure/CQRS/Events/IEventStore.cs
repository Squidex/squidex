// ==========================================================================
//  IEventStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
{
    public interface IEventStore
    {
        IObservable<EventData> GetEventsAsync();

        IObservable<EventData> GetEventsAsync(string streamName);

        Task AppendEventsAsync(Guid commitId, string streamName, int expectedVersion, IEnumerable<EventData> events);
    }
}
