// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentHistoryEventsCreator : HistoryEventsCreatorBase
    {
        public ContentHistoryEventsCreator(TypeNameRegistry typeNameRegistry)
            : base(typeNameRegistry)
        {
            AddEventMessage<ContentCreated>(
                T.Get("history.contents.created"));

            AddEventMessage<ContentUpdated>(
                T.Get("history.contents.updated"));

            AddEventMessage<ContentDeleted>(
                T.Get("history.contents.deleted"));

            AddEventMessage<ContentDraftCreated>(
                T.Get("history.contents.draftCreated"));

            AddEventMessage<ContentDraftDeleted>(
                T.Get("history.contents.draftDeleted"));

            AddEventMessage<ContentSchedulingCancelled>(
                T.Get("history.contents.scheduleFailed"));

            AddEventMessage<ContentStatusChanged>(
                T.Get("history.statusChanged"));

            AddEventMessage<ContentStatusScheduled>(
                T.Get("history.contents.scheduleCompleted"));
        }

        protected override Task<HistoryEvent?> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            var channel = $"contents.{@event.Headers.AggregateId()}";

            if (@event.Payload is SchemaEvent schemaEvent)
            {
                if (schemaEvent.SchemaId == null)
                {
                    return Task.FromResult<HistoryEvent?>(null);
                }

                channel = $"schemas.{schemaEvent.SchemaId.Id}.{channel}";
            }

            var result = ForEvent(@event.Payload, channel);

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

            return Task.FromResult<HistoryEvent?>(result);
        }
    }
}
