// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Events;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class TextIndexingProcess(
    IJsonSerializer serializer,
    ITextIndex textIndex,
    ITextIndexerState textIndexerState,
    TextIndexExtraction extraction,
    TextIndexRebuildCoordinator coordinator)
    : IEventConsumer
{
    public int BatchSize => 1000;

    public int BatchDelay => 1000;

    public string Name => "TextIndexer6";

    public StreamFilter EventsFilter { get; } = StreamFilter.Prefix("content-");

    public ITextIndex TextIndex
    {
        get => textIndex;
    }

    public async Task ClearAsync()
    {
        await textIndex.ClearAsync();
        await textIndexerState.ClearAsync();
    }

    public async Task On(IEnumerable<Envelope<IEvent>> events)
    {
        // The rebuild jobs only hold the lock for short operations in memory, but they cannot hand back while the batch is processed.
        using var batch = await coordinator.BeginBatchAsync();

        var indexEvents = new List<Envelope<IEvent>>();

        foreach (var @event in events)
        {
            // The rebuild job indexes the skipped contents from the event store.
            if (@event.Payload is ContentEvent contentEvent && batch.TrySkip(contentEvent.AppId.Id, contentEvent.SchemaId.Id, contentEvent.ContentId))
            {
                continue;
            }

            indexEvents.Add(@event);
        }

        if (indexEvents.Count > 0)
        {
            var textStates = await QueryStatesAsync(indexEvents);

            var updates = new TextIndexUpdates(textStates, new TextIndexCommands(serializer));

            foreach (var @event in indexEvents)
            {
                updates.On(@event);
            }

            await updates.WriteAsync(textIndex, textIndexerState, extraction, default);
        }
    }

    private Task<Dictionary<UniqueContentId, TextContentState>> QueryStatesAsync(IEnumerable<Envelope<IEvent>> events)
    {
        var ids =
            events
                .Select(x => x.Payload).OfType<ContentEvent>()
                .Select(x => new UniqueContentId(x.AppId.Id, x.ContentId))
                .ToHashSet();

        return textIndexerState.GetAsync(ids);
    }
}
