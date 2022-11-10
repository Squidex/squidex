// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class ContentHistoryEventsCreator : HistoryEventsCreatorBase
{
    public ContentHistoryEventsCreator(TypeRegistry typeRegistry)
        : base(typeRegistry)
    {
        AddEventMessage<ContentCreated>(
            "history.contents.created");

        AddEventMessage<ContentUpdated>(
            "history.contents.updated");

        AddEventMessage<ContentDeleted>(
            "history.contents.deleted");

        AddEventMessage<ContentDraftCreated>(
            "history.contents.draftCreated");

        AddEventMessage<ContentDraftDeleted>(
            "history.contents.draftDeleted");

        AddEventMessage<ContentSchedulingCancelled>(
            "history.contents.scheduleFailed");

        AddEventMessage<ContentStatusChanged>(
            "history.statusChanged");

        AddEventMessage<ContentStatusScheduled>(
            "history.contents.scheduleCompleted");
    }

    protected override Task<HistoryEvent?> CreateEventCoreAsync(Envelope<IEvent> @event)
    {
        HistoryEvent? result = null;

        if (@event.Payload is ContentEvent contentEvent)
        {
            var channel = $"contents.{contentEvent.ContentId}";

            if (@event.Payload is SchemaEvent schemaEvent)
            {
                if (schemaEvent.SchemaId == null)
                {
                    return Task.FromResult<HistoryEvent?>(null);
                }

                channel = $"schemas.{schemaEvent.SchemaId.Id}.{channel}";
            }

            result = ForEvent(@event.Payload, channel);

            if (@event.Payload is SchemaEvent schemaEvent2)
            {
                result = result.Param("Schema", schemaEvent2.SchemaId.Name);
            }

            if (@event.Payload is ContentStatusChanged contentStatusChanged)
            {
                result = result.Param("Status", contentStatusChanged.Status);
            }

            if (@event.Payload is ContentStatusScheduled contentStatusScheduled)
            {
                result = result.Param("Status", contentStatusScheduled.Status);
            }
        }

        return Task.FromResult(result);
    }
}
