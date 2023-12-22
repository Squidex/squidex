// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class TextIndexingProcess : IEventConsumer
{
    private readonly IJsonSerializer serializer;
    private readonly ITextIndex textIndex;
    private readonly ITextIndexerState textIndexerState;

    public int BatchSize => 1000;

    public int BatchDelay => 1000;

    public string Name => "TextIndexer6";

    public StreamFilter EventsFilter { get; } = StreamFilter.Prefix("content-");

    public ITextIndex TextIndex
    {
        get => textIndex;
    }

    private sealed class Updates
    {
        private readonly Dictionary<UniqueContentId, TextContentState> currentState;
        private readonly Dictionary<UniqueContentId, TextContentState> currentUpdates;
        private readonly Dictionary<(UniqueContentId, byte), IndexCommand> commands = [];
        private readonly IJsonSerializer serializer;

        public Updates(Dictionary<UniqueContentId, TextContentState> states, IJsonSerializer serializer)
        {
            currentState = states;
            currentUpdates = [];
            this.serializer = serializer;
        }

        public async Task WriteAsync(ITextIndex textIndex, ITextIndexerState textIndexerState)
        {
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
            switch (@event.Payload)
            {
                case ContentCreated created:
                    Create(created, created.Data);
                    break;
                case ContentUpdated updated:
                    Update(updated, updated.Data);
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
        }

        private void Create(ContentEvent @event, ContentData data)
        {
            var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);

            var state = new TextContentState
            {
                UniqueContentId = uniqueId
            };

            Index(@event,
                new UpsertIndexEntry
                {
                    UniqueContentId = uniqueId,
                    GeoObjects = data.ToGeo(serializer),
                    IsNew = true,
                    Stage = 0,
                    ServeAll = true,
                    ServePublished = false,
                    Texts = data.ToTexts(),
                });

            currentState[state.UniqueContentId] = state;
            currentUpdates[state.UniqueContentId] = state;
        }

        private void CreateDraft(ContentEvent @event)
        {
            var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);

            if (currentState.TryGetValue(uniqueId, out var state))
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

            if (currentState.TryGetValue(uniqueId, out var state))
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

            if (currentState.TryGetValue(uniqueId, out var state))
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

        private void Publish(ContentEvent @event)
        {
            var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);

            if (currentState.TryGetValue(uniqueId, out var state))
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

            if (currentState.TryGetValue(uniqueId, out var state))
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

            if (currentState.TryGetValue(uniqueId, out var state))
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
                    GeoObjects = data.ToGeo(serializer),
                    Stage = stage,
                    ServeAll = all,
                    ServePublished = published,
                    Texts = data.ToTexts()
                });
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

        private void Index(ContentEvent @event, IndexCommand command)
        {
            command.SchemaId = @event.SchemaId;

            var key = (command.UniqueContentId, command.Stage);

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

    public TextIndexingProcess(
        IJsonSerializer serializer,
        ITextIndex textIndex,
        ITextIndexerState textIndexerState)
    {
        this.serializer = serializer;
        this.textIndex = textIndex;
        this.textIndexerState = textIndexerState;
    }

    public async Task ClearAsync()
    {
        await textIndex.ClearAsync();
        await textIndexerState.ClearAsync();
    }

    public async Task On(IEnumerable<Envelope<IEvent>> events)
    {
        var states = await QueryStatesAsync(events);

        var updates = new Updates(states, serializer);

        foreach (var @event in events)
        {
            updates.On(@event);
        }

        await updates.WriteAsync(textIndex, textIndexerState);
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
