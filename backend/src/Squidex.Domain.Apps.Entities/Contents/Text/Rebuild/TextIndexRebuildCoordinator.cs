// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

// The text indexer and the rebuild jobs both run on the worker node, therefore the coordination can happen in memory.
public sealed class TextIndexRebuildCoordinator
{
    private readonly AsyncLock syncLock = new AsyncLock();

    // The number of events per app that the text indexer has skipped, because the app is rebuilt.
    private readonly Dictionary<DomainId, long> skippedEvents = [];

    public async Task<TextIndexBatch> BeginBatchAsync(
        CancellationToken ct = default)
    {
        // The rebuild only holds the lock for short operations in memory, never for the rebuild itself.
        var handle = await syncLock.EnterAsync(ct);

        return new TextIndexBatch(handle, skippedEvents);
    }

    public async Task TakeOverAsync(DomainId appId,
        CancellationToken ct = default)
    {
        using (await syncLock.EnterAsync(ct))
        {
            if (!skippedEvents.TryAdd(appId, 0))
            {
                throw new DomainException("The full text index of the app is already rebuilt.");
            }
        }
    }

    public async Task<long> GetSkippedEventsAsync(DomainId appId,
        CancellationToken ct = default)
    {
        using (await syncLock.EnterAsync(ct))
        {
            return skippedEvents.GetValueOrDefault(appId);
        }
    }

    public async Task<bool> TryHandBackAsync(DomainId appId, long expectedSkippedEvents,
        CancellationToken ct = default)
    {
        using (await syncLock.EnterAsync(ct))
        {
            // The text indexer has skipped events after the rebuild has read the event store, therefore it has to catch up again.
            if (skippedEvents.TryGetValue(appId, out var count) && count != expectedSkippedEvents)
            {
                return false;
            }

            skippedEvents.Remove(appId);
            return true;
        }
    }

    public async Task ReleaseAsync(DomainId appId)
    {
        using (await syncLock.EnterAsync())
        {
            skippedEvents.Remove(appId);
        }
    }
}
