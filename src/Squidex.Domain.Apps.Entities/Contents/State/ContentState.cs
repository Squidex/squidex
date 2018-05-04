// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Contents.State
{
    public class ContentState : DomainObjectState<ContentState>, IContentEntity
    {
        [JsonProperty]
        public NamedId<Guid> AppId { get; set; }

        [JsonProperty]
        public NamedId<Guid> SchemaId { get; set; }

        [JsonProperty]
        public NamedContentData Data { get; set; }

        [JsonProperty]
        public Status Status { get; set; }

        [JsonProperty]
        public Status? ScheduledTo { get; set; }

        [JsonProperty]
        public Instant? ScheduledAt { get; set; }

        [JsonProperty]
        public RefToken ScheduledBy { get; set; }

        [JsonProperty]
        public bool IsDeleted { get; set; }

        protected void On(ContentCreated @event)
        {
            SchemaId = @event.SchemaId;

            Data = @event.Data;

            AppId = @event.AppId;
        }

        protected void On(ContentUpdated @event)
        {
            Data = @event.Data;
        }

        protected void On(ContentStatusScheduled @event)
        {
            ScheduledAt = @event.DueTime;
            ScheduledBy = @event.Actor;
            ScheduledTo = @event.Status;
        }

        protected void On(ContentStatusChanged @event)
        {
            Status = @event.Status;

            ScheduledAt = null;
            ScheduledBy = null;
            ScheduledTo = null;
        }

        protected void On(ContentDeleted @event)
        {
            IsDeleted = true;
        }

        public ContentState Apply(Envelope<IEvent> @event)
        {
            var payload = (SquidexEvent)@event.Payload;

            return Clone().Update(payload, @event.Headers, r => r.DispatchAction(payload));
        }
    }
}
