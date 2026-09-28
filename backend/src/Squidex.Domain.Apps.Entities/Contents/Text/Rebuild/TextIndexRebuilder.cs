// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

public sealed class TextIndexRebuilder(
    IJsonSerializer serializer,
    ITextIndex textIndex,
    ITextIndexerState textIndexerState,
    TextIndexExtraction extraction,
    IEventStore eventStore,
    IEventFormatter eventFormatter)
    : ITextIndexRebuilder
{
    public async Task RebuildAsync(DomainId appId, IReadOnlyCollection<DomainId> contentIds,
        CancellationToken ct = default)
    {
        // Replay the full history without the persisted state to calculate the entries from scratch.
        var updates = new TextIndexUpdates([], new TextIndexCommands(serializer));

        foreach (var contentId in contentIds)
        {
            var streamFilter = StreamFilter.Name($"content-{DomainId.Combine(appId, contentId)}");

            // Stream the events, because the commands only keep the latest version of each stage.
            await foreach (var storedEvent in eventStore.QueryAllAsync(streamFilter, ct: ct))
            {
                var @event = eventFormatter.ParseIfKnown(storedEvent);
                if (@event != null)
                {
                    updates.On(@event);
                }
            }
        }

        updates.Commands.PrepareRebuild();

        // The text indexer does not write the entries of the contents, because it skips their events until the rebuild is completed.
        await updates.WriteAsync(textIndex, textIndexerState, extraction, ct);
    }
}
