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
        Task<IReadOnlyList<StoredEvent>> QueryLatestAsync(string streamName, int count);

        Task<IReadOnlyList<StoredEvent>> QueryAsync(string streamName, long streamPosition = 0);

        Task QueryAsync(Func<StoredEvent, Task> callback, string? streamFilter = null, string? position = null, CancellationToken ct = default);

        Task AppendAsync(Guid commitId, string streamName, ICollection<EventData> events);

        Task AppendAsync(Guid commitId, string streamName, long expectedVersion, ICollection<EventData> events);

        Task DeleteStreamAsync(string streamName);

        IEventSubscription CreateSubscription(IEventSubscriber subscriber, string? streamFilter = null, string? position = null);
    }
}
