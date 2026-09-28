// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Events;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

internal sealed partial class TextIndexUpdates(
    Dictionary<UniqueContentId, TextContentState> states,
    TextIndexCommands commands,
    bool isRebuild = false)
{
    private readonly Dictionary<UniqueContentId, TextContentState> currentUpdates = [];

    public TextIndexCommands Commands => commands;

    public async Task WriteAsync(ITextIndex textIndex, ITextIndexerState textIndexerState, TextIndexExtraction extraction,
        CancellationToken ct)
    {
        await commands.WriteAsync(textIndex, extraction, ct);

        if (currentUpdates.Count > 0)
        {
            await textIndexerState.SetAsync(currentUpdates.Values.ToList(), ct);

            currentUpdates.Clear();
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

        // A rebuild has already indexed the content up to this version. The rebuild itself replays everything from the start.
        if (!isRebuild && version != null && states.TryGetValue(uniqueId, out var existing) && existing.Version >= version)
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

        if (isRebuild)
        {
            // The entries already exist, therefore we have to replace them.
            commands.Create(@event, uniqueId, data, false);
            commands.Delete(@event, uniqueId, 1);
        }
        else
        {
            commands.Create(@event, uniqueId, data, true);
        }

        states[state.UniqueContentId] = state;
        currentUpdates[state.UniqueContentId] = state;
    }

    private void Update(ContentEvent @event, ContentData data)
    {
        var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);
        if (states.TryGetValue(uniqueId, out var state))
        {
            switch (state.State)
            {
                case TextState.Stage0_Draft__Stage1_None:
                    commands.Upsert(@event, uniqueId, 0, true, false, data);
                    break;
                case TextState.Stage0_Published__Stage1_None:
                    commands.Upsert(@event, uniqueId, 0, true, true, data);
                    break;
                case TextState.Stage0_Published__Stage1_Draft:
                    commands.Upsert(@event, uniqueId, 1, true, false, data);
                    commands.Update(@event, uniqueId, 0, false, true);
                    break;
                case TextState.Stage1_Draft__Stage0_None:
                    commands.Upsert(@event, uniqueId, 1, true, false, data);
                    break;
                case TextState.Stage1_Published__Stage0_None:
                    commands.Upsert(@event, uniqueId, 1, true, true, data);
                    break;
                case TextState.Stage1_Published__Stage0_Draft:
                    commands.Upsert(@event, uniqueId, 0, true, false, data);
                    commands.Update(@event, uniqueId, 1, false, true);
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
                    commands.Upsert(@event, uniqueId, 0, true, false, @event.Data);
                    break;
                case TextState.Stage0_Published__Stage1_None when @event.Data != null:
                    commands.Upsert(@event, uniqueId, 0, true, true, @event.Data);
                    break;
                case TextState.Stage1_Draft__Stage0_None when @event.Data != null:
                    commands.Upsert(@event, uniqueId, 1, true, false, @event.Data);
                    break;
                case TextState.Stage1_Published__Stage0_None when @event.Data != null:
                    commands.Upsert(@event, uniqueId, 1, true, true, @event.Data);
                    break;
                case TextState.Stage0_Published__Stage1_Draft:
                    if (@event.Data != null)
                    {
                        commands.Upsert(@event, uniqueId, 0, false, true, @event.Data);
                    }

                    if (@event.NewData != null)
                    {
                        commands.Upsert(@event, uniqueId, 1, true, false, @event.NewData);
                    }

                    break;
                case TextState.Stage1_Published__Stage0_Draft:
                    if (@event.Data != null)
                    {
                        commands.Upsert(@event, uniqueId, 1, false, true, @event.Data);
                    }

                    if (@event.NewData != null)
                    {
                        commands.Upsert(@event, uniqueId, 0, true, false, @event.NewData);
                    }

                    break;
            }

            currentUpdates[state.UniqueContentId] = state;
        }
    }
}
