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
                "created content element.");

            AddEventMessage<ContentUpdated>(
                "updated content element.");

            AddEventMessage<ContentDeleted>(
                "deleted content element.");

            AddEventMessage<ContentPublished>(
                "published content element.");

            AddEventMessage<ContentUnpublished>(
                "unpublished content element.");
        }

        protected override Task<HistoryEventToStore> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            var channel = $"contents.{@event.Headers.AggregateId()}";

            return Task.FromResult(ForEvent(@event.Payload, channel));
        }
    }
}
