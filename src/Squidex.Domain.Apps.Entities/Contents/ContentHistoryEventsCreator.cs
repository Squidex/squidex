﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentHistoryEventsCreator : HistoryEventsCreatorBase
    {
        public ContentHistoryEventsCreator(TypeNameRegistry typeNameRegistry)
            : base(typeNameRegistry)
        {
            AddEventMessage<ContentCreated>(
                "created content item.");

            AddEventMessage<ContentUpdated>(
                "updated content item.");

            AddEventMessage<ContentDeleted>(
                "deleted content item.");

            AddEventMessage<ContentStatusChanged>(
                "changed status of content item to {[Status]}.");
        }

        protected override Task<HistoryEventToStore> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            var channel = $"contents.{@event.Headers.AggregateId()}";

            var result = ForEvent(@event.Payload, channel);

            if (@event.Payload is ContentStatusChanged contentStatusChanged)
            {
                result = result.AddParameter("Status", contentStatusChanged.Status);
            }

            return Task.FromResult(result);
        }
    }
}
