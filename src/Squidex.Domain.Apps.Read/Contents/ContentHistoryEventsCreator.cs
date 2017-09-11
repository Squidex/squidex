// ==========================================================================
//  ContentHistoryEventsCreator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Read.History;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Read.Contents
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

            AddEventMessage<ContentRestored>(
                "restored content item.");

            AddEventMessage<ContentPublished>(
                "published content item.");

            AddEventMessage<ContentUnpublished>(
                "unpublished content item.");
        }

        protected override Task<HistoryEventToStore> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            var channel = $"contents.{@event.Headers.AggregateId()}";

            return Task.FromResult(ForEvent(@event.Payload, channel));
        }
    }
}
