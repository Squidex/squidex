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
    private readonly Dictionary<DomainId, TextIndexRebuildScope> scopes = [];

    public async Task<TextIndexBatch> BeginBatchAsync(
        CancellationToken ct = default)
    {
        // The lock is only held for short operations of the rebuild, e.g. to take over an app, never for the rebuild itself.
        var handle = await syncLock.EnterAsync(ct);

        return new TextIndexBatch(handle, scopes);
    }

    public async Task TakeOverAsync(DomainId appId, DomainId? schemaId,
        CancellationToken ct = default)
    {
        using (await syncLock.EnterAsync(ct))
        {
            if (!scopes.TryAdd(appId, new TextIndexRebuildScope(schemaId)))
            {
                throw new DomainException("The full text index of the app is already rebuilt.");
            }
        }
    }

    public async Task<bool> IsRebuildingAsync(DomainId appId,
        CancellationToken ct = default)
    {
        using (await syncLock.EnterAsync(ct))
        {
            return scopes.ContainsKey(appId);
        }
    }

    public async Task<List<DomainId>> TakeSkippedAsync(DomainId appId,
        CancellationToken ct = default)
    {
        using (await syncLock.EnterAsync(ct))
        {
            return TakeSkipped(appId);
        }
    }

    public async Task<List<DomainId>> TryHandBackAsync(DomainId appId,
        CancellationToken ct = default)
    {
        using (await syncLock.EnterAsync(ct))
        {
            var skipped = TakeSkipped(appId);
            if (skipped.Count > 0)
            {
                return skipped;
            }

            // Nothing has been skipped since the last call, therefore the text indexer can index the app again.
            scopes.Remove(appId);
            return skipped;
        }
    }

    public async Task<List<DomainId>> ReleaseAsync(DomainId appId)
    {
        using (await syncLock.EnterAsync())
        {
            var skipped = TakeSkipped(appId);

            scopes.Remove(appId);
            return skipped;
        }
    }

    private List<DomainId> TakeSkipped(DomainId appId)
    {
        if (!scopes.TryGetValue(appId, out var scope))
        {
            return [];
        }

        var result = scope.SkippedContents.ToList();

        scope.SkippedContents.Clear();
        return result;
    }
}
