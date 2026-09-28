// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Text.Extraction;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class TextIndexingProcess(
    IJsonSerializer serializer,
    ITextIndex textIndex,
    ITextIndexerState textIndexerState,
    IAppProvider appProvider,
    TextExtractor textExtractor,
    IEventStore eventStore,
    IEventFormatter eventFormatter)
    : IEventConsumer, ITextIndexRebuilder
{
    // Serializes the event handling and the rebuilds per app, which both run on the worker node.
    private readonly AsyncKeyedLock<DomainId> appLocks = new AsyncKeyedLock<DomainId>();

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
        Dictionary<ContentData, Dictionary<string, string>?> texts,
        IJsonSerializer serializer)
    {
        private readonly Dictionary<UniqueContentId, TextContentState> currentUpdates = [];
        private readonly Dictionary<(UniqueContentId, byte), IndexCommand> commands = [];

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
            if (@event.Payload is not ContentEvent contentEvent)
            {
                return;
            }

            var uniqueId = new UniqueContentId(contentEvent.AppId.Id, contentEvent.ContentId);

            // The version is only missing in tests, where events are not read from the event store.
            long? version = @event.Headers.ContainsKey(CommonHeaders.EventStreamNumber) ? @event.Headers.EventStreamNumber() : null;

            // A rebuild has already indexed the content up to this version.
            if (version != null && states.TryGetValue(uniqueId, out var existing) && existing.Version >= version)
            {
                return;
            }

            Handle(@event);

            if (version != null && states.TryGetValue(uniqueId, out var state))
            {
                state.Version = version;

                currentUpdates[uniqueId] = state;
            }
        }

        public void PrepareRebuild()
        {
            var contents = new Dictionary<UniqueContentId, NamedId<DomainId>>();

            foreach (var command in commands.Values)
            {
                // The entries already exist, therefore old geo objects and user infos must be deleted.
                if (command is UpsertIndexEntry upsert)
                {
                    upsert.IsNew = false;
                }

                contents[command.UniqueContentId] = command.SchemaId;
            }

            // Delete entries of stages that do not exist anymore.
            foreach (var (uniqueId, schemaId) in contents)
            {
                for (byte stage = 0; stage < 2; stage++)
                {
                    if (!commands.ContainsKey((uniqueId, stage)))
                    {
                        commands[(uniqueId, stage)] = new DeleteIndexEntry { UniqueContentId = uniqueId, SchemaId = schemaId, Stage = stage };
                    }
                }
            }
        }

        private void Handle(Envelope<IEvent> @event)
        {
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
                    GeoObjects = data.ToGeo(serializer),
                    IsNew = true,
                    Stage = 0,
                    ServeAll = true,
                    ServePublished = false,
                    Texts = texts.GetValueOrDefault(data),
                    UserInfos = data.ToUserInfos(),
                });

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
                    GeoObjects = data.ToGeo(serializer),
                    Stage = stage,
                    ServeAll = all,
                    ServePublished = published,
                    Texts = texts.GetValueOrDefault(data),
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

    public async Task ClearAsync()
    {
        await textIndex.ClearAsync();
        await textIndexerState.ClearAsync();
    }

    public async Task On(IEnumerable<Envelope<IEvent>> events)
    {
        // The extraction does not depend on the state, therefore we do not need the locks for it.
        var textValues = await ExtractTextsAsync(events, default);

        // Sort the keys to acquire the locks in a consistent order.
        var appIds =
            events
                .Select(x => x.Payload).OfType<ContentEvent>()
                .Select(x => x.AppId.Id).Distinct()
                .OrderBy(x => x.ToString(), StringComparer.Ordinal)
                .ToList();

        var handles = new List<IDisposable>(appIds.Count);
        try
        {
            // Only wait for rebuilds of the apps in this batch, so that other apps are not affected.
            foreach (var appId in appIds)
            {
                handles.Add(await appLocks.EnterAsync(appId));
            }

            var textStates = await QueryStatesAsync(events);
            var textBatch = new Updates(textStates, textValues, serializer);

            foreach (var @event in events)
            {
                textBatch.On(@event);
            }

            await textBatch.WriteAsync(textIndex, textIndexerState);
        }
        finally
        {
            foreach (var handle in handles)
            {
                handle.Dispose();
            }
        }
    }

    public async Task RebuildAsync(DomainId appId, IReadOnlyCollection<DomainId> contentIds,
        CancellationToken ct = default)
    {
        var versions = new Dictionary<DomainId, long>();

        // Reading the events and extracting the texts is the expensive part, therefore we do it without the lock.
        var events = await ReadEventsAsync(appId, contentIds, versions, ct);

        var textValues = await ExtractTextsAsync(events, ct);
        var textBatch = new Updates([], textValues, serializer);

        // Replay the full history without the persisted state to calculate the entries from scratch.
        foreach (var @event in events)
        {
            textBatch.On(@event);
        }

        using (await appLocks.EnterAsync(appId, ct))
        {
            // Events could have been added in the meantime, which could already have been handled by the event consumer.
            var newEvents = await ReadEventsAsync(appId, contentIds, versions, ct);

            if (newEvents.Count > 0)
            {
                foreach (var (data, texts) in await ExtractTextsAsync(newEvents, ct))
                {
                    textValues[data] = texts;
                }

                foreach (var @event in newEvents)
                {
                    textBatch.On(@event);
                }
            }

            textBatch.PrepareRebuild();

            // The versions in the state ensure that the event consumer skips the events that have been replayed.
            await textBatch.WriteAsync(textIndex, textIndexerState);
        }
    }

    private async Task<List<Envelope<IEvent>>> ReadEventsAsync(DomainId appId, IReadOnlyCollection<DomainId> contentIds, Dictionary<DomainId, long> versions,
        CancellationToken ct)
    {
        var streams = await Task.WhenAll(contentIds.Select(async contentId =>
        {
            var streamName = $"content-{DomainId.Combine(appId, contentId)}";

            // Only read the events after the version that has been read before.
            var storedEvents = await eventStore.QueryStreamAsync(streamName, versions.GetValueOrDefault(contentId, EtagVersion.Empty), ct);

            return (contentId, storedEvents);
        }));

        var result = new List<Envelope<IEvent>>();

        foreach (var (contentId, storedEvents) in streams)
        {
            foreach (var storedEvent in storedEvents)
            {
                var @event = eventFormatter.ParseIfKnown(storedEvent);

                if (@event != null)
                {
                    result.Add(@event);
                }

                versions[contentId] = storedEvent.EventStreamNumber;
            }
        }

        return result;
    }

    private async Task<Dictionary<ContentData, Dictionary<string, string>?>> ExtractTextsAsync(IEnumerable<Envelope<IEvent>> events,
        CancellationToken ct)
    {
        // Compare by reference, because the events hold the instances and value equality is expensive.
        var result = new Dictionary<ContentData, Dictionary<string, string>?>(ReferenceEqualityComparer.Instance);

        var schemas = new Dictionary<(DomainId AppId, DomainId SchemaId), (Schema? Schema, ResolvedComponents Components)>();

        foreach (var @event in events.Select(x => x.Payload).OfType<ContentEvent>())
        {
            foreach (var data in GetData(@event))
            {
                if (result.ContainsKey(data))
                {
                    continue;
                }

                (DomainId AppId, DomainId SchemaId) key = (@event.AppId.Id, @event.SchemaId.Id);

                if (!schemas.TryGetValue(key, out var schema))
                {
                    schema = await GetSchemaAsync(key.AppId, key.SchemaId, ct);

                    schemas[key] = schema;
                }

                var context = new TextExtractionContext
                {
                    AppId = @event.AppId,
                    Components = schema.Components,
                    ContentId = @event.ContentId,
                    Data = data,
                    Schema = schema.Schema,
                    SchemaId = @event.SchemaId,
                };

                result[data] = await textExtractor.ExtractAsync(context, ct);
            }
        }

        return result;
    }

    private async Task<(Schema?, ResolvedComponents)> GetSchemaAsync(DomainId appId, DomainId schemaId,
        CancellationToken ct)
    {
        var schema = await appProvider.GetSchemaAsync(appId, schemaId, true, ct);

        if (schema == null)
        {
            return (null, ResolvedComponents.Empty);
        }

        var components = await appProvider.GetComponentsAsync(schema, ct);

        return (schema, components);
    }

    private static IEnumerable<ContentData> GetData(ContentEvent @event)
    {
        switch (@event)
        {
            case ContentCreated created:
                yield return created.Data;
                break;
            case ContentUpdated updated:
                yield return updated.Data;
                break;
            case ContentDraftCreated { MigratedData: not null } draftCreated:
                yield return draftCreated.MigratedData;
                break;
            case ContentMigrated migrated:
                if (migrated.Data != null)
                {
                    yield return migrated.Data;
                }

                if (migrated.NewData != null)
                {
                    yield return migrated.NewData;
                }

                break;
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
