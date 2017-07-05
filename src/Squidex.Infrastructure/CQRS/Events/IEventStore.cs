// ==========================================================================
//  IEventStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
{
    public interface IEventStore
    {
        IObservable<StoredEvent> GetEventsAsync(string streamFilter = null, string position = null);

        Task GetEventsAsync(Func<StoredEvent, Task> callback, CancellationToken cancellationToken, string streamFilter = null, string position = null);

        Task AppendEventsAsync(Guid commitId, string streamName, int expectedVersion, ICollection<EventData> events);
    }
}
