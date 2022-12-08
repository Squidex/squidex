// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.State;

public sealed class InMemoryTextIndexerState : ITextIndexerState
{
    private readonly Dictionary<DomainId, TextContentState> states = new Dictionary<DomainId, TextContentState>();

    public Task ClearAsync(
        CancellationToken ct = default)
    {
        states.Clear();

        return Task.CompletedTask;
    }

    public Task<Dictionary<DomainId, TextContentState>> GetAsync(HashSet<DomainId> ids,
        CancellationToken ct = default)
    {
        Guard.NotNull(ids);

        var result = new Dictionary<DomainId, TextContentState>();

        foreach (var id in ids)
        {
            if (states.TryGetValue(id, out var state))
            {
                result.Add(id, state);
            }
        }

        return Task.FromResult(result);
    }

    public Task SetAsync(List<TextContentState> updates,
        CancellationToken ct = default)
    {
        Guard.NotNull(updates);

        foreach (var update in updates)
        {
            if (update.IsDeleted)
            {
                states.Remove(update.UniqueContentId);
            }
            else
            {
                states[update.UniqueContentId] = update;
            }
        }

        return Task.CompletedTask;
    }
}
