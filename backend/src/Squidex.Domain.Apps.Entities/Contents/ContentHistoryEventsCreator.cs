﻿// ==========================================================================
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

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentHistoryEventsCreator : HistoryEventsCreatorBase
    {
        public ContentHistoryEventsCreator(TypeNameRegistry typeNameRegistry)
            : base(typeNameRegistry)
        {
            AddEventMessage<ContentCreated>(
                "created {[Schema]} content.");

            AddEventMessage<ContentUpdated>(
                "updated {[Schema]} content.");

            AddEventMessage<ContentDeleted>(
                "deleted {[Schema]} content.");

            AddEventMessage<ContentDraftCreated>(
                "created new draft.");

            AddEventMessage<ContentDraftDeleted>(
                "deleted draft.");

            AddEventMessage<ContentSchedulingCancelled>(
                "failed to schedule status change for {[Schema]} content.");

            AddEventMessage<ContentStatusChanged>(
                "changed status of {[Schema]} content to {[Status]}.");

            AddEventMessage<ContentStatusScheduled>(
                "scheduled to change status of {[Schemra]} content to {[Status]}.");
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
}
