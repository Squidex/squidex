// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
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

        async Task AppendUnsafeAsync(IEnumerable<EventCommit> commits)
        {
            foreach (var commit in commits)
            {
                await AppendAsync(commit.Id, commit.StreamName, commit.Offset, commit.Events);
            }
        }

        async Task<IReadOnlyDictionary<string, IReadOnlyList<StoredEvent>>> QueryManyAsync(IEnumerable<string> streamNames)
        {
            var result = new Dictionary<string, IReadOnlyList<StoredEvent>>();

            foreach (var streamName in streamNames)
            {
                result[streamName] = await QueryAsync(streamName);
            }

            return result;
        }
    }
}
