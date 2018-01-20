// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.EventSourcing
{
    public interface IEventStore
    {
        Task<IReadOnlyList<StoredEvent>> GetEventsAsync(string streamName, long streamPosition = 0);

        Task GetEventsAsync(Func<StoredEvent, Task> callback, string streamFilter = null, string position = null, CancellationToken cancellationToken = default(CancellationToken));

        Task AppendEventsAsync(Guid commitId, string streamName, ICollection<EventData> events);

        Task AppendEventsAsync(Guid commitId, string streamName, long expectedVersion, ICollection<EventData> events);

        IEventSubscription CreateSubscription(IEventSubscriber subscriber, string streamFilter, string position = null);
    }
}
