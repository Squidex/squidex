// ==========================================================================
//  ContentState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Contents.State
{
    public class ContentState : DomainObjectState<ContentState>, IContentEntity
    {
        [JsonProperty]
        public NamedContentData Data { get; set; }

        [JsonProperty]
        public Guid AppId { get; set; }

        [JsonProperty]
        public Guid SchemaId { get; set; }

        [JsonProperty]
        public Status Status { get; set; }

        [JsonProperty]
        public bool IsDeleted { get; set; }

        protected void On(ContentCreated @event)
        {
            SchemaId = @event.SchemaId.Id;

            Data = @event.Data;
        }

        protected void On(ContentUpdated @event)
        {
            Data = @event.Data;
        }

        protected void On(ContentStatusChanged @event)
        {
            Status = @event.Status;
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
