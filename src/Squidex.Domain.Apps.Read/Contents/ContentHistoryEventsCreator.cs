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
using Squidex.Infrastructure.EventSourcing;

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
