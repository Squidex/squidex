// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Infrastructure.EventSourcing;

public interface IEventStore
{
    Task<IReadOnlyList<StoredEvent>> QueryReverseAsync(string streamName, int take = int.MaxValue,
        CancellationToken ct = default);

    Task<IReadOnlyList<StoredEvent>> QueryAsync(string streamName, long streamPosition = 0,
        CancellationToken ct = default);

    IAsyncEnumerable<StoredEvent> QueryAllReverseAsync(string? streamFilter = null, Instant timestamp = default, int take = int.MaxValue,
        CancellationToken ct = default);

    IAsyncEnumerable<StoredEvent> QueryAllAsync(string? streamFilter = null, string? position = null, int take = int.MaxValue,
        CancellationToken ct = default);

    Task AppendAsync(Guid commitId, string streamName, ICollection<EventData> events,
        CancellationToken ct = default);

    Task AppendAsync(Guid commitId, string streamName, long expectedVersion, ICollection<EventData> events,
        CancellationToken ct = default);

    Task DeleteAsync(string streamFilter,
        CancellationToken ct = default);

    Task DeleteStreamAsync(string streamName,
        CancellationToken ct = default);

    IEventSubscription CreateSubscription(IEventSubscriber<StoredEvent> eventSubscriber, string? streamFilter = null, string? position = null);

    async Task AppendUnsafeAsync(IEnumerable<EventCommit> commits,
        CancellationToken ct = default)
    {
        foreach (var commit in commits)
        {
            await AppendAsync(commit.Id, commit.StreamName, commit.Offset, commit.Events, ct);
        }
    }

    async Task<IReadOnlyDictionary<string, IReadOnlyList<StoredEvent>>> QueryManyAsync(IEnumerable<string> streamNames,
        CancellationToken ct = default)
    {
        var result = new Dictionary<string, IReadOnlyList<StoredEvent>>();

        foreach (var streamName in streamNames)
        {
            result[streamName] = await QueryAsync(streamName, 0, ct);
        }

        return result;
    }
}
