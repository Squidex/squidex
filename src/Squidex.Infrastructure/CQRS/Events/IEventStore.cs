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
        Task<IReadOnlyList<StoredEvent>> GetEventsAsync(string streamName);

        Task GetEventsAsync(Func<StoredEvent, Task> callback, CancellationToken cancellationToken, string streamFilter = null, string position = null);

        Task AppendEventsAsync(Guid commitId, string streamName, ICollection<EventData> events);

        Task AppendEventsAsync(Guid commitId, string streamName, int expectedVersion, ICollection<EventData> events);

        IEventSubscription CreateSubscription(IEventSubscriber subscriber, string streamFilter, string position = null);
    }
}
