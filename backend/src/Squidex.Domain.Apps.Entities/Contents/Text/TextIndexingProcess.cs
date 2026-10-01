// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
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

    private sealed class Updates(
        Dictionary<UniqueContentId, TextContentState> states,
        IJsonSerializer serializer,
        bool isRebuild)
    {
        private readonly Dictionary<UniqueContentId, TextContentState> currentUpdates = [];
        private readonly Dictionary<(UniqueContentId, byte), IndexCommand> commands = [];
        private readonly Dictionary<UpsertIndexEntry, (ContentEvent Event, ContentData Data)> sources = [];

        public async Task WriteAsync(ITextIndex textIndex, ITextIndexerState textIndexerState, TextIndexExtraction extraction,
            CancellationToken ct)
        {
            // Only calculate the values for the final versions, the other versions have been replaced within the batch.
            var upserts = commands.Values.OfType<UpsertIndexEntry>().ToList();
            if (upserts.Count > 0)
            {
                var texts = await extraction.ExtractAsync(upserts.Select(x => sources[x]), ct);

                foreach (var upsert in upserts)
                {
                    var data = sources[upsert].Data;
                    var text = texts.GetValueOrDefault(data);

                    upsert.GeoObjects = data.ToGeo(serializer);
                    upsert.Texts = text?.Texts;
                    upsert.Titles = text?.Titles;
                }
            }

            if (commands.Count > 0)
            {
                await textIndex.ExecuteAsync(commands.Values.ToArray());
            }

            if (currentUpdates.Count > 0)
            {
                await textIndexerState.SetAsync(currentUpdates.Values.ToList());
            }
        }

        public void On(Envelope<IEvent> @event)
        {
            if (@event.Payload is not ContentEvent contentEvent)
            {
                return;
            }

            var uniqueId = new UniqueContentId(contentEvent.AppId.Id, contentEvent.ContentId);

            var version = @event.Headers.EventStreamNumber();

            // A rebuild has already indexed the content up to this version. The rebuild itself replays everything from the start.
            if (!isRebuild && states.TryGetValue(uniqueId, out var existing) && existing.Version >= version)
            {
                return;
            }

            switch (@event.Payload)
            {
                case ContentCreated created:
                    Create(created, created.Data);
                    break;
                case ContentUpdated updated:
                    Update(updated, updated.Data);
                    break;
                case ContentMigrated migrated:
                    Migrate(migrated);
                    break;
                case ContentStatusChanged statusChanged when statusChanged.Status == Status.Published:
                    Publish(statusChanged);
                    break;
                case ContentStatusChanged statusChanged:
                    Unpublish(statusChanged);
                    break;
                case ContentDraftDeleted draftDelted:
                    DeleteDraft(draftDelted);
                    break;
                case ContentDeleted deleted:
                    Delete(deleted);
                    break;
                case ContentDraftCreated draftCreated:
                    {
                        CreateDraft(draftCreated);

                        if (draftCreated.MigratedData != null)
                        {
                            Update(draftCreated, draftCreated.MigratedData);
                        }
                    }

                    break;
            }

            if (states.TryGetValue(uniqueId, out var state))
            {
                state.Version = version;

                currentUpdates[uniqueId] = state;
            }
        }

        private void Create(ContentEvent @event, ContentData data)
        {
            var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);

            var state = new TextContentState
            {
                UniqueContentId = uniqueId,
            };

            Index(@event,
                new UpsertIndexEntry
                {
                    UniqueContentId = uniqueId,
                    IsNew = !isRebuild,
                    Stage = 0,
                    ServeAll = true,
                    ServePublished = false,
                    UserInfos = data.ToUserInfos(),
                },
                data);

            // The entries of a rebuild already exist, therefore we also delete the old draft.
            if (isRebuild)
            {
                CoreDelete(@event, uniqueId, 1);
            }

            states[state.UniqueContentId] = state;
            currentUpdates[state.UniqueContentId] = state;
        }

        private void CreateDraft(ContentEvent @event)
        {
            var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);

            if (states.TryGetValue(uniqueId, out var state))
            {
                switch (state.State)
                {
                    case TextState.Stage0_Published__Stage1_None:
                        state.State = TextState.Stage0_Published__Stage1_Draft;
                        break;
                    case TextState.Stage1_Published__Stage0_None:
                        state.State = TextState.Stage1_Published__Stage0_Draft;
                        break;
                }

                currentUpdates[state.UniqueContentId] = state;
            }
        }

        private void Unpublish(ContentEvent @event)
        {
            var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);

            if (states.TryGetValue(uniqueId, out var state))
            {
                switch (state.State)
                {
                    case TextState.Stage0_Published__Stage1_None:
                        CoreUpdate(@event, uniqueId, 0, true, false);

                        state.State = TextState.Stage0_Draft__Stage1_None;
                        break;
                    case TextState.Stage1_Published__Stage0_None:
                        CoreUpdate(@event, uniqueId, 1, true, false);

                        state.State = TextState.Stage1_Draft__Stage0_None;
                        break;
                }

                currentUpdates[state.UniqueContentId] = state;
            }
        }

        private void Update(ContentEvent @event, ContentData data)
        {
            var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);

            if (states.TryGetValue(uniqueId, out var state))
            {
                switch (state.State)
                {
                    case TextState.Stage0_Draft__Stage1_None:
                        CoreUpsert(@event, uniqueId, 0, true, false, data);
                        break;
                    case TextState.Stage0_Published__Stage1_None:
                        CoreUpsert(@event, uniqueId, 0, true, true, data);
                        break;
                    case TextState.Stage0_Published__Stage1_Draft:
                        CoreUpsert(@event, uniqueId, 1, true, false, data);
                        CoreUpdate(@event, uniqueId, 0, false, true);
                        break;
                    case TextState.Stage1_Draft__Stage0_None:
                        CoreUpsert(@event, uniqueId, 1, true, false, data);
                        break;
                    case TextState.Stage1_Published__Stage0_None:
                        CoreUpsert(@event, uniqueId, 1, true, true, data);
                        break;
                    case TextState.Stage1_Published__Stage0_Draft:
                        CoreUpsert(@event, uniqueId, 0, true, false, data);
                        CoreUpdate(@event, uniqueId, 1, false, true);
                        break;
                }

                currentUpdates[state.UniqueContentId] = state;
            }
        }

        private void Migrate(ContentMigrated @event)
        {
            var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);

            if (states.TryGetValue(uniqueId, out var state))
            {
                // The current version is stored in the published stage, if a draft exists. Otherwise it is the only stage.
                switch (state.State)
                {
                    case TextState.Stage0_Draft__Stage1_None when @event.Data != null:
                        CoreUpsert(@event, uniqueId, 0, true, false, @event.Data);
                        break;
                    case TextState.Stage0_Published__Stage1_None when @event.Data != null:
                        CoreUpsert(@event, uniqueId, 0, true, true, @event.Data);
                        break;
                    case TextState.Stage1_Draft__Stage0_None when @event.Data != null:
                        CoreUpsert(@event, uniqueId, 1, true, false, @event.Data);
                        break;
                    case TextState.Stage1_Published__Stage0_None when @event.Data != null:
                        CoreUpsert(@event, uniqueId, 1, true, true, @event.Data);
                        break;
                    case TextState.Stage0_Published__Stage1_Draft:
                        if (@event.Data != null)
                        {
                            CoreUpsert(@event, uniqueId, 0, false, true, @event.Data);
                        }

                        if (@event.NewData != null)
                        {
                            CoreUpsert(@event, uniqueId, 1, true, false, @event.NewData);
                        }

                        break;
                    case TextState.Stage1_Published__Stage0_Draft:
                        if (@event.Data != null)
                        {
                            CoreUpsert(@event, uniqueId, 1, false, true, @event.Data);
                        }

                        if (@event.NewData != null)
                        {
                            CoreUpsert(@event, uniqueId, 0, true, false, @event.NewData);
                        }

                        break;
                }

                currentUpdates[state.UniqueContentId] = state;
            }
        }

        private void Publish(ContentEvent @event)
        {
            var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);

            if (states.TryGetValue(uniqueId, out var state))
            {
                switch (state.State)
                {
                    case TextState.Stage0_Published__Stage1_Draft:
                        CoreUpdate(@event, uniqueId, 1, true, true);
                        CoreDelete(@event, uniqueId, 0);

                        state.State = TextState.Stage1_Published__Stage0_None;
                        break;
                    case TextState.Stage1_Published__Stage0_Draft:
                        CoreUpdate(@event, uniqueId, 0, true, true);
                        CoreDelete(@event, uniqueId, 1);

                        state.State = TextState.Stage0_Published__Stage1_None;
                        break;
                    case TextState.Stage0_Draft__Stage1_None:
                        CoreUpdate(@event, uniqueId, 0, true, true);

                        state.State = TextState.Stage0_Published__Stage1_None;
                        break;
                    case TextState.Stage1_Draft__Stage0_None:
                        CoreUpdate(@event, uniqueId, 1, true, true);

                        state.State = TextState.Stage1_Published__Stage0_None;
                        break;
                }

                currentUpdates[state.UniqueContentId] = state;
            }
        }

        private void DeleteDraft(ContentEvent @event)
        {
            var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);

            if (states.TryGetValue(uniqueId, out var state))
            {
                switch (state.State)
                {
                    case TextState.Stage0_Published__Stage1_Draft:
                        CoreUpdate(@event, uniqueId, 0, true, true);
                        CoreDelete(@event, uniqueId, 1);

                        state.State = TextState.Stage0_Published__Stage1_None;
                        break;
                    case TextState.Stage1_Published__Stage0_Draft:
                        CoreUpdate(@event, uniqueId, 1, true, true);
                        CoreDelete(@event, uniqueId, 0);

                        state.State = TextState.Stage1_Published__Stage0_None;
                        break;
                }

                currentUpdates[state.UniqueContentId] = state;
            }
        }

        private void Delete(ContentEvent @event)
        {
            var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);

            if (states.TryGetValue(uniqueId, out var state))
            {
                CoreDelete(@event, uniqueId, 0);
                CoreDelete(@event, uniqueId, 1);

                state.State = TextState.Deleted;

                currentUpdates[state.UniqueContentId] = state;
            }
        }

        private void CoreUpsert(ContentEvent @event, UniqueContentId uniqueId, byte stage, bool all, bool published, ContentData data)
        {
            Index(@event,
                new UpsertIndexEntry
                {
                    UniqueContentId = uniqueId,
                    Stage = stage,
                    ServeAll = all,
                    ServePublished = published,
                },
                data);
        }

        private void CoreUpdate(ContentEvent @event, UniqueContentId uniqueId, byte stage, bool all, bool published)
        {
            Index(@event,
                new UpdateIndexEntry
                {
                    UniqueContentId = uniqueId,
                    Stage = stage,
                    ServeAll = all,
                    ServePublished = published,
                });
        }

        private void CoreDelete(ContentEvent @event, UniqueContentId uniqueId, byte stage)
        {
            Index(@event,
                new DeleteIndexEntry
                {
                    UniqueContentId = uniqueId,
                    Stage = stage,
                });
        }

        private void Index(ContentEvent @event, IndexCommand command, ContentData? data = null)
        {
            command.SchemaId = @event.SchemaId;

            var key = (command.UniqueContentId, command.Stage);

            if (command is UpsertIndexEntry newUpsert && data != null)
            {
                sources[newUpsert] = (@event, data);
            }

            if (command is UpdateIndexEntry update &&
                commands.TryGetValue(key, out var existing) &&
                existing is UpsertIndexEntry upsert)
            {
                upsert.ServeAll = update.ServeAll;
                upsert.ServePublished = update.ServePublished;
            }
            else
            {
                commands[key] = command;
            }
        }
    }

    public async Task ClearAsync()
    {
        await textIndex.ClearAsync();
        await textIndexerState.ClearAsync();
    }

    public async Task On(IEnumerable<Envelope<IEvent>> events)
    {
        // The rebuild job cannot hand back the app while the batch is handled.
        using var batch = await coordinator.BeginBatchAsync();

        var indexEvents = new List<Envelope<IEvent>>();

        foreach (var @event in events)
        {
            // The events of apps that are rebuilt are replayed by the rebuild job.
            if (@event.Payload is ContentEvent contentEvent && batch.TrySkip(contentEvent.AppId.Id))
            {
                continue;
            }

            indexEvents.Add(@event);
        }

        await ApplyAsync(indexEvents, false);
    }

    public async Task ApplyAsync(List<Envelope<IEvent>> events, bool isRebuild,
        CancellationToken ct = default)
    {
        var textStates = await QueryStatesAsync(events);
        var textBatch = new Updates(textStates, serializer, isRebuild);

        foreach (var @event in events)
        {
            textBatch.On(@event);
        }

        await textBatch.WriteAsync(textIndex, textIndexerState, extraction, ct);
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
